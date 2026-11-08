using System.Data.Common;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WhatToCook.Api.Domain.Recipes;
using WhatToCook.Api.Infrastructure.Data;
using Xunit.Abstractions;

namespace WhatToCook.Api.IntegrationTests.Specs;

public sealed class RecipeSubstringSearchDiagnosticsTests(
    TestWebApplicationFactory factory,
    ITestOutputHelper output
) : IClassFixture<TestWebApplicationFactory>
{
    private const string IndexName = "perf03_recipe_title_trgm_test_idx";

    [Fact]
    public async Task SubstringSearch_ComparePlansAtOneAndTenThousandRecipes()
    {
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var term = "kurkuma";
        var query = db.Recipes.AsNoTracking().Where(x => x.IsActive);
        var efSql = query
            .Where(x => x.NormalizedTitle.Contains(term))
            .OrderBy(x => x.NormalizedTitle)
            .ThenBy(x => x.Title)
            .ThenBy(x => x.Id)
            .Take(50)
            .Select(x => x.Id)
            .ToQueryString();

        // This is the SQL operator shape that a pg_trgm index can support.
        // Keep the diagnostic honest if a future provider changes the translation.
        Assert.Contains("LIKE", efSql);
        output.WriteLine($"PERF-03 generated EF query: {efSql}");

        await db.Database.OpenConnectionAsync();
        try
        {
            await InsertSyntheticRecipesAsync(db, 1, 1000);
            await db.Database.ExecuteSqlRawAsync("ANALYZE recipes");

            var smallBefore = await CaptureAsync(db, 1000, "kurkuma", "no-gin");
            await db.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS pg_trgm");
            await CreateTestIndexAsync(db);
            var smallAfter = await CaptureAsync(db, 1000, "kurkuma", "with-gin");
            AssertSameResults(smallBefore, smallAfter);

            await InsertSyntheticRecipesAsync(db, 1001, 10000);
            await db.Database.ExecuteSqlRawAsync("ANALYZE recipes");

            // Measure the same 10k-row catalog in both states, not separate data.
            await db.Database.ExecuteSqlRawAsync($"DROP INDEX {IndexName}");
            await db.Database.ExecuteSqlRawAsync("ANALYZE recipes");
            var largeBefore = await CaptureAsync(db, 10000, "kurkuma", "no-gin");
            await CreateTestIndexAsync(db);
            var largeAfter = await CaptureAsync(db, 10000, "kurkuma", "with-gin");
            AssertSameResults(largeBefore, largeAfter);

            // Broad and short searches can favor a sequential scan even with GIN.
            foreach (var needle in new[] { "domowe", "zu" })
            {
                var indexed = await CaptureAsync(db, 10000, needle, "with-gin");
                await db.Database.ExecuteSqlRawAsync($"DROP INDEX {IndexName}");
                var unindexed = await CaptureAsync(db, 10000, needle, "no-gin");
                AssertSameResults(unindexed, indexed);
                await CreateTestIndexAsync(db);
            }

            using var response = await client.GetAsync(
                "/api/recipes/search?title=kurkuma&pageSize=50"
            );
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var page = await response.Content.ReadFromJsonAsync<
                List<RecipeApiContracts.RecipeSummaryDto>
            >();
            Assert.NotNull(page);
            Assert.Equal(50, page.Count);
            Assert.All(page, x => Assert.Contains("kurkuma", x.Title));
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }

    private static async Task InsertSyntheticRecipesAsync(
        AppDbContext db,
        int fromInclusive,
        int toInclusive
    )
    {
        // One statement avoids EF tracking or 10k HTTP writes dominating the probe.
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO recipes
                ("Id", "Title", "NormalizedTitle", "Servings", "IsActive", "CreatedAt", "UpdatedAt")
            SELECT
                gen_random_uuid(),
                'Profilowy przepis ' || lpad(n::text, 6, '0') ||
                    CASE
                        WHEN n % 100 = 0 THEN ' kurkuma w farszu'
                        WHEN n % 4 = 0 THEN ' zupa warzywna'
                        ELSE ' domowe danie'
                    END,
                'profilowy przepis ' || lpad(n::text, 6, '0') ||
                    CASE
                        WHEN n % 100 = 0 THEN ' kurkuma w farszu'
                        WHEN n % 4 = 0 THEN ' zupa warzywna'
                        ELSE ' domowe danie'
                    END,
                4, true, now(), now()
            FROM generate_series({fromInclusive}, {toInclusive}) AS series(n)
            """
        );
    }

    private static async Task CreateTestIndexAsync(AppDbContext db)
    {
        // Exists only in the throwaway Testcontainers database, never in migrations.
        await db.Database.ExecuteSqlRawAsync(
            $"CREATE INDEX {IndexName} ON recipes USING GIN (\"NormalizedTitle\" gin_trgm_ops)"
        );
        await db.Database.ExecuteSqlRawAsync("ANALYZE recipes");
    }

    private async Task<Probe> CaptureAsync(AppDbContext db, int size, string needle, string variant)
    {
        var query = db
            .Recipes.AsNoTracking()
            .Where(x => x.IsActive && x.NormalizedTitle.Contains(needle));

        var count = await query.CountAsync();
        var ids = await query
            .OrderBy(x => x.NormalizedTitle)
            .ThenBy(x => x.Title)
            .ThenBy(x => x.Id)
            .Take(50)
            .Select(x => x.Id)
            .ToArrayAsync();

        var countPlan = await ExplainAsync(db, needle, countQuery: true);
        var pagePlan = await ExplainAsync(db, needle, countQuery: false);
        output.WriteLine(
            $"PERF-03 size={size} phrase={needle} index={variant} results={count}"
        );
        output.WriteLine($"PERF-03 COUNT {Format(countPlan)}");
        output.WriteLine($"PERF-03 PAGE {Format(pagePlan)}");

        return new Probe(count, ids, countPlan, pagePlan);
    }

    private static string Format(PlanInfo plan) =>
        $"time={plan.Ms:F2}ms hits={plan.HitBlocks} reads={plan.ReadBlocks} scans={plan.Scans}";

    private static void AssertSameResults(Probe before, Probe after)
    {
        Assert.Equal(before.Count, after.Count);
        Assert.Equal(before.Ids, after.Ids);
    }

    private static async Task<PlanInfo> ExplainAsync(
        AppDbContext db,
        string needle,
        bool countQuery
    )
    {
        var select = countQuery
            ? "SELECT count(*) FROM recipes"
            : """SELECT "Id" FROM recipes""";
        var suffix = countQuery
            ? string.Empty
            : """ ORDER BY "NormalizedTitle", "Title", "Id" LIMIT 50""";

        await using DbCommand command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText =
            $"""
            EXPLAIN (ANALYZE, BUFFERS, FORMAT JSON)
            {select}
            WHERE "IsActive" AND "NormalizedTitle" LIKE '%' || @needle || '%'
            {suffix}
            """;
        var parameter = command.CreateParameter();
        parameter.ParameterName = "needle";
        parameter.Value = needle;
        command.Parameters.Add(parameter);

        var raw = await command.ExecuteScalarAsync();
        Assert.NotNull(raw);
        using var json = JsonDocument.Parse(raw.ToString()!);
        var explain = json.RootElement[0];
        var plan = explain.GetProperty("Plan");
        var scanNodes = new List<string>();
        CollectScans(plan, scanNodes);

        return new PlanInfo(
            explain.GetProperty("Execution Time").GetDouble(),
            plan.GetProperty("Shared Hit Blocks").GetInt32(),
            plan.GetProperty("Shared Read Blocks").GetInt32(),
            string.Join(", ", scanNodes)
        );
    }

    private static void CollectScans(JsonElement node, List<string> scans)
    {
        var type = node.GetProperty("Node Type").GetString() ?? string.Empty;
        if (type.Contains("Scan", StringComparison.Ordinal))
        {
            var index = node.TryGetProperty("Index Name", out var name)
                ? name.GetString()
                : null;
            scans.Add(index is null ? type : $"{type} [{index}]");
        }

        if (node.TryGetProperty("Plans", out var nested))
        {
            foreach (var child in nested.EnumerateArray())
            {
                CollectScans(child, scans);
            }
        }
    }

    private sealed record Probe(int Count, Guid[] Ids, PlanInfo CountPlan, PlanInfo PagePlan);

    private sealed record PlanInfo(double Ms, int HitBlocks, int ReadBlocks, string Scans);
}

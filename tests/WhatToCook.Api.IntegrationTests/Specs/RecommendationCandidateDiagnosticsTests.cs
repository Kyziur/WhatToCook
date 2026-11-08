using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WhatToCook.Api.Domain.Planning;
using WhatToCook.Api.Domain.Recipes;
using WhatToCook.Api.Domain.Shopping;
using WhatToCook.Api.Infrastructure.Data;
using Xunit.Abstractions;

namespace WhatToCook.Api.IntegrationTests.Specs;

public sealed class RecommendationCandidateDiagnosticsTests(
    TestWebApplicationFactory factory,
    ITestOutputHelper output
) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task CandidatePrefilter_ShouldPreserveRecommendationsAtGrowingCatalogSizes()
    {
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var key = $"perf04owned{suffix}";
        var owned = new Ingredient { DisplayName = key, NormalizedName = key };
        var a = new Ingredient
        {
            DisplayName = $"perf04a{suffix}",
            NormalizedName = $"perf04a{suffix}",
        };
        var b = new Ingredient
        {
            DisplayName = $"perf04b{suffix}",
            NormalizedName = $"perf04b{suffix}",
        };
        var c = new Ingredient
        {
            DisplayName = $"perf04c{suffix}",
            NormalizedName = $"perf04c{suffix}",
        };
        db.Ingredients.AddRange(owned, a, b, c);

        var plan = new MealPlan
        {
            Name = $"PERF04 {suffix}",
            DateFrom = new DateOnly(2026, 11, 9),
            DateTo = new DateOnly(2026, 11, 15),
        };
        var shoppingList = new ShoppingList { MealPlan = plan };
        shoppingList.Items.Add(
            new ShoppingListItem
            {
                Name = owned.DisplayName,
                NormalizedName = key,
                State = ChecklistState.Mam,
                IsManual = false,
            }
        );
        db.ShoppingLists.Add(shoppingList);
        await db.SaveChangesAsync();

        var prefix = $"PERF04 {suffix} ";
        var keys = new[] { key };
        var lastCount = 0;
        foreach (var size in new[] { 100, 1000, 10000 })
        {
            await SeedAsync(db, prefix, lastCount + 1, size, owned.Id, a.Id, b.Id, c.Id);
            lastCount = size;
            db.ChangeTracker.Clear();

            // This matches today's service query shape and candidate filtering semantics.
            var baseQuery = db
                .Recipes.AsNoTracking()
                .Where(x => x.IsActive && x.Title.StartsWith(prefix));

            var baseline = await MeasureAsync(baseQuery);
            var eligibleQuery = baseQuery.Where(x =>
                x.Ingredients.Any(ri => keys.Contains(ri.Ingredient.NormalizedName))
            );
            var filtered = await MeasureAsync(eligibleQuery);

            var expectedCandidates = baseline.Rows.Where(x =>
                x.Ingredients.Any(ingredient => keys.Contains(ingredient.NormalizedName))
            );
            var expectedIds = expectedCandidates.Select(x => x.Id).Order().ToArray();
            var actualIds = filtered.Rows.Select(x => x.Id).Order().ToArray();
            Assert.Equal(expectedIds, actualIds);

            var expected = Rank(baseline.Rows, key);
            var selected = Rank(filtered.Rows, key);
            Assert.Equal(expected, selected);
            // Recipe 40 is intentionally inactive at every measured catalog size.
            Assert.Equal(size - 1, baseline.Rows.Count);
            Assert.True(filtered.Rows.Count < baseline.Rows.Count);

            var sql = eligibleQuery.ToQueryString();
            Assert.Contains("EXISTS", sql);

            output.WriteLine($"PERF-04 catalog: {size}");
            output.WriteLine(
                $"PERF-04 baseline rows: {baseline.Rows.Count}; links: {baseline.Links}"
            );
            output.WriteLine(
                $"PERF-04 filtered rows: {filtered.Rows.Count}; links: {filtered.Links}"
            );
            output.WriteLine(
                $"PERF-04 baseline ms: {baseline.ElapsedMs:F2}; bytes: {baseline.AllocatedBytes}"
            );
            output.WriteLine(
                $"PERF-04 filtered ms: {filtered.ElapsedMs:F2}; bytes: {filtered.AllocatedBytes}"
            );

            var response = await client.GetAsync(
                $"/api/shopping-lists/plan/{plan.Id}/recommendations?limit=20"
            );
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<RecommendationsPayload>();
            Assert.NotNull(result);
            Assert.Equal(expected, result.Items.Select(x => x.RecipeId).ToArray());
        }
    }

    private static Guid[] Rank(IEnumerable<Candidate> rows, string ownedKey) =>
        rows.Select(row =>
            {
                var ingredients = row
                    .Ingredients.DistinctBy(x => x.NormalizedName, StringComparer.Ordinal)
                    .ToArray();
                var matches = ingredients.Count(x => x.NormalizedName == ownedKey);
                var coverage = ingredients.Length == 0 ? 0m : (decimal)matches / ingredients.Length;
                return new RankingRow(row.Id, row.Title, matches, coverage);
            })
            .Where(x => x.Matches > 0 && x.Coverage >= 0.3m)
            .OrderByDescending(x => x.Matches)
            .ThenByDescending(x => x.Coverage)
            .ThenBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
            .Take(20)
            .Select(x => x.Id)
            .ToArray();

    private static async Task<Observation> MeasureAsync(IQueryable<Recipe> query)
    {
        var before = GC.GetTotalAllocatedBytes(precise: false);
        var startedAt = Stopwatch.GetTimestamp();
        var rows = await query
            .Select(x => new Candidate(
                x.Id,
                x.Title,
                x.Ingredients.Select(ri => new IngredientName(ri.Ingredient.NormalizedName))
                    .ToList()
            ))
            .ToListAsync();
        var elapsedMs = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
        var allocatedBytes = GC.GetTotalAllocatedBytes(precise: false) - before;
        return new Observation(
            rows,
            rows.Sum(x => x.Ingredients.Count),
            elapsedMs,
            allocatedBytes
        );
    }

    private static async Task SeedAsync(
        AppDbContext db,
        string prefix,
        int start,
        int end,
        Guid ownedId,
        Guid aId,
        Guid bId,
        Guid cId
    )
    {
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO recipes
                ("Id", "Title", "NormalizedTitle", "Servings", "IsActive", "CreatedAt", "UpdatedAt")
            SELECT
                gen_random_uuid(),
                {prefix} || lpad(n::text, 5, '0'),
                lower({prefix}) || lpad(n::text, 5, '0'),
                4,
                n <> 40,
                now(),
                now()
            FROM generate_series({start}, {end}) AS sequence(n)
            """
        );

        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO recipe_ingredients ("Id", "RecipeId", "IngredientId", "SortOrder")
            SELECT gen_random_uuid(), recipe."Id",
                CASE
                    WHEN slot = 0 AND right(recipe."Title", 5)::integer % 20 = 0
                        THEN {ownedId}
                    WHEN slot = 0 THEN {cId}
                    WHEN slot = 1 THEN {aId}
                    WHEN right(recipe."Title", 5)::integer % 100 = 0 THEN {aId}
                    ELSE {bId}
                END,
                slot
            FROM recipes recipe
            CROSS JOIN generate_series(0, 2) AS position(slot)
            WHERE recipe."Title" LIKE {prefix + "%"}
                AND right(recipe."Title", 5)::integer BETWEEN {start} AND {end}
            """
        );
        await db.Database.ExecuteSqlRawAsync("ANALYZE recipes");
        await db.Database.ExecuteSqlRawAsync("ANALYZE recipe_ingredients");
    }

    private sealed record RankingRow(Guid Id, string Title, int Matches, decimal Coverage);

    private sealed record IngredientName(string NormalizedName);

    private sealed record Candidate(Guid Id, string Title, List<IngredientName> Ingredients);

    private sealed record Observation(
        List<Candidate> Rows,
        int Links,
        double ElapsedMs,
        long AllocatedBytes
    );

    private sealed class RecommendationsPayload
    {
        public List<RecommendationItem> Items { get; set; } = [];
    }

    private sealed class RecommendationItem
    {
        public Guid RecipeId { get; set; }
    }
}

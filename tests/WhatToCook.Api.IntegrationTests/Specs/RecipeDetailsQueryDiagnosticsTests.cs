using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WhatToCook.Api.Domain.Recipes;
using WhatToCook.Api.Infrastructure.Data;
using Xunit.Abstractions;

namespace WhatToCook.Api.IntegrationTests.Specs;

public sealed class RecipeDetailsQueryDiagnosticsTests(
    TestWebApplicationFactory factory,
    ITestOutputHelper output
) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task LargeRecipe_CompareSingleAndSplitQueryWithoutTimingGate()
    {
        using var client = factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N");
        var payload = RecipeApiContracts.CreateRecipePayload($"Query shape {suffix}");
        payload.Ingredients = Enumerable
            .Range(1, 100)
            .Select(number => new RecipeApiContracts.RecipeIngredientPayload(
                $"Profile ingredient {number:000} {suffix}",
                "1",
                "g"
            ))
            .ToList();
        payload.Steps = Enumerable
            .Range(1, 100)
            .Select(number => $"Preparation step {number:000}")
            .ToList();
        payload.Tags = [$"ProfileA{suffix}", $"ProfileB{suffix}"];

        using var create = await client.PostAsJsonAsync("/api/recipes", payload);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeDetailsDto>();
        Assert.NotNull(created);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // These includes mirror the read-only details endpoint; no production query is changed.
        IQueryable<Recipe> query = db
            .Recipes.AsNoTracking()
            .Where(x => x.Id == created.Id)
            .Include(x => x.RecipeTags)
                .ThenInclude(x => x.Tag)
            .Include(x => x.Ingredients)
                .ThenInclude(x => x.Ingredient)
            .Include(x => x.Steps);

        // Warm the two EF query plans before capturing a repeat. Do not assert an
        // absolute latency or relative speed on a shared GitHub runner.
        await MeasureAsync(query.AsSingleQuery());
        await MeasureAsync(query.AsSplitQuery());
        var single = await MeasureAsync(query.AsSingleQuery());
        var split = await MeasureAsync(query.AsSplitQuery());

        Assert.Equal(100, single.Ingredients);
        Assert.Equal(100, single.Steps);
        Assert.Equal(2, single.Tags);
        Assert.Equal(single.Ingredients, split.Ingredients);
        Assert.Equal(single.Steps, split.Steps);
        Assert.Equal(single.Tags, split.Tags);

        using var details = await client.GetAsync($"/api/recipes/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, details.StatusCode);
        var actual = await details.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeDetailsDto>();
        Assert.NotNull(actual);
        Assert.Equal(100, actual.Ingredients.Count);
        Assert.Equal(100, actual.Steps.Count);

        // Upper-bound join multiplicity for this fixture (not network rows or benchmark results).
        var estimatedCartesianRows =
            single.Ingredients * single.Steps * Math.Max(single.Tags, 1);
        output.WriteLine(
            $"PERF-07 fixture: ingredients={single.Ingredients} steps={single.Steps} " +
            $"tags={single.Tags} single-query join upper bound={estimatedCartesianRows}"
        );
        output.WriteLine(
            $"PERF-07 observed elapsed (CI is noisy): single={single.ElapsedMs:F1}ms; " +
            $"split={split.ElapsedMs:F1}ms. No performance threshold is enforced."
        );
    }

    private static async Task<QueryObservation> MeasureAsync(IQueryable<Recipe> query)
    {
        var startedAt = Stopwatch.GetTimestamp();
        var recipe = await query.SingleAsync();
        var elapsed = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
        return new QueryObservation(
            elapsed,
            recipe.Ingredients.Count,
            recipe.Steps.Count,
            recipe.RecipeTags.Count
        );
    }

    private sealed record QueryObservation(
        double ElapsedMs,
        int Ingredients,
        int Steps,
        int Tags
    );
}

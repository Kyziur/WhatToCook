using System.Net.Http.Json;

namespace WhatToCook.Api.IntegrationTests.Specs;

public sealed class LargeSeedSpecificationTests(LargeSeedWebApplicationFactory factory)
    : IClassFixture<LargeSeedWebApplicationFactory>
{
    [Fact]
    public async Task LargeSeedProfile_ShouldProvideExactlyOneHundredRecipesAndTwentyPlans()
    {
        using var client = factory.CreateClient();

        var recipes = await client.GetFromJsonAsync<List<RecipeApiContracts.RecipeSummaryDto>>(
            "/api/recipes"
        );
        var plans = await client.GetFromJsonAsync<List<MealPlanSummaryDto>>("/api/plans");

        Assert.NotNull(recipes);
        Assert.NotNull(plans);
        Assert.Equal(100, recipes!.Count);
        Assert.All(recipes, recipe => Assert.True(recipe.IsActive));
        Assert.Equal(20, plans!.Count);
        Assert.All(plans, plan => Assert.Equal(7, plan.PlannedRecipeCount));
    }

    private sealed class MealPlanSummaryDto
    {
        public int PlannedRecipeCount { get; init; }
    }
}

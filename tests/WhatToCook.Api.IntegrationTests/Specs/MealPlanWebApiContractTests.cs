using System.Net.Http.Json;

extern alias WhatToCookWeb;

using WhatToCookWeb::WhatToCook.Web.Components.Features.Planning.Services;

namespace WhatToCook.Api.IntegrationTests.Specs;

/// <summary>
/// Real PostgreSQL-backed API responses are deserialized by the actual Blazor Web HTTP client.
/// This catches response/request drift without centralizing feature DTOs.
/// </summary>
public sealed class MealPlanWebApiContractTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task WebClient_CanRoundTripPlanAndRecognizeRegenerationConflict()
    {
        using var httpClient = factory.CreateClient();
        var client = new ApiMealPlanService(httpClient);
        var from = new DateOnly(2026, 8, 3);
        var to = new DateOnly(2026, 8, 9);
        var name = $"Web client contract {Guid.NewGuid():N}";

        var created = await client.CreatePlanAsync(new MealPlanCreateRequest(name, from, to));
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(name, created.Name);
        Assert.Equal(from, created.DateFrom);
        Assert.Equal(to, created.DateTo);
        Assert.False(created.HasGeneratedShoppingList);
        Assert.Empty(created.Entries);

        var summaries = await client.GetPlansAsync();
        var summary = Assert.Single(summaries, plan => plan.Id == created.Id);
        Assert.Equal(name, summary.Name);
        Assert.Equal(0, summary.PlannedRecipeCount);

        var fetched = await client.GetPlanAsync(created.Id);
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(created.Name, fetched.Name);
        Assert.Equal(created.DateFrom, fetched.DateFrom);
        Assert.Equal(created.DateTo, fetched.DateTo);
        Assert.Empty(fetched.Entries);

        using var recipeResponse = await httpClient.PostAsJsonAsync(
            "/api/recipes",
            RecipeApiContracts.CreateRecipePayload($"Contract recipe {Guid.NewGuid():N}")
        );
        recipeResponse.EnsureSuccessStatusCode();
        var recipe = await recipeResponse.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeDetailsDto>();
        Assert.NotNull(recipe);

        var withEntry = await client.SavePlanAsync(
            created.Id,
            new MealPlanSaveRequest(
                name,
                from,
                to,
                false,
                [new MealPlanEntry(null, recipe.Id, recipe.Title, true, from, 1.5m, 0)]
            )
        );
        Assert.True(withEntry.IsSuccess, withEntry.ErrorMessage);
        Assert.NotNull(withEntry.Plan);
        var mappedEntry = Assert.Single(withEntry.Plan.Entries);
        Assert.NotNull(mappedEntry.Id);
        Assert.Equal(recipe.Id, mappedEntry.RecipeId);
        Assert.Equal(recipe.Title, mappedEntry.RecipeTitle);
        Assert.True(mappedEntry.RecipeIsActive);
        Assert.Equal(from, mappedEntry.PlannedDate);
        Assert.Equal(1.5m, mappedEntry.Multiplier);
        Assert.Equal(0, mappedEntry.SortOrder);

        var refreshedWithEntry = await client.GetPlanAsync(created.Id);
        Assert.NotNull(refreshedWithEntry);
        Assert.Equal(mappedEntry, Assert.Single(refreshedWithEntry.Entries));

        Assert.True(await client.MarkShoppingGeneratedAsync(created.Id));

        var blocked = await client.SavePlanAsync(
            created.Id,
            new MealPlanSaveRequest("Updated plan", from, to, false, [])
        );
        Assert.False(blocked.IsSuccess);
        Assert.True(blocked.RegenerationRequired);

        var saved = await client.SavePlanAsync(
            created.Id,
            new MealPlanSaveRequest("Updated plan", from, to, true, [])
        );
        Assert.True(saved.IsSuccess, saved.ErrorMessage);
        Assert.False(saved.RegenerationRequired);
        Assert.NotNull(saved.Plan);
        Assert.Equal("Updated plan", saved.Plan.Name);
        Assert.True(saved.Plan.HasGeneratedShoppingList);
        Assert.Empty(saved.Plan.Entries);

        var persisted = await client.GetPlanAsync(created.Id);
        Assert.NotNull(persisted);
        Assert.Equal(saved.Plan.Id, persisted.Id);
        Assert.Equal(saved.Plan.Name, persisted.Name);
        Assert.Equal(saved.Plan.HasGeneratedShoppingList, persisted.HasGeneratedShoppingList);
        Assert.Empty(persisted.Entries);
    }
}

using WhatToCook.Web.Components.Features.Planning.Services;

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
        Assert.Equal(created, fetched);

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
        Assert.Equal(saved.Plan, persisted);
    }
}

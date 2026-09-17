using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WhatToCook.Api.Infrastructure.Data;

namespace WhatToCook.Api.IntegrationTests.Specs;

public sealed class RecipeSeedSpecificationTests(SeedEnabledWebApplicationFactory factory)
    : IClassFixture<SeedEnabledWebApplicationFactory>
{
    [Fact]
    public async Task SeedEnabledAndCatalogEmpty_ShouldInsertStarterRecipes()
    {
        using var client = factory.CreateClient();
        var recipes = await client.GetFromJsonAsync<List<RecipeApiContracts.RecipeSummaryDto>>(
            "/api/recipes"
        );

        Assert.NotNull(recipes);
        Assert.NotEmpty(recipes!);
        Assert.Contains(recipes!, x => x.Title == "Grecka sałatka");
    }

    [Fact]
    public async Task SeedEnabledAndCatalogAlreadyPopulated_ShouldNotOverwriteExistingData()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var beforeCount = dbContext.Recipes.Count();

        await RecipeCatalogSeeder.SeedIfEnabledAsync(
            dbContext,
            scope.ServiceProvider.GetRequiredService<IConfiguration>(),
            scope.ServiceProvider.GetRequiredService<IHostEnvironment>(),
            CancellationToken.None
        );

        var afterCount = dbContext.Recipes.Count();
        Assert.Equal(beforeCount, afterCount);
    }

    [Fact]
    public void SeedEnabledCatalog_ShouldProvideWideFilterDatasetForUiPreview()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var uniqueTags = dbContext.Tags.Select(x => x.NormalizedName).Distinct().Count();
        var uniqueIngredients = dbContext
            .Ingredients.Select(x => x.NormalizedName)
            .Distinct()
            .Count();

        Assert.True(
            uniqueTags >= 20,
            $"Expected at least 20 unique tags in seed data, got {uniqueTags}."
        );
        Assert.True(
            uniqueIngredients >= 50,
            $"Expected at least 50 unique ingredients in seed data, got {uniqueIngredients}."
        );
    }
}

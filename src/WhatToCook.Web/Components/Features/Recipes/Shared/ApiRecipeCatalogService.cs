using System.Net.Http.Json;

namespace WhatToCook.Web.Components.Features.Recipes.Shared;

public sealed class ApiRecipeCatalogService(HttpClient httpClient) : IRecipeCatalogService
{
    public async Task<IReadOnlyList<RecipeCatalogItem>> GetActiveRecipesAsync(
        CancellationToken cancellationToken = default
    )
    {
        var summaries =
            await httpClient.GetFromJsonAsync<List<RecipeSummaryDto>>(
                "/api/recipes",
                cancellationToken
            ) ?? [];

        return summaries
            .Where(x => x.IsActive)
            .OrderBy(x => x.Title)
            .Select(MapSummaryToCatalogItem)
            .ToList();
    }

    public async Task<RecipeCatalogItem?> GetByIdAsync(
        Guid recipeId,
        CancellationToken cancellationToken = default
    )
    {
        var details = await httpClient.GetFromJsonAsync<RecipeDetailsDto>(
            $"/api/recipes/{recipeId}",
            cancellationToken
        );
        return details is null ? null : MapToCatalogItem(details);
    }

    public async Task<bool> ArchiveAsync(
        Guid recipeId,
        CancellationToken cancellationToken = default
    )
    {
        var response = await httpClient.PostAsync(
            $"/api/recipes/{recipeId}/archive",
            content: null,
            cancellationToken
        );

        return response.IsSuccessStatusCode;
    }

    private static RecipeCatalogItem MapSummaryToCatalogItem(RecipeSummaryDto summary)
    {
        return new RecipeCatalogItem(
            summary.Id,
            summary.Title,
            summary.Servings,
            summary.Source,
            summary.MainPhotoPath,
            summary.IsActive,
            summary.Tags,
            summary.Ingredients.Select(x => new RecipeCatalogIngredient(x, null, null)).ToList(),
            []
        );
    }

    private static RecipeCatalogItem MapToCatalogItem(RecipeDetailsDto details)
    {
        return new RecipeCatalogItem(
            details.Id,
            details.Title,
            details.Servings,
            details.Source,
            details.MainPhotoPath,
            details.IsActive,
            details.Tags,
            details
                .Ingredients.Select(x => new RecipeCatalogIngredient(
                    x.Ingredient,
                    x.QuantityText,
                    x.Unit
                ))
                .ToList(),
            details.Steps
        );
    }

    private sealed class RecipeSummaryDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public int Servings { get; init; }
        public string? Source { get; init; }
        public string? MainPhotoPath { get; init; }
        public bool IsActive { get; init; }
        public List<string> Tags { get; init; } = [];
        public List<string> Ingredients { get; init; } = [];
    }

    private sealed class RecipeDetailsDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public int Servings { get; init; }
        public string? Source { get; init; }
        public string? MainPhotoPath { get; init; }
        public bool IsActive { get; init; }
        public List<string> Tags { get; init; } = [];
        public List<RecipeIngredientDto> Ingredients { get; init; } = [];
        public List<string> Steps { get; init; } = [];
    }

    private sealed class RecipeIngredientDto
    {
        public string Ingredient { get; init; } = string.Empty;
        public string? QuantityText { get; init; }
        public string? Unit { get; init; }
    }
}

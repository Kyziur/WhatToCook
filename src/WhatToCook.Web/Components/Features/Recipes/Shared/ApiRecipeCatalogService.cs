using System.Net.Http.Json;

namespace WhatToCook.Web.Components.Features.Recipes.Shared;

public sealed class ApiRecipeCatalogService(HttpClient httpClient) : IRecipeCatalogService
{
    public async Task<IReadOnlyList<RecipeCatalogItem>> GetActiveRecipesAsync(
        CancellationToken cancellationToken = default
    )
    {
        // The planner needs the complete active catalog. The legacy /api/recipes
        // endpoint caps at 500, so traverse the bounded API pages instead.
        // Facets are for the library filter UI, not the planner; skip their
        // full-catalog SQL queries on every traversed page.
        const int pageSize = 50;
        var results = new List<RecipeCatalogItem>();
        for (var page = 1; ; page++)
        {
            var response = await GetPageAsync(
                new CatalogQuery(
                    page,
                    pageSize,
                    string.Empty,
                    [],
                    [],
                    true,
                    "TitleAscending",
                    false
                ),
                cancellationToken
            );
            if (response.Items.Count == 0)
            {
                if (results.Count < response.TotalCount)
                {
                    throw new HttpRequestException(
                        "The paged recipe catalog ended before all recipes were received."
                    );
                }

                break;
            }

            results.AddRange(response.Items.Where(item => item.IsActive));
            if ((long)page * pageSize >= response.TotalCount)
            {
                if (results.Count < response.TotalCount)
                {
                    throw new HttpRequestException(
                        "The paged recipe catalog returned fewer items than its declared total."
                    );
                }

                break;
            }
        }

        return results.OrderBy(x => x.Title).ToList();
    }

    public async Task<CatalogPage> GetPageAsync(
        CatalogQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var fields = new List<string>
        {
            $"page={query.Page}",
            $"pageSize={query.PageSize}",
            $"sort={Uri.EscapeDataString(query.Sort)}",
            $"matchAllIngredients={query.MatchAllIngredients.ToString().ToLowerInvariant()}",
            $"includeFacets={query.IncludeFacets.ToString().ToLowerInvariant()}",
        };
        if (!string.IsNullOrWhiteSpace(query.Title))
        {
            fields.Add($"q={Uri.EscapeDataString(query.Title)}");
        }
        if (query.Tags.Count > 0)
        {
            fields.Add($"tags={Uri.EscapeDataString(string.Join("|", query.Tags))}");
        }
        if (query.Ingredients.Count > 0)
        {
            fields.Add($"ingredients={Uri.EscapeDataString(string.Join("|", query.Ingredients))}");
        }

        var page =
            await httpClient.GetFromJsonAsync<RecipePageDto>(
                $"/api/recipes/page?{string.Join("&", fields)}",
                cancellationToken
            ) ?? throw new HttpRequestException("Catalog page response is missing.");
        return new CatalogPage(
            page.Items.Select(MapSummaryToCatalogItem).ToList(),
            page.TotalCount,
            page.Tags,
            page.Ingredients,
            page.TagUsage
        );
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

    private sealed class RecipePageDto
    {
        public List<RecipeSummaryDto> Items { get; init; } = [];
        public int TotalCount { get; init; }
        public List<string> Tags { get; init; } = [];
        public List<string> Ingredients { get; init; } = [];
        public Dictionary<string, int> TagUsage { get; init; } = [];
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

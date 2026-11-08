namespace WhatToCook.Web.Components.Features.Recipes.Shared;

public sealed record RecipeCatalogItem(
    Guid Id,
    string Title,
    int Servings,
    string? Source,
    string? MainPhotoPath,
    bool IsActive,
    IReadOnlyList<string> Tags,
    IReadOnlyList<RecipeCatalogIngredient> Ingredients,
    IReadOnlyList<string> Steps
);

public sealed record RecipeCatalogIngredient(string Name, string? QuantityText, string? Unit);

public sealed record CatalogQuery(
    int Page,
    int PageSize,
    string Title,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> Ingredients,
    bool MatchAllIngredients,
    string Sort
);

public sealed record CatalogPage(
    IReadOnlyList<RecipeCatalogItem> Items,
    int TotalCount,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> Ingredients,
    IReadOnlyDictionary<string, int> TagUsage
);

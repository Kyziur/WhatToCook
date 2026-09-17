namespace WhatToCook.Api.Features.Recipes.Shared;

public sealed class RecipeUpsertRequest
{
    public string Title { get; init; } = string.Empty;
    public int Servings { get; init; }
    public string? Source { get; init; }
    public List<RecipeIngredientRequest> Ingredients { get; init; } = [];
    public List<string> Steps { get; init; } = [];
    public List<string> Tags { get; init; } = [];
}

public sealed class RecipeIngredientRequest
{
    public string Name { get; init; } = string.Empty;
    public string? QuantityText { get; init; }
    public string? Unit { get; init; }
}

public sealed class RecipeSummaryResponse(
    Guid id,
    string title,
    int servings,
    string? source,
    string? mainPhotoPath,
    bool isActive,
    IReadOnlyCollection<string> tags,
    IReadOnlyCollection<string> ingredients
)
{
    public Guid Id { get; } = id;
    public string Title { get; } = title;
    public int Servings { get; } = servings;
    public string? Source { get; } = source;
    public string? MainPhotoPath { get; } = mainPhotoPath;
    public bool IsActive { get; } = isActive;
    public IReadOnlyCollection<string> Tags { get; } = tags;
    public IReadOnlyCollection<string> Ingredients { get; } = ingredients;
}

public sealed class RecipeDetailsResponse(
    Guid id,
    string title,
    int servings,
    string? source,
    string? mainPhotoPath,
    bool isActive,
    IReadOnlyCollection<string> tags,
    IReadOnlyCollection<RecipeIngredientResponse> ingredients,
    IReadOnlyCollection<string> steps
)
{
    public Guid Id { get; } = id;
    public string Title { get; } = title;
    public int Servings { get; } = servings;
    public string? Source { get; } = source;
    public string? MainPhotoPath { get; } = mainPhotoPath;
    public bool IsActive { get; } = isActive;
    public IReadOnlyCollection<string> Tags { get; } = tags;
    public IReadOnlyCollection<RecipeIngredientResponse> Ingredients { get; } = ingredients;
    public IReadOnlyCollection<string> Steps { get; } = steps;
}

public sealed class RecipeIngredientResponse(string ingredient, string? quantityText, string? unit)
{
    public string Ingredient { get; } = ingredient;
    public string? QuantityText { get; } = quantityText;
    public string? Unit { get; } = unit;
}

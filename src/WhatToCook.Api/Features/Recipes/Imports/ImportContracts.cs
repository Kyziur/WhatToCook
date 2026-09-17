namespace WhatToCook.Api.Features.Recipes.Imports;

public sealed class CreateRecipeImportFromUrlRequest
{
    public string Url { get; init; } = string.Empty;
}

public sealed class UpdateRecipeImportDraftRequest
{
    public string? Title { get; init; }
    public int? Servings { get; init; }
    public string? Source { get; init; }
    public List<RecipeImportIngredientRequest> Ingredients { get; init; } = [];
    public List<string> Steps { get; init; } = [];
    public List<string> Tags { get; init; } = [];
}

public sealed class RecipeImportIngredientRequest
{
    public string Name { get; init; } = string.Empty;
    public string? QuantityText { get; init; }
    public string? Unit { get; init; }
}

public sealed class RecipeImportDraftResponse(
    Guid id,
    string sourceType,
    string status,
    string? sourceUrl,
    string? title,
    int? servings,
    string? source,
    bool canFinalize,
    Guid? finalizedRecipeId,
    IReadOnlyCollection<RecipeImportIngredientResponse> ingredients,
    IReadOnlyCollection<string> steps,
    IReadOnlyCollection<string> tags,
    IReadOnlyCollection<RecipeImportIssueResponse> issues
)
{
    public Guid Id { get; } = id;
    public string SourceType { get; } = sourceType;
    public string Status { get; } = status;
    public string? SourceUrl { get; } = sourceUrl;
    public string? Title { get; } = title;
    public int? Servings { get; } = servings;
    public string? Source { get; } = source;
    public bool CanFinalize { get; } = canFinalize;
    public Guid? FinalizedRecipeId { get; } = finalizedRecipeId;
    public IReadOnlyCollection<RecipeImportIngredientResponse> Ingredients { get; } = ingredients;
    public IReadOnlyCollection<string> Steps { get; } = steps;
    public IReadOnlyCollection<string> Tags { get; } = tags;
    public IReadOnlyCollection<RecipeImportIssueResponse> Issues { get; } = issues;
}

public sealed class RecipeImportIngredientResponse(string name, string? quantityText, string? unit)
{
    public string Name { get; } = name;
    public string? QuantityText { get; } = quantityText;
    public string? Unit { get; } = unit;
}

public sealed class RecipeImportIssueResponse(
    string fieldPath,
    string code,
    string message,
    string severity
)
{
    public string FieldPath { get; } = fieldPath;
    public string Code { get; } = code;
    public string Message { get; } = message;
    public string Severity { get; } = severity;
}

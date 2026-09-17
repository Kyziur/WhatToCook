namespace WhatToCook.Api.Features.Recipes.Imports;

public sealed record RecipeImportExtractionResult(
    bool IsSuccess,
    string? ErrorMessage,
    RecipeImportExtractedDraft? Draft
)
{
    public static RecipeImportExtractionResult Success(RecipeImportExtractedDraft draft) =>
        new(true, null, draft);

    public static RecipeImportExtractionResult Failure(string errorMessage) =>
        new(false, errorMessage, null);
}

public sealed record RecipeImportExtractedDraft(
    string? Title,
    int? Servings,
    string? Source,
    string RawContent,
    IReadOnlyList<RecipeImportIngredientRequest> Ingredients,
    IReadOnlyList<string> Steps,
    IReadOnlyList<string> Tags
);

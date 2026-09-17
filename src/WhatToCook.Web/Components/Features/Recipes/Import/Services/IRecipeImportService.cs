using WhatToCook.Web.Components.Features.Recipes.Shared;

namespace WhatToCook.Web.Components.Features.Recipes.Import.Services;

public interface IRecipeImportService
{
    Task<RecipeImportCreateResult> CreateFromUrlAsync(
        string url,
        CancellationToken cancellationToken = default
    );

    Task<RecipeImportDraft?> GetDraftAsync(
        Guid draftId,
        CancellationToken cancellationToken = default
    );

    Task<RecipeImportUpdateResult> UpdateDraftAsync(
        Guid draftId,
        RecipeImportDraftUpdateRequest request,
        CancellationToken cancellationToken = default
    );

    Task<RecipeImportFinalizeResult> FinalizeDraftAsync(
        Guid draftId,
        CancellationToken cancellationToken = default
    );
}

public sealed record RecipeImportDraft(
    Guid Id,
    string SourceType,
    string Status,
    string? SourceUrl,
    string? Title,
    int? Servings,
    string? Source,
    bool CanFinalize,
    Guid? FinalizedRecipeId,
    IReadOnlyList<RecipeImportIngredient> Ingredients,
    IReadOnlyList<string> Steps,
    IReadOnlyList<string> Tags,
    IReadOnlyList<RecipeImportIssue> Issues
);

public sealed record RecipeImportIngredient(string Name, string? QuantityText, string? Unit);

public sealed record RecipeImportIssue(
    string FieldPath,
    string Code,
    string Message,
    string Severity
);

public sealed record RecipeImportDraftUpdateRequest(
    string? Title,
    int? Servings,
    string? Source,
    IReadOnlyList<RecipeImportIngredient> Ingredients,
    IReadOnlyList<string> Steps,
    IReadOnlyList<string> Tags
);

public sealed record RecipeImportCreateResult(
    bool IsSuccess,
    RecipeImportDraft? Draft,
    string? ErrorMessage
)
{
    public static RecipeImportCreateResult Success(RecipeImportDraft draft) =>
        new(true, draft, null);

    public static RecipeImportCreateResult Failure(string message) => new(false, null, message);
}

public sealed record RecipeImportUpdateResult(
    bool IsSuccess,
    RecipeImportDraft? Draft,
    string? ErrorMessage
)
{
    public static RecipeImportUpdateResult Success(RecipeImportDraft draft) =>
        new(true, draft, null);

    public static RecipeImportUpdateResult Failure(string message) => new(false, null, message);
}

public sealed record RecipeImportFinalizeResult(
    bool IsSuccess,
    RecipeCatalogItem? Recipe,
    RecipeImportDraft? Draft,
    string? ErrorMessage
)
{
    public static RecipeImportFinalizeResult Success(RecipeCatalogItem recipe) =>
        new(true, recipe, null, null);

    public static RecipeImportFinalizeResult Failure(
        RecipeImportDraft? draft,
        string? errorMessage
    ) => new(false, null, draft, errorMessage);
}

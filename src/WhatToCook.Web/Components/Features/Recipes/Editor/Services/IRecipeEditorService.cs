namespace WhatToCook.Web.Components.Features.Recipes.Editor.Services;

public interface IRecipeEditorService
{
    Task<RecipeEditorDraft?> GetByIdAsync(
        Guid recipeId,
        CancellationToken cancellationToken = default
    );

    Task<RecipeSaveResult> SaveAsync(
        Guid? recipeId,
        RecipeSaveRequest request,
        CancellationToken cancellationToken = default
    );

    Task<RecipePhotoUploadResult> UploadPhotoAsync(
        Guid recipeId,
        Stream stream,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<string>> GetIngredientSuggestionsAsync(
        string query,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<string>> GetTagSuggestionsAsync(
        string query,
        CancellationToken cancellationToken = default
    );
}

public sealed record RecipeSaveRequest(
    string Title,
    int Servings,
    string? Source,
    IReadOnlyList<RecipeSaveIngredient> Ingredients,
    IReadOnlyList<string> Steps,
    IReadOnlyList<string> Tags
);

public sealed record RecipeSaveIngredient(string Name, string? QuantityText, string? Unit);

public sealed record RecipeEditorDraft(
    Guid Id,
    string Title,
    int Servings,
    string? Source,
    string? MainPhotoPath,
    IReadOnlyList<string> Tags,
    IReadOnlyList<RecipeSaveIngredient> Ingredients,
    IReadOnlyList<string> Steps
);

public sealed record RecipePhotoUploadResult(
    bool IsSuccess,
    string? MainPhotoPath,
    string? ErrorMessage
)
{
    public static RecipePhotoUploadResult Success(string? mainPhotoPath) =>
        new(true, mainPhotoPath, null);

    public static RecipePhotoUploadResult Failure(string? errorMessage) =>
        new(false, null, errorMessage);
}

public sealed record RecipeSaveResult(
    bool IsSuccess,
    Guid? RecipeId,
    string? ErrorMessage,
    IReadOnlyDictionary<string, IReadOnlyList<string>> ValidationErrors
)
{
    public static RecipeSaveResult Success(Guid recipeId) =>
        new(true, recipeId, null, new Dictionary<string, IReadOnlyList<string>>());

    public static RecipeSaveResult Failure(
        string? errorMessage,
        IReadOnlyDictionary<string, IReadOnlyList<string>>? validationErrors = null
    ) =>
        new(
            false,
            null,
            errorMessage,
            validationErrors ?? new Dictionary<string, IReadOnlyList<string>>()
        );
}

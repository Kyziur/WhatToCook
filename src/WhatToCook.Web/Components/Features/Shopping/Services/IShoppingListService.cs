namespace WhatToCook.Web.Components.Features.Shopping.Services;

public interface IShoppingListService
{
    Task<ShoppingListDetails?> GetForPlanAsync(
        Guid planId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<RecipeRecommendation>?> GetRecipeRecommendationsAsync(
        Guid planId,
        CancellationToken cancellationToken = default
    );

    Task<ShoppingListDetails?> SetItemStateAsync(
        Guid planId,
        Guid itemId,
        ShoppingChecklistState state,
        CancellationToken cancellationToken = default
    );

    Task<ShoppingListDetails?> AddManualItemAsync(
        Guid planId,
        string text,
        CancellationToken cancellationToken = default
    );

    Task<string?> GetCopyTextAsync(Guid planId, CancellationToken cancellationToken = default);
}

public enum ShoppingChecklistState
{
    NieMam = 0,
    Mam = 1,
}

public sealed record ShoppingListDetails(
    Guid Id,
    Guid MealPlanId,
    string MealPlanName,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<ShoppingListItem> Items
);

public sealed record ShoppingListItem(
    Guid Id,
    string Name,
    string? QuantityText,
    string? Unit,
    ShoppingChecklistState State,
    bool IsManual,
    int SortOrder,
    string DisplayText
);

public sealed record RecipeRecommendation(
    Guid RecipeId,
    string Title,
    string? MainPhotoPath,
    int MatchedIngredientCount,
    int TotalIngredientCount,
    IReadOnlyList<string> MatchedIngredients
);

using WhatToCook.Web.Components.Features.Shopping.Services;

namespace WhatToCook.Web.ComponentTests.Fakes;

internal sealed class EmptyShoppingListService : IShoppingListService
{
    public Task<ShoppingListDetails?> GetForPlanAsync(
        Guid planId,
        CancellationToken cancellationToken = default
    ) => Task.FromResult<ShoppingListDetails?>(null);

    public Task<IReadOnlyList<RecipeRecommendation>?> GetRecipeRecommendationsAsync(
        Guid planId,
        CancellationToken cancellationToken = default
    ) => Task.FromResult<IReadOnlyList<RecipeRecommendation>?>([]);

    public Task<ShoppingListDetails?> SetItemStateAsync(
        Guid planId,
        Guid itemId,
        ShoppingChecklistState state,
        CancellationToken cancellationToken = default
    ) => Task.FromResult<ShoppingListDetails?>(null);

    public Task<ShoppingListDetails?> AddManualItemAsync(
        Guid planId,
        string text,
        CancellationToken cancellationToken = default
    ) => Task.FromResult<ShoppingListDetails?>(null);

    public Task<string?> GetCopyTextAsync(
        Guid planId,
        CancellationToken cancellationToken = default
    ) => Task.FromResult<string?>(null);
}

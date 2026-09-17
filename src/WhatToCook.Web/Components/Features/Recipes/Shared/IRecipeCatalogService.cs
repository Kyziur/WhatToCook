namespace WhatToCook.Web.Components.Features.Recipes.Shared;

public interface IRecipeCatalogService
{
    Task<IReadOnlyList<RecipeCatalogItem>> GetActiveRecipesAsync(
        CancellationToken cancellationToken = default
    );

    Task<RecipeCatalogItem?> GetByIdAsync(
        Guid recipeId,
        CancellationToken cancellationToken = default
    );

    Task<bool> ArchiveAsync(Guid recipeId, CancellationToken cancellationToken = default);
}

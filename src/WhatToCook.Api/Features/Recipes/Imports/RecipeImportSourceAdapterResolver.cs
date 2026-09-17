namespace WhatToCook.Api.Features.Recipes.Imports;

public interface IRecipeImportSourceAdapterResolver
{
    IRecipeImportSourceAdapter? Resolve(Uri sourceUri);
}

public sealed class RecipeImportSourceAdapterResolver(
    IEnumerable<IRecipeImportSourceAdapter> sourceAdapters
) : IRecipeImportSourceAdapterResolver
{
    private readonly IReadOnlyList<IRecipeImportSourceAdapter> sourceAdapters = sourceAdapters
        .OrderByDescending(x => x.Priority)
        .ThenBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
        .ToList();

    public IRecipeImportSourceAdapter? Resolve(Uri sourceUri)
    {
        return sourceAdapters.FirstOrDefault(x => x.CanHandle(sourceUri));
    }
}

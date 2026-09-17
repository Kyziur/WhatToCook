namespace WhatToCook.Api.Features.Recipes.Imports;

public interface IRecipeImportSourceAdapter
{
    string Key { get; }

    int Priority { get; }

    bool CanHandle(Uri sourceUri);

    RecipeImportExtractionResult Extract(Uri sourceUri, string html);
}

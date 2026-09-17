namespace WhatToCook.Api.Features.Recipes.Imports;

public abstract class RecipeImportSourceAdapterBase(string key, int priority = 0)
    : IRecipeImportSourceAdapter
{
    public string Key { get; } = key;

    public int Priority { get; } = priority;

    public abstract bool CanHandle(Uri sourceUri);

    public abstract RecipeImportExtractionResult Extract(Uri sourceUri, string html);

    protected static bool MatchesHost(Uri sourceUri, params string[] hostFragments)
    {
        return hostFragments.Any(hostFragment =>
            string.Equals(sourceUri.Host, hostFragment, StringComparison.OrdinalIgnoreCase)
            || string.Equals(
                sourceUri.Host,
                $"www.{hostFragment}",
                StringComparison.OrdinalIgnoreCase
            )
        );
    }

    protected static bool MatchesLoopbackFixture(Uri sourceUri, string absolutePathPrefix)
    {
        return sourceUri.IsLoopback
            && sourceUri.AbsolutePath.StartsWith(
                absolutePathPrefix,
                StringComparison.OrdinalIgnoreCase
            );
    }
}

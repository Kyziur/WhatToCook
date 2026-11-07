using WhatToCook.Web.Components.Shared;

namespace WhatToCook.Web.Components.Features.Recipes.Shared;

public interface IRecipeCatalogService
{
    Task<IReadOnlyList<RecipeCatalogItem>> GetActiveRecipesAsync(
        CancellationToken cancellationToken = default
    );

    // Test/in-memory fallback. The production client overrides it with server-side paging.
    async Task<CatalogPage> GetPageAsync(
        CatalogQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var all = (await GetActiveRecipesAsync(cancellationToken)).Where(x => x.IsActive).ToList();
        var tags = all.SelectMany(x => x.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToArray();
        var ingredients = all.SelectMany(x => x.Ingredients)
            .Select(x => IngredientFilterNameNormalizer.Normalize(x.Name))
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToArray();
        var usage = all.SelectMany(x => x.Tags)
            .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.Count(), StringComparer.OrdinalIgnoreCase);
        var title = SearchTextNormalizer.Normalize(query.Title);
        var matches = all.Where(x =>
                SearchTextNormalizer.Normalize(x.Title).Contains(title, StringComparison.Ordinal)
            )
            .Where(x =>
                query.Tags.All(tag =>
                    x.Tags.Any(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase))
                )
            )
            .Where(x =>
                query.Ingredients.Count == 0
                || (
                    query.MatchAllIngredients
                        ? query.Ingredients.All(ingredient =>
                            x.Ingredients.Any(i =>
                                string.Equals(
                                    IngredientFilterNameNormalizer.Normalize(i.Name),
                                    ingredient,
                                    StringComparison.OrdinalIgnoreCase
                                )
                            )
                        )
                        : query.Ingredients.Any(ingredient =>
                            x.Ingredients.Any(i =>
                                string.Equals(
                                    IngredientFilterNameNormalizer.Normalize(i.Name),
                                    ingredient,
                                    StringComparison.OrdinalIgnoreCase
                                )
                            )
                        )
                )
            );
        var sorted = query.Sort switch
        {
            "TitleDescending" => matches.OrderByDescending(x => x.Title).ThenBy(x => x.Id),
            "ServingsAscending" => matches
                .OrderBy(x => x.Servings)
                .ThenBy(x => x.Title)
                .ThenBy(x => x.Id),
            "ServingsDescending" => matches
                .OrderByDescending(x => x.Servings)
                .ThenBy(x => x.Title)
                .ThenBy(x => x.Id),
            _ => matches.OrderBy(x => x.Title).ThenBy(x => x.Id),
        };
        var filtered = sorted.ToList();
        return new CatalogPage(
            filtered.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList(),
            filtered.Count,
            tags,
            ingredients,
            usage
        );
    }

    Task<RecipeCatalogItem?> GetByIdAsync(
        Guid recipeId,
        CancellationToken cancellationToken = default
    );

    Task<bool> ArchiveAsync(Guid recipeId, CancellationToken cancellationToken = default);
}

using Microsoft.EntityFrameworkCore;
using WhatToCook.Api.Domain.Recipes;

namespace WhatToCook.Api.Features.Recipes.Shared;

internal static class RecipePaging
{
    public const int DefaultPageSize = 24;
    public const int MaxPageSize = 50;

    public static bool IsInvalid(int page, int size) =>
        page < 1 || size < 1 || size > MaxPageSize || (long)(page - 1) * size > int.MaxValue;

    public static async Task<RecipePage> FetchAsync(
        IQueryable<Recipe> query,
        int page,
        int size,
        string? sort,
        CancellationToken cancellationToken
    )
    {
        var total = await query.CountAsync(cancellationToken);
        var ordered = sort switch
        {
            "TitleDescending" => query.OrderByDescending(x => x.NormalizedTitle).ThenBy(x => x.Title).ThenBy(x => x.Id),
            "ServingsAscending" => query.OrderBy(x => x.Servings).ThenBy(x => x.NormalizedTitle).ThenBy(x => x.Title).ThenBy(x => x.Id),
            "ServingsDescending" => query.OrderByDescending(x => x.Servings).ThenBy(x => x.NormalizedTitle).ThenBy(x => x.Title).ThenBy(x => x.Id),
            _ => query.OrderBy(x => x.NormalizedTitle).ThenBy(x => x.Title).ThenBy(x => x.Id),
        };
        var pageIds = await ordered
            .Skip((page - 1) * size)
            .Take(size)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
        if (pageIds.Count == 0)
        {
            return new RecipePage([], total);
        }

        // Load the bounded page's relationships only, then restore stable SQL ordering.
        var entities = await query
            .Where(x => pageIds.Contains(x.Id))
            .Include(x => x.RecipeTags).ThenInclude(x => x.Tag)
            .Include(x => x.Ingredients).ThenInclude(x => x.Ingredient)
            .ToListAsync(cancellationToken);
        var byId = entities.ToDictionary(x => x.Id);
        return new RecipePage(
            pageIds.Select(id => byId[id].ToSummaryResponse()).ToList(),
            total
        );
    }
}

internal sealed record RecipePage(IReadOnlyList<RecipeSummaryResponse> Items, int TotalCount);

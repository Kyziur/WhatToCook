using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhatToCook.Api.Features.Recipes.Shared;
using WhatToCook.Api.Infrastructure.Data;

namespace WhatToCook.Api.Features.Recipes.Library;

public static class LibraryEndpoints
{
    private const int MaxLibraryResults = 500;

    public static IEndpointRouteBuilder MapRecipeLibraryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/recipes").WithTags("Recipes");

        group.MapGet("/", GetActiveRecipes).WithName("GetRecipeLibrary");
        group.MapGet("/page", GetActiveRecipesPage).WithName("GetRecipeLibraryPage");

        return app;
    }

    private static async Task<IResult> GetActiveRecipesPage(
        [AsParameters] RecipeLibraryPageRequest request,
        AppDbContext dbContext,
        CancellationToken cancellationToken
    )
    {
        var page = request.Page ?? 1;
        var pageSize = request.PageSize ?? RecipePaging.DefaultPageSize;
        if (RecipePaging.IsInvalid(page, pageSize))
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    [nameof(request.Page)] = ["Page must be positive and pageSize must be between 1 and 50."],
                }
            );
        }

        var query = dbContext.Recipes.AsNoTracking().Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var title = TextNormalizer.Normalize(request.Q);
            query = query.Where(x => x.NormalizedTitle.Contains(title));
        }

        var tags = SplitNormalizedValues(request.Tags);
        foreach (var tag in tags)
        {
            // The library intentionally matches ALL selected tags, unlike the legacy search API.
            query = query.Where(x => x.RecipeTags.Any(rt => rt.Tag.NormalizedName == tag));
        }

        var ingredients = SplitNormalizedValues(request.Ingredients);
        if (request.MatchAllIngredients ?? true)
        {
            foreach (var ingredient in ingredients)
            {
                query = query.Where(x =>
                    x.Ingredients.Any(ri => ri.Ingredient.NormalizedName.Contains(ingredient))
                );
            }
        }
        else if (ingredients.Length > 0)
        {
            query = query.Where(x =>
                x.Ingredients.Any(ri => ingredients.Any(ingredient =>
                    ri.Ingredient.NormalizedName.Contains(ingredient)
                ))
            );
        }

        var result = await RecipePaging.FetchAsync(
            query, page, pageSize, request.Sort ?? "TitleAscending", cancellationToken
        );

        // Facets represent the whole active catalog, not just the current result page.
        var tagUsage = await dbContext.RecipeTags
            .AsNoTracking()
            .Where(x => x.Recipe.IsActive)
            .GroupBy(x => x.Tag.DisplayName)
            .Select(group => new { Tag = group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.Tag, x => x.Count, cancellationToken);
        var allIngredients = await dbContext.Ingredients
            .AsNoTracking()
            .Where(x => x.RecipeIngredients.Any(ri => ri.Recipe.IsActive))
            .OrderBy(x => x.DisplayName)
            .Select(x => x.DisplayName)
            .ToListAsync(cancellationToken);

        return Results.Ok(new RecipeLibraryPageResponse(
            result.Items, result.TotalCount, page, pageSize,
            tagUsage.Keys.OrderBy(x => x).ToArray(),
            allIngredients,
            tagUsage
        ));
    }

    private static string[] SplitNormalizedValues(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(TextNormalizer.Normalize)
                .Where(x => x.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

    private sealed class RecipeLibraryPageRequest
    {
        [FromQuery(Name = "page")]
        public int? Page { get; init; }
        [FromQuery(Name = "pageSize")]
        public int? PageSize { get; init; }
        [FromQuery(Name = "q")]
        public string? Q { get; init; }
        [FromQuery(Name = "tags")]
        public string? Tags { get; init; }
        [FromQuery(Name = "ingredients")]
        public string? Ingredients { get; init; }
        [FromQuery(Name = "matchAllIngredients")]
        public bool? MatchAllIngredients { get; init; }
        [FromQuery(Name = "sort")]
        public string? Sort { get; init; }
    }

    private sealed record RecipeLibraryPageResponse(
        IReadOnlyList<RecipeSummaryResponse> Items,
        int TotalCount,
        int Page,
        int PageSize,
        IReadOnlyList<string> Tags,
        IReadOnlyList<string> Ingredients,
        IReadOnlyDictionary<string, int> TagUsage
    );

    private static async Task<IResult> GetActiveRecipes(
        AppDbContext dbContext,
        CancellationToken cancellationToken
    )
    {
        var recipes = await dbContext
            .Recipes.AsNoTracking()
            .Where(x => x.IsActive)
            .Include(x => x.RecipeTags)
                .ThenInclude(x => x.Tag)
            .Include(x => x.Ingredients)
                .ThenInclude(x => x.Ingredient)
            .OrderBy(x => x.Title)
            .Take(MaxLibraryResults)
            .ToListAsync(cancellationToken);

        return Results.Ok(recipes.Select(x => x.ToSummaryResponse()));
    }
}

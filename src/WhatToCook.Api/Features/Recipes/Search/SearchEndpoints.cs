using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhatToCook.Api.Features.Recipes.Shared;
using WhatToCook.Api.Infrastructure.Data;

namespace WhatToCook.Api.Features.Recipes.Search;

public static class SearchEndpoints
{
    public static IEndpointRouteBuilder MapRecipeSearchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/recipes").WithTags("Recipes");

        group.MapGet("/search", SearchRecipes).WithName("SearchRecipes");
        group
            .MapGet("/suggestions/ingredients", GetIngredientSuggestions)
            .WithName("GetIngredientSuggestions");
        group.MapGet("/suggestions/tags", GetTagSuggestions).WithName("GetTagSuggestions");

        return app;
    }

    private static async Task<IResult> SearchRecipes(
        [AsParameters] RecipeSearchRequest request,
        AppDbContext dbContext,
        CancellationToken cancellationToken
    )
    {
        var query = dbContext
            .Recipes.AsNoTracking()
            .Where(x => x.IsActive)
            .Include(x => x.RecipeTags)
                .ThenInclude(x => x.Tag)
            .Include(x => x.Ingredients)
                .ThenInclude(x => x.Ingredient)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            var normalizedTitle = TextNormalizer.Normalize(request.Title);
            query = query.Where(x => x.NormalizedTitle.Contains(normalizedTitle));
        }

        var normalizedTags = request
            .Tag.Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(TextNormalizer.Normalize)
            .Distinct()
            .ToArray();
        if (normalizedTags.Length > 0)
        {
            query = query.Where(x =>
                x.RecipeTags.Any(rt => normalizedTags.Contains(rt.Tag.NormalizedName))
            );
        }

        var normalizedIngredients = request
            .Ingredient.Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(TextNormalizer.Normalize)
            .Distinct()
            .ToArray();
        foreach (var ingredient in normalizedIngredients)
        {
            query = query.Where(x =>
                x.Ingredients.Any(ri => ri.Ingredient.NormalizedName == ingredient)
            );
        }

        var recipes = await query.OrderBy(x => x.Title).ToListAsync(cancellationToken);

        return Results.Ok(recipes.Select(x => x.ToSummaryResponse()));
    }

    private static async Task<IResult> GetIngredientSuggestions(
        [FromQuery(Name = "q")] string? query,
        [FromQuery(Name = "limit")] int? limit,
        AppDbContext dbContext,
        CancellationToken cancellationToken
    )
    {
        var normalizedQuery = TextNormalizer.Normalize(query ?? string.Empty);
        var maxResults = Math.Clamp(limit ?? 12, 1, 50);

        var ingredientQuery = dbContext.Ingredients.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            ingredientQuery = ingredientQuery.Where(x =>
                x.NormalizedName.Contains(normalizedQuery)
            );
        }

        var suggestions = await ingredientQuery
            .OrderBy(x => x.DisplayName)
            .Select(x => x.DisplayName)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        return Results.Ok(suggestions);
    }

    private static async Task<IResult> GetTagSuggestions(
        [FromQuery(Name = "q")] string? query,
        [FromQuery(Name = "limit")] int? limit,
        AppDbContext dbContext,
        CancellationToken cancellationToken
    )
    {
        var normalizedQuery = TextNormalizer.Normalize(query ?? string.Empty);
        var maxResults = Math.Clamp(limit ?? 12, 1, 50);

        var tagQuery = dbContext.Tags.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            tagQuery = tagQuery.Where(x => x.NormalizedName.Contains(normalizedQuery));
        }

        var suggestions = await tagQuery
            .OrderBy(x => x.DisplayName)
            .Select(x => x.DisplayName)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        return Results.Ok(suggestions);
    }

    private sealed class RecipeSearchRequest
    {
        [FromQuery(Name = "title")]
        public string? Title { get; init; }

        [FromQuery(Name = "tag")]
        public string[] Tag { get; init; } = [];

        [FromQuery(Name = "ingredient")]
        public string[] Ingredient { get; init; } = [];
    }
}

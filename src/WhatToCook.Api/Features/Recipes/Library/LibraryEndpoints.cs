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

        group
            .MapGet("/", GetActiveRecipes)
            .WithName("GetRecipeLibrary")
            .CacheOutput(options => options.Cache());

        return app;
    }

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

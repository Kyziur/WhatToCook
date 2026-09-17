using Microsoft.EntityFrameworkCore;
using WhatToCook.Api.Features.Recipes.Shared;
using WhatToCook.Api.Infrastructure.Data;

namespace WhatToCook.Api.Features.Recipes.Details;

public static class DetailsEndpoints
{
    public static IEndpointRouteBuilder MapRecipeDetailsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/recipes").WithTags("Recipes");

        group.MapGet("/{recipeId:guid}", GetRecipeDetails).WithName("GetRecipeDetails");

        return app;
    }

    private static async Task<IResult> GetRecipeDetails(
        Guid recipeId,
        AppDbContext dbContext,
        CancellationToken cancellationToken
    )
    {
        var recipe = await dbContext
            .Recipes.AsNoTracking()
            .Include(x => x.RecipeTags)
                .ThenInclude(x => x.Tag)
            .Include(x => x.Ingredients)
                .ThenInclude(x => x.Ingredient)
            .Include(x => x.Steps)
            .SingleOrDefaultAsync(x => x.Id == recipeId, cancellationToken);

        return recipe is null ? Results.NotFound() : Results.Ok(recipe.ToDetailsResponse());
    }
}

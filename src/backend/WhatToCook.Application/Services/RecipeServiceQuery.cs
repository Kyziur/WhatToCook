using Microsoft.EntityFrameworkCore;
using WhatToCook.Application.DataTransferObjects.Responses;
using WhatToCook.Application.Infrastructure;

namespace WhatToCook.Application.Services;

public class RecipeServiceQuery
{
    private readonly DatabaseContext _dbContext;

    public RecipeServiceQuery(DatabaseContext dbcontext) => _dbContext = dbcontext;

    public async Task<RecipeResponse?> GetByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        Domain.Recipe? recipe = await _dbContext.Recipes
            .AsNoTracking()
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(x => x.Name == name);

        if (recipe is null)
        {
            return null;
        }

        var recipeResponse = RecipeResponse.MapFrom(recipe);

        return recipeResponse;
    }

    public async Task<List<RecipeResponse>> GetRecipes() => await _dbContext.Recipes.Select(recipe => new RecipeResponse()
    {
        Id = recipe.Id,
        Name = recipe.Name,
        Ingredients = recipe.Ingredients.Select(x => x.Name),
        PreparationDescription = recipe.PreparationDescription,
        TimeToPrepare = recipe.TimeToPrepare,
        ImagePath = recipe.Image.Path,
        Tags = recipe.Tags.Select(x => x.Name).ToArray()
    }).ToListAsync();
}
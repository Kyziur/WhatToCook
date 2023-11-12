using Microsoft.EntityFrameworkCore;
using WhatToCook.Application.DataTransferObjects.Responses;
using WhatToCook.Application.Infrastructure;

namespace WhatToCook.Application.Services;

public class RecipeServiceQuery
{
    private readonly DatabaseContext _dbcontext;

    public RecipeServiceQuery(DatabaseContext dbcontext)
    {
        _dbcontext = dbcontext;
    }

    public async Task<RecipeResponse?> GetByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var recipe = await _dbcontext.Recipes.Include(r => r.Ingredients)
            .FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());
        if (recipe is null)
        {
            return null;
        }

        var recipeResponse = RecipeResponse.MapFrom(recipe);

        return recipeResponse;
    }

    public async Task<List<RecipeResponse>> GetRecipes()
    {
        var query = await _dbcontext.Recipes.Select(recipe => new RecipeResponse()
        {
            Id = recipe.Id,
            Name = recipe.Name,
            Ingredients = recipe.Ingredients.Select(x => x.Name),
            PreparationDescription = recipe.Description,
            TimeToPrepare = recipe.TimeToPrepare,
            ImagePath = recipe.Image
        }).ToListAsync();
        return query;
    }
}
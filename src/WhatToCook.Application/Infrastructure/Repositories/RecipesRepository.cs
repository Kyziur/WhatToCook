using Microsoft.EntityFrameworkCore;
using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure.Repositories;

public interface IRecipesRepository
{
    List<Recipe> GetByNames(IEnumerable<string> names);
}

public class RecipesRepository : IRecipesRepository
{
    private readonly DatabaseContext _dbContext;

    public RecipesRepository(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<Recipe> GetByNames(IEnumerable<string> names)
    {
        var recipes = _dbContext.Recipes.Include(recipe => recipe.PlansOfMeals)
        .Where(recipe => names.Contains(recipe.Name)).ToList();
        var existingRecipeNames = recipes.Select(r => r.Name).ToList();
        if (!existingRecipeNames.OrderBy(n => n).SequenceEqual(names.OrderBy(n => n)))
        {
            var missingRecipeNames = existingRecipeNames.Except(names);
            throw new Exception($"Recipes do not exist in the database: {string.Join(", ", missingRecipeNames)}");
        }
        return recipes;
    }
}
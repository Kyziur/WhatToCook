using Microsoft.EntityFrameworkCore;
using WhatToCook.Application.Domain;
using WhatToCook.WebApp.DataTransferObject.Requests;

namespace WhatToCook.Application.Infrastructure.Repositories;

public interface IRecipesRepository
{
    Task<Recipe> GetRecipeByName(string name);
    List<Recipe> GetByNames(IEnumerable<string> names);
    Task Create(Recipe recipe);
    Task Update(Recipe recipe);
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
    public async Task<Recipe> GetRecipeByName(string name)
    {
        return await _dbContext.Recipes.Include(r => r.Ingredients).FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task Create(Recipe recipe)
    {
        await _dbContext.Recipes.AddAsync(recipe);
        await _dbContext.SaveChangesAsync();
        
    }
    public async Task Update(Recipe recipe)
    {
         _dbContext.Recipes.Update(recipe);
        await _dbContext.SaveChangesAsync();

    }
}
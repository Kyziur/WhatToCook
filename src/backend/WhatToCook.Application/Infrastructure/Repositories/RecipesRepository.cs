using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Exceptions;

namespace WhatToCook.Application.Infrastructure.Repositories;

public interface IRecipesRepository
{
    Task Create(Recipe recipe);
    Task Delete(int id);
    Task<Recipe?> GetByName(string name);
    Task<Recipe?> GetById(int id);
    List<Recipe> GetRecipesByIdForMealPlan(IEnumerable<int> ids);
    Task Update(Recipe recipe);
}

public class RecipesRepository : IRecipesRepository
{
    private readonly DatabaseContext _dbContext;
    private readonly ILogger _logger;

    public RecipesRepository(DatabaseContext dbContext, ILogger<RecipesRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Create(Recipe recipe)
    {
        _ = await _dbContext.Recipes.AddAsync(recipe);
        _ = await _dbContext.SaveChangesAsync();
    }

    public async Task Delete(int id) => _ = await _dbContext.Recipes.FindAsync(id) ??
                     throw new NotFoundException($"Recipe with ID '{id}' not found.");

    public async Task<Recipe?> GetByName(string name) => await _dbContext.Recipes
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Name == name);

    public async Task<Recipe?> GetById(int id) => await _dbContext.Recipes
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == id);

    public List<Recipe> GetRecipesByIdForMealPlan(IEnumerable<int> ids)
    {
        var uniqueIds = ids.Distinct().ToList();
        var recipes = _dbContext.Recipes.Where(recipe => uniqueIds.Contains(recipe.Id)).ToList();
        if (recipes.Count != uniqueIds.Count)
        {
            IEnumerable<int> missingRecipeIds = uniqueIds.Except(recipes.Select(x => x.Id));
            string errorMessage = "Not all recipes exist in the database.";
            _logger.LogError(errorMessage + " Missing IDs: {MissingIDs}", missingRecipeIds);
            throw new Exception(errorMessage);
        }

        return recipes;
    }

    public async Task Update(Recipe recipe)
    {
        _ = _dbContext.Recipes.Update(recipe);
        _ = await _dbContext.SaveChangesAsync();
    }
}
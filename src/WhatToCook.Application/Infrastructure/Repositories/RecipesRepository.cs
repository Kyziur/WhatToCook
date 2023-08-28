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
        return _dbContext.Recipes.Include(recipe => recipe.PlansOfMeals)
            .Where(recipe => names.Contains(recipe.Name)).ToList();
    }
}
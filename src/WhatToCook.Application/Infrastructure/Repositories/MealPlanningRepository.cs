using Microsoft.EntityFrameworkCore;
using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure.Repositories;

public interface IMealPlanningRepository
{
    Task<PlanOfMeals> GetMealPlanByName(string name);
    Task Create(PlanOfMeals planOfMeals);
    Task Update(PlanOfMeals planOfMeals);
}

public class MealPlanningRepository : IMealPlanningRepository
{
    private readonly DatabaseContext _dbContext;

    public MealPlanningRepository(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Create(PlanOfMeals planOfMeals)
    {
        await _dbContext.PlanOfMeals.AddAsync(planOfMeals);
        await _dbContext.SaveChangesAsync();
    }
    public async Task<PlanOfMeals> GetMealPlanByName(string name)
    {
        return await _dbContext.PlanOfMeals.Include(r => r.Recipes).FirstOrDefaultAsync(r => r.Name == name);
    }
    public async Task Update(PlanOfMeals planOfMeals)
    {
        _dbContext.PlanOfMeals.Update(planOfMeals);
        await _dbContext.SaveChangesAsync();
    }

}
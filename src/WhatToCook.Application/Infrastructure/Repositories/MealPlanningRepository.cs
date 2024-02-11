using Microsoft.EntityFrameworkCore;
using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure.Repositories;

public interface IMealPlanningRepository
{
    Task Create(PlanOfMeals planOfMeals);

    Task<PlanOfMeals?> GetMealPlanByName(string name);

    Task Update(PlanOfMeals planOfMeals);
}

public class MealPlanningRepository : IMealPlanningRepository
{
    private readonly DatabaseContext _dbContext;

    public MealPlanningRepository(DatabaseContext dbContext) => _dbContext = dbContext;

    public async Task Create(PlanOfMeals planOfMeals)
    {
        _ = await _dbContext.PlanOfMeals.AddAsync(planOfMeals);
        _ = await _dbContext.SaveChangesAsync();
    }

    public async Task<PlanOfMeals?> GetMealPlanByName(string name) => await _dbContext.PlanOfMeals
        .Include(r => r.RecipePlanOfMeals)
        .ThenInclude(p => p.Recipe)
        .FirstOrDefaultAsync(x => x.Name == name);

    public async Task Update(PlanOfMeals planOfMeals)
    {
        _ = _dbContext.PlanOfMeals.Update(planOfMeals);
        _ = await _dbContext.SaveChangesAsync();
    }
}
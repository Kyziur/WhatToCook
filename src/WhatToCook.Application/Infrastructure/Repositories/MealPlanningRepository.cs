using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure.Repositories;

public interface IMealPlanningRepository
{
    Task Create(PlanOfMeals planOfMeals);
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
}
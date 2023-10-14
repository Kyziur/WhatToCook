﻿using Microsoft.EntityFrameworkCore;
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
    private readonly DatabaseContext _dbcontext;

    public MealPlanningRepository(DatabaseContext dbContext)
    {
        _dbcontext = dbContext;
    }

    public async Task Create(PlanOfMeals planOfMeals)
    {
        await _dbcontext.PlanOfMeals.AddAsync(planOfMeals);
        await _dbcontext.SaveChangesAsync();
    }
    public async Task<PlanOfMeals?> GetMealPlanByName(string name)
    {
        return await _dbcontext.PlanOfMeals.Include(r => r.Recipes).FirstOrDefaultAsync(r => r.Name == name);
    }
    public async Task Update(PlanOfMeals planOfMeals)
    {
        _dbcontext.PlanOfMeals.Update(planOfMeals);
        await _dbcontext.SaveChangesAsync();
    }

}
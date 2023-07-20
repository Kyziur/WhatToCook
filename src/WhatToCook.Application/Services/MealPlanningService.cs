using Microsoft.EntityFrameworkCore;
using WhatToCook.Application.DataTransferObjects.Requests;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Infrastructure;

namespace WhatToCook.Application.Services;

public class MealPlanningService
{
    private DatabaseContext _dbcontext;

    public MealPlanningService(DatabaseContext dbconetxt)
    {
        _dbcontext = dbconetxt;
    }

    public async Task<PlanOfMeals> Create(PlanOfMealRequest planOfMealRequest)
    {
        var getRecipeForMealPlan = planOfMealRequest.Recipes.Select(x => x.Name);
        var recipies = _dbcontext.Recipes.Include(recipe => recipe.PlansOfMeals)
            .Where(recipe => getRecipeForMealPlan.Contains(recipe.Name)).ToList();
        var planOfMeals = new PlanOfMeals()
        {
            Name = planOfMealRequest.Name,
            Id = planOfMealRequest.Id,
            FromDate = DateTime.SpecifyKind(planOfMealRequest.FromDate, DateTimeKind.Utc),
            ToDate = DateTime.SpecifyKind(planOfMealRequest.ToDate, DateTimeKind.Utc),
            Recipes = recipies
        };

        await _dbcontext.PlanOfMeals.AddAsync(planOfMeals);
        await _dbcontext.SaveChangesAsync();
        return planOfMeals;
    }
}
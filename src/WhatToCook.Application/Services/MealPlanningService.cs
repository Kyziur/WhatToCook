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
        var recipes = _dbcontext.Recipes.Include(recipe => recipe.PlansOfMeals)
            .Where(recipe => getRecipeForMealPlan.Contains(recipe.Name)).ToList();
        var planOfMeals = new PlanOfMeals()
        {
            Name = planOfMealRequest.Name,
            Id = planOfMealRequest.Id,
            FromDate = DateTime.SpecifyKind(planOfMealRequest.FromDate, DateTimeKind.Utc),
            ToDate = DateTime.SpecifyKind(planOfMealRequest.ToDate, DateTimeKind.Utc),
            Recipes = recipes
        };

        await _dbcontext.PlanOfMeals.AddAsync(planOfMeals);
        await _dbcontext.SaveChangesAsync();
        return planOfMeals;
    }

    public async Task<PlanOfMeals> Update(UpdatePlanOfMealRequest planOfMealRequest)
    {
        var planOfMeals = await _dbcontext.PlanOfMeals.Include(r => r.Recipes).FirstOrDefaultAsync(r => r.Name == planOfMealRequest.Name);
        var recipes = _dbcontext.Recipes.Include(recipe => recipe.PlansOfMeals)
            .Where(recipe => planOfMealRequest.Recipes.Contains(recipe.Name)).ToList();
        if (planOfMeals == null)
        {
            throw new Exception($"Cannot Update {planOfMealRequest.Name}");
        }
        planOfMeals.Name = planOfMealRequest.Name;
        planOfMeals.FromDate = planOfMealRequest.FromDate;
        planOfMeals.ToDate = planOfMealRequest.ToDate;
        planOfMeals.Recipes = recipes;
        _dbcontext.Update(planOfMeals);
        await _dbcontext.SaveChangesAsync();
        return planOfMeals;

    }
}
using WhatToCook.Application.DataTransferObjects.Requests;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Infrastructure.Repositories;

namespace WhatToCook.Application.Services;

public class MealPlanningService
{
    private readonly IRecipesRepository _recipesRepository;
    private readonly IMealPlanningRepository _mealPlanningRepository;

    public MealPlanningService(IRecipesRepository recipesRepository, IMealPlanningRepository mealPlanningRepository)
    {
        _recipesRepository = recipesRepository;
        _mealPlanningRepository = mealPlanningRepository;
    }

    public async Task<PlanOfMeals> Create(PlanOfMealRequest planOfMealRequest)
    {
        var getRecipeForMealPlan = planOfMealRequest.Recipes.Select(x => x.Name);
        var recipes = _recipesRepository.GetByNames(getRecipeForMealPlan);
        var recipesNamesToLookFor = planOfMealRequest.Recipes.Select(x => x.Name);
        var recipies = _recipesRepository.GetByNames(recipesNamesToLookFor);
        
        var planOfMeals = new PlanOfMeals()
        {
            Name = planOfMealRequest.Name,
            Id = planOfMealRequest.Id,
            FromDate = DateTime.SpecifyKind(planOfMealRequest.FromDate, DateTimeKind.Utc),
            ToDate = DateTime.SpecifyKind(planOfMealRequest.ToDate, DateTimeKind.Utc),
            Recipes = recipes
        };

        await _mealPlanningRepository.Create(planOfMeals);
        
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
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
        var getMealPlanToUpdate = await _mealPlanningRepository.GetMealPlanByName(planOfMealRequest.Name);
        var getRecipeForMealPlanUpdate = planOfMealRequest.Recipes.Select(x => x.Name);

        var recipes = _recipesRepository.GetByNames(getRecipeForMealPlanUpdate);
        if (getMealPlanToUpdate == null)
        {
            throw new Exception($"Cannot Update {planOfMealRequest.Name}");
        }
        //check if all recipes in the request exist in the database
        var existingRecipes = recipes.Select(r => r.Name).ToList();
        if (!existingRecipes.OrderBy(n => n).SequenceEqual(getRecipeForMealPlanUpdate.OrderBy(n => n)))
        {
            throw new Exception("Some recipes do not exist in the database");
        }
        //check if todate is lower than fromdate
        if (planOfMealRequest.ToDate < planOfMealRequest.FromDate)
        {
            throw new Exception("ToDate can't be lower than FromDate");
        }
        getMealPlanToUpdate.Name = planOfMealRequest.Name;
        getMealPlanToUpdate.FromDate = planOfMealRequest.FromDate;
        getMealPlanToUpdate.ToDate = planOfMealRequest.ToDate;
        getMealPlanToUpdate.Recipes = recipes;
        await _mealPlanningRepository.Update(getMealPlanToUpdate);
        return getMealPlanToUpdate;
    }

}
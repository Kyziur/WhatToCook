using Microsoft.Extensions.Logging;
using WhatToCook.Application.DataTransferObjects.Requests;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Exceptions;
using WhatToCook.Application.Infrastructure.Repositories;

namespace WhatToCook.Application.Services;

public class MealPlanningService
{
    private readonly IRecipesRepository _recipesRepository;
    private readonly IMealPlanningRepository _mealPlanningRepository;
    private readonly ILogger _logger;
    public MealPlanningService(IRecipesRepository recipesRepository, IMealPlanningRepository mealPlanningRepository, ILogger<MealPlanningService> logger)
    {
        _recipesRepository = recipesRepository;
        _mealPlanningRepository = mealPlanningRepository;
        _logger = logger;
    }

    public async Task<PlanOfMeals> Create(PlanOfMealRequest planOfMealRequest)
    {
        var RecipeForMealPlan = planOfMealRequest.Recipes.Select(x => x.Name);
        var recipes = _recipesRepository.GetRecipesByNameForMealPlan(RecipeForMealPlan);

        var planOfMeals = new PlanOfMeals
    (
        planOfMealRequest.Name,
        DateTime.SpecifyKind(planOfMealRequest.FromDate, DateTimeKind.Utc),
        DateTime.SpecifyKind(planOfMealRequest.ToDate, DateTimeKind.Utc),
        recipes
    );
        await _mealPlanningRepository.Create(planOfMeals);

        return planOfMeals;
    }

    public async Task<PlanOfMeals> Update(UpdatePlanOfMealRequest planOfMealRequest)
    {
        var mealPlanToUpdate = await _mealPlanningRepository.GetMealPlanByName(planOfMealRequest.Name);
        var RecipeForMealPlanUpdate = planOfMealRequest.Recipes.Select(x => x.Name);

        var recipes = _recipesRepository.GetRecipesByNameForMealPlan(RecipeForMealPlanUpdate);
        if (mealPlanToUpdate == null)
        {
            _logger.LogError($"Attempted to update a non-existent meal plan: {planOfMealRequest.Name}");
            throw new NotFoundException(nameof(mealPlanToUpdate));
        }
        //check if all recipes in the request exist in the database
        var existingRecipes = recipes.Select(r => r.Name).ToList();
        if (!existingRecipes.OrderBy(n => n).SequenceEqual(RecipeForMealPlanUpdate.OrderBy(n => n)))
        {
            _logger.LogError($"Some recipes do not exist in the database");
            throw new NotFoundException($"Some recipes do not exist in the database");
        }

        mealPlanToUpdate.Name = planOfMealRequest.Name;
        mealPlanToUpdate.SetDates(planOfMealRequest.FromDate, planOfMealRequest.ToDate);
        mealPlanToUpdate.Recipes = recipes;;
        await _mealPlanningRepository.Update(mealPlanToUpdate);
        return mealPlanToUpdate;
    }

}
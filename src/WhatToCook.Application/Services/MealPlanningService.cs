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
        var requestedRecipeIds = planOfMealRequest.Recipes.SelectMany(x => x.RecipeIds).ToList();
        var recipes = _recipesRepository.GetRecipesByIdForMealPlan(requestedRecipeIds);
        var dayRecipePairs = planOfMealRequest.Recipes
            .SelectMany(dayRecipe => dayRecipe.RecipeIds,
                        (dayRecipe, recipeId) =>
                        new RecipePerDay(dayRecipe.Day, recipes.First(r => r.Id == recipeId))).ToList();

        var planOfMeals = new PlanOfMeals
    (
        planOfMealRequest.Name,
        DateTime.SpecifyKind(planOfMealRequest.FromDate, DateTimeKind.Utc),
        DateTime.SpecifyKind(planOfMealRequest.ToDate, DateTimeKind.Utc),
        new List<RecipePerDay>()
    );
        planOfMeals.SetRecipes(dayRecipePairs);
        _logger.LogInformation("Creating a meal plan with {numberOfRecipes} recipes", dayRecipePairs.Count);
        await _mealPlanningRepository.Create(planOfMeals);

        return planOfMeals;
    }

    public async Task<PlanOfMeals> Update(UpdatePlanOfMealRequest planOfMealRequest)
    {
        var mealPlanToUpdate = await _mealPlanningRepository.GetMealPlanByName(planOfMealRequest.Name);

        if (mealPlanToUpdate == null)
        {
            _logger.LogError("Attempted to update a non-existent meal plan {planOfMealName}", planOfMealRequest.Name);
            throw new NotFoundException(nameof(mealPlanToUpdate));
        }

        var RecipeForMealPlanUpdate = planOfMealRequest.Recipes.SelectMany(x => x.RecipeIds);
        var recipes = _recipesRepository.GetRecipesByIdForMealPlan(RecipeForMealPlanUpdate);

        var existingRecipes = recipes.Select(r => r.Id).ToList();

        var missingRecipes = RecipeForMealPlanUpdate.Except(existingRecipes).ToList();
        if (missingRecipes.Any())
        {
            var missingIdsMessage = "Some recipes do not exist in the database. Missing IDs: " + string.Join(", ", missingRecipes);

            _logger.LogError("Some recipes do not exist in the database. Missing IDs: {MissingRecipeIds}", missingIdsMessage);

            throw new NotFoundException(missingIdsMessage);
        }

        mealPlanToUpdate.Name = planOfMealRequest.Name;
        mealPlanToUpdate.SetDates(planOfMealRequest.FromDate, planOfMealRequest.ToDate);

        var updatedRecipePlans = planOfMealRequest.Recipes
        .SelectMany(dayRecipe => dayRecipe.RecipeIds, (dayRecipe, recipeId) =>
        new RecipePerDay(dayRecipe.Day, recipes.First(r => r.Id == recipeId))).ToList();

        mealPlanToUpdate.SetRecipes(updatedRecipePlans);

        await _mealPlanningRepository.Update(mealPlanToUpdate);

        return mealPlanToUpdate;
    }
}
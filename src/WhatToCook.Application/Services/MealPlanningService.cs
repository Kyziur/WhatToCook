using Microsoft.Extensions.Logging;
using System.Linq;
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

        //var RecipeForMealPlan = planOfMealRequest.Recipes.SelectMany(x => x.RecipeIds).Distinct();
        //var recipes = _recipesRepository.GetRecipesByNameForMealPlan(RecipeForMealPlan);

        //var recipePlans = recipes.Select(r => new RecipePlanOfMeals
        //{
        //    RecipeId = r.Id,
        //    PlanOfMealsId = 0,
        //    Day = planOfMealRequest.Recipes.First(x => x.RecipeIds.Contains(r.Id)).Day,
        //}).ToList();

        var requestedRecipeIds = planOfMealRequest.Recipes.SelectMany(x => x.RecipeIds).ToList();
        var recipes = _recipesRepository.GetRecipesByIdForMealPlan(requestedRecipeIds);
        var dayRecipePairs = planOfMealRequest.Recipes
            .SelectMany(dayRecipe => dayRecipe.RecipeIds,
                        (dayRecipe, recipeId) =>
                        new RecipePerDay(dayRecipe.Day, recipes.First(r => r.Id == recipeId) )).ToList();

        var planOfMeals = new PlanOfMeals
    (
        planOfMealRequest.Name,
        DateTime.SpecifyKind(planOfMealRequest.FromDate, DateTimeKind.Utc),
        DateTime.SpecifyKind(planOfMealRequest.ToDate, DateTimeKind.Utc),
        dayRecipePairs  
        
    );
        _logger.LogInformation($"Creating a meal plan with {dayRecipePairs.Count} recipes.");
        await _mealPlanningRepository.Create(planOfMeals);

        return planOfMeals;
    }

    public async Task<PlanOfMeals> Update(UpdatePlanOfMealRequest planOfMealRequest)
    {
        var mealPlanToUpdate = await _mealPlanningRepository.GetMealPlanByName(planOfMealRequest.Name);
        var RecipeForMealPlanUpdate = planOfMealRequest.Recipes.SelectMany(x => x.RecipeIds);

        var recipes = _recipesRepository.GetRecipesByIdForMealPlan(RecipeForMealPlanUpdate);
        if (mealPlanToUpdate == null)
        {
            _logger.LogError($"Attempted to update a non-existent meal plan: {planOfMealRequest.Name}");
            throw new NotFoundException(nameof(mealPlanToUpdate));
        }
        var existingRecipes = recipes.Select(r => r.Id).ToList();
        if (!existingRecipes.OrderBy(n => n).SequenceEqual(RecipeForMealPlanUpdate.OrderBy(n => n)))
        {
            _logger.LogError($"Some recipes do not exist in the database");
            throw new NotFoundException($"Some recipes do not exist in the database");
        }
        //var updatedRecipePlans = recipes.Select(r => new RecipePlanOfMeals
        //{
        //    RecipeId = r.Id,
        //    PlanOfMealsId = mealPlanToUpdate.Id
        //}).ToList();
        mealPlanToUpdate.Name = planOfMealRequest.Name;
        mealPlanToUpdate.SetDates(planOfMealRequest.FromDate, planOfMealRequest.ToDate);
        //mealPlanToUpdate.RecipePlanOfMeals = updatedRecipePlans;
        await _mealPlanningRepository.Update(mealPlanToUpdate);
        return mealPlanToUpdate;
    }

}
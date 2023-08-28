using Microsoft.EntityFrameworkCore;
using WhatToCook.Application.DataTransferObjects.Requests;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Infrastructure;
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
        var recipesNamesToLookFor = planOfMealRequest.Recipes.Select(x => x.Name);
        var recipies = _recipesRepository.GetByNames(recipesNamesToLookFor);
        
        var planOfMeals = new PlanOfMeals()
        {
            Name = planOfMealRequest.Name,
            Id = planOfMealRequest.Id,
            FromDate = DateTime.SpecifyKind(planOfMealRequest.FromDate, DateTimeKind.Utc),
            ToDate = DateTime.SpecifyKind(planOfMealRequest.ToDate, DateTimeKind.Utc),
            Recipes = recipies
        };

        await _mealPlanningRepository.Create(planOfMeals);
        
        return planOfMeals;
    }
}
using Microsoft.EntityFrameworkCore;
using WhatToCook.Application.DataTransferObjects.Responses;
using WhatToCook.Application.Infrastructure;

namespace WhatToCook.Application.Services;

public class MealPlanningServiceQuery
{
    private readonly DatabaseContext _dbContext;

    public MealPlanningServiceQuery(DatabaseContext dbContext) => _dbContext = dbContext;

    public async Task<ShoppingListResponse> GetIngredientsForMealPlanById(int mealPlanId)
    {
        Domain.PlanOfMeals mealPlan = await _dbContext.PlanOfMeals
            .AsNoTracking()
            .AsSplitQuery()
            .Include(mp => mp.RecipePlanOfMeals)
            .ThenInclude(r => r.Recipe)
            .ThenInclude(x => x.Ingredients)
            .FirstAsync(mp => mp.Id == mealPlanId);

        var dayWiseIngredientsList = mealPlan.RecipePlanOfMeals
            .Select(r => new DayWiseIngredientsResponse(r.Day, r.Recipe.Ingredients
                .Select(i => i.Name)))
            .ToList();

        var shoppingList = new ShoppingListResponse
        { FromDate = mealPlan.FromDate, ToDate = mealPlan.ToDate, IngredientsPerDay = dayWiseIngredientsList };
        return shoppingList;
    }

    public async Task<PlanOfMealResponses> GetAll(CancellationToken token = default)
    {
        var query = await _dbContext.PlanOfMeals
            .AsNoTracking()
            .Include(p => p.RecipePlanOfMeals)
            .ThenInclude(x => x.Recipe)
            .Select(planOfMeal => new
            {
                planOfMeal.Name,
                planOfMeal.Id,
                planOfMeal.FromDate,
                planOfMeal.ToDate,
                Recipes = planOfMeal.RecipePlanOfMeals.Select(recipe =>
                    new
                    {
                        recipe.Day,
                        recipe.RecipeId
                    })
            }).ToListAsync(token);

        var mealPlans = query.Select(x => new PlanOfMealResponse
        {
            Name = x.Name,
            Id = x.Id,
            FromDate = x.FromDate,
            ToDate = x.ToDate,
            Recipes = x.Recipes.GroupBy(mp => mp.Day).Select(grouped => new MealPlanForDayResponse()
            {
                Day = grouped.Key,
                RecipesIds = grouped.Select(r => r.RecipeId)
            })
        }).ToList();

        return new PlanOfMealResponses(mealPlans);
    }

    public async Task<PlanOfMealResponse?> GetByName(string name, CancellationToken token = default)
    {
        var query = await _dbContext.PlanOfMeals
            .AsNoTracking()
            .Include(p => p.RecipePlanOfMeals)
            .ThenInclude(x => x.Recipe)
            .Where(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase))
            .Select(planOfMeal => new
            {
                planOfMeal.Name,
                planOfMeal.Id,
                planOfMeal.FromDate,
                planOfMeal.ToDate,
                Recipes = planOfMeal.RecipePlanOfMeals.Select(recipe =>
                    new
                    {
                        recipe.Day,
                        recipe.RecipeId
                    })
            }).FirstOrDefaultAsync(token);

        return query is null
            ? null
            : new PlanOfMealResponse
            {
                Name = query.Name,
                Id = query.Id,
                FromDate = query.FromDate,
                ToDate = query.ToDate,
                Recipes = query.Recipes.GroupBy(x => x.Day).Select(x => new MealPlanForDayResponse
                {
                    Day = x.Key,
                    RecipesIds = x.Select(r => r.RecipeId)
                })
            };
    }
}
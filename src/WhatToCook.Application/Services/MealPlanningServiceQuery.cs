using Microsoft.EntityFrameworkCore;
using WhatToCook.Application.DataTransferObjects.Responses;
using WhatToCook.Application.Infrastructure;

namespace WhatToCook.Application.Services
{
    public class MealPlanningServiceQuery
    {
        private readonly DatabaseContext _dbcontext;

        public MealPlanningServiceQuery(DatabaseContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<ShoppingListResponse> GetIngredientsForMealPlanById(int mealPlanId)
        {
            var mealPlan = await _dbcontext.PlanOfMeals
                .AsNoTracking()
                .AsSplitQuery()
                .Include(mp => mp.RecipePlanOfMeals)
                .ThenInclude(r => r.Recipe)
                .ThenInclude(x => x.Ingredients)
                .FirstAsync(mp => mp.Id == mealPlanId);

            if (mealPlan == null) return new ShoppingListResponse();

            var dayWiseIngredientsList = mealPlan.RecipePlanOfMeals
                .Select(r => new DayWiseIngredientsResponse(r.Day, r.Recipe.Ingredients
                .Select(i => i.Name)))
                .ToList();

            var shoppingList = new ShoppingListResponse { FromDate = mealPlan.FromDate, ToDate = mealPlan.ToDate, IngredientsPerDay = dayWiseIngredientsList };
            return shoppingList;
        }

        public async Task<List<PlanOfMealResponse>> GetPlanOfMeals()
        {
            var query = await _dbcontext.PlanOfMeals
                .AsNoTracking()
                .Include(p => p.RecipePlanOfMeals)
                .ThenInclude(x => x.Recipe)
                .Select(planOfMeal => new PlanOfMealResponse()
                {
                    Name = planOfMeal.Name,
                    Id = planOfMeal.Id,
                    FromDate = planOfMeal.FromDate,
                    ToDate = planOfMeal.ToDate,
                    Recipes = planOfMeal.RecipePlanOfMeals.Select(recipe => new RecipeInMealPlanResponse(recipe.Recipe.Name))
                }).ToListAsync();
            return query;
        }
    }
}
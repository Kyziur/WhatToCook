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

        public async Task<List<PlanOfMealResponse>> GetPlanOfMeals()
        {
            var query = await _dbcontext.PlanOfMeals.Select(planOfMeal => new PlanOfMealResponse()
            {
                Name = planOfMeal.Name,
                Id = planOfMeal.Id,
                FromDate = planOfMeal.FromDate,
                ToDate = planOfMeal.ToDate,
                Recipes = planOfMeal.Recipes.Select(recipe => new RecipeInMealPlanResponse(recipe.Name))
            }).ToListAsync();
            return query;
        }
        public async Task<ShoppingListResponse?> GetIngredientsForMealPlanById(int mealPlanId)
        {
            var mealPlan = await _dbcontext.PlanOfMeals
                .Include(mp => mp.Recipes)
                .ThenInclude(r => r.Ingredients)
                .FirstOrDefaultAsync(mp => mp.Id == mealPlanId);

            if (mealPlan == null) return null;

            // Assuming each recipe corresponds to a day, this logic should be modified if this isn't the case
            var dayWiseIngredientsList = mealPlan.Recipes.Select(r => new DayWiseIngredients
            {
                Ingredients = r.Ingredients.Select(i => i.Name).ToList()
            }).ToList();

            return new ShoppingListResponse
            {
                ToDate = mealPlan.ToDate,
                FromDate = mealPlan.FromDate,
                Ingredients = mealPlan.Recipes.SelectMany(x => x.Ingredients).Select(i => i.Name).Distinct().ToList(),
                DayWiseIngredientsList = dayWiseIngredientsList
            };
        }
    }
}
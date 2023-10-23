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
            var query = await _dbcontext.PlanOfMeals.Include(p => p.RecipePlanOfMeals).ThenInclude(x => x.Recipe).Select(planOfMeal => new PlanOfMealResponse()
            {
                Name = planOfMeal.Name,
                Id = planOfMeal.Id,
                FromDate = planOfMeal.FromDate,
                ToDate = planOfMeal.ToDate,
                Recipes = planOfMeal.RecipePlanOfMeals.Select(recipe => new RecipeInMealPlanResponse(recipe.Recipe.Name))
            }).ToListAsync();
            return query;
        }
        public async Task<List<DayWiseIngredientsResponse>> GetIngredientsForMealPlanById(int mealPlanId)
        {
            var mealPlan = await _dbcontext.PlanOfMeals
                .Include(mp => mp.RecipePlanOfMeals)
                .ThenInclude(r => r.Recipe)
                .ThenInclude(x => x.Ingredients)
                .FirstOrDefaultAsync(mp => mp.Id == mealPlanId);

            if (mealPlan == null) return new List<DayWiseIngredientsResponse>();

            var dayWiseIngredientsList = mealPlan.RecipePlanOfMeals.Select(r => new DayWiseIngredientsResponse
            {
                Day = r.Day,
                Ingredients = r.Recipe.Ingredients.Select(i => i.Name).ToList()
            }).ToList();

            return dayWiseIngredientsList;
        }
    }
}
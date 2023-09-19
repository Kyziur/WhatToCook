using Microsoft.EntityFrameworkCore;
using WhatToCook.Application.DataTransferObjects.Responses;
using WhatToCook.Application.Domain;
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
    }
}
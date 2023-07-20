using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatToCook.Application.DataTransferObjects.Responses;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Infrastructure;
using WhatToCook.WebApp.DataTransferObject.Responses;

namespace WhatToCook.Application.Services
{
    public class MealPlanningServiceQuery
    {
        private readonly DatabaseContext _dbcontext;
        public MealPlanningServiceQuery(DatabaseContext dbcontext) {

            _dbcontext = dbcontext;
        }
        public async Task<List<PlanOfMealResponse>> GetPlanOfMeals()
        {

            List<PlanOfMeals> planOfMeals = await _dbcontext.PlanOfMeals.Include(b => b.Recipes).ToListAsync();
            List<PlanOfMealResponse> planOfMealsMappingResult = new();
            foreach (var planOfMeal in planOfMeals)
            {
                PlanOfMealResponse planOfMealResponse = PlanOfMealResponse.MapFrom(planOfMeal);
                planOfMealsMappingResult.Add(planOfMealResponse);
            }

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

using Azure.Core;
using Microsoft.EntityFrameworkCore;
using WhatToCook.Application.DataTransferObjects.Requests;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Services;

namespace WhatToCook.Application.DataTransferObjects.Responses
{
    public record RecipeInMealPlanResponse(string Name);

    public class PlanOfMealResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public IEnumerable<RecipeInMealPlanResponse> Recipes { get; set; }
        public static PlanOfMealResponse MapFrom(PlanOfMeals planOfMeals)
        {
          
            return new PlanOfMealResponse
            {
                Id = planOfMeals.Id,
                Name = planOfMeals.Name,
                Recipes = planOfMeals.Recipes.Select(recipe => new RecipeInMealPlanResponse(recipe.Name)),
                FromDate = DateTime.SpecifyKind(planOfMeals.FromDate, DateTimeKind.Utc),
                ToDate = DateTime.SpecifyKind(planOfMeals.ToDate, DateTimeKind.Utc)
             };
        }
    }
}


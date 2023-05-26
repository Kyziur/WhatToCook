using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatToCook.Application.Domain;
using WhatToCook.WebApp.DataTransferObject.Responses;

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
                Recipes = planOfMeals.Recipes.Select(x => new RecipeInMealPlanResponse(x.Name)),
                FromDate = planOfMeals.FromDate,
                ToDate = planOfMeals.ToDate
            };
        }
    }
}


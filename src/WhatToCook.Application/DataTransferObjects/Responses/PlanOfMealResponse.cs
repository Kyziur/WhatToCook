using WhatToCook.Application.Domain;

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
                Recipes = planOfMeals.RecipePlanOfMeals.Select(r => new RecipeInMealPlanResponse(r.Recipe.Name)),
                FromDate = DateTime.SpecifyKind(planOfMeals.FromDate, DateTimeKind.Utc),
                ToDate = DateTime.SpecifyKind(planOfMeals.ToDate, DateTimeKind.Utc)
            };
        }
    }
}


namespace WhatToCook.Application.Domain
{
    public class RecipePlanOfMeals
    {
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
        public int PlanOfMealsId { get; set; }
        public PlanOfMeals PlanOfMeals { get; set; }
        public DateTime Day { get; set; }
        public RecipePlanOfMeals(Recipe recipe, PlanOfMeals planOfMeals, DateTime day)
        {
            Recipe = recipe;
            PlanOfMeals = planOfMeals;
            Day = day;
        }
        private RecipePlanOfMeals()
        {
             
        }
    }
}

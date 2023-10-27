namespace WhatToCook.Application.Domain
{
    public class RecipePlanOfMeals
    {
        public int RecipeId { get; private set; }
        public Recipe Recipe { get; private set; }
        public int PlanOfMealsId { get; private set; }
        public PlanOfMeals PlanOfMeals { get; set; }
        public DateTime Day { get; private set; }

        public RecipePlanOfMeals(Recipe recipe, PlanOfMeals planOfMeals, DateTime day)
        {
            SetRecipe(recipe);
            SetPlanOfMeals(planOfMeals);
            SetDay(day);
        }

        private RecipePlanOfMeals()
        { }

        public void SetRecipe(Recipe recipe)
        {
            Recipe = recipe ?? throw new ArgumentNullException(nameof(recipe), "Recipe cannot be null.");
        }

        public void SetPlanOfMeals(PlanOfMeals planOfMeals)
        {
            PlanOfMeals = planOfMeals ?? throw new ArgumentNullException(nameof(planOfMeals), "PlanOfMeals cannot be null.");
        }

        public void SetDay(DateTime day)
        {
            Day = day;
        }
    }
}
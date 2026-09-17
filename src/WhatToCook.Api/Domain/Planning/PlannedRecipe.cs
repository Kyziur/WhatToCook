using WhatToCook.Api.Domain.Recipes;

namespace WhatToCook.Api.Domain.Planning;

public sealed class PlannedRecipe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MealPlanId { get; set; }
    public MealPlan MealPlan { get; set; } = null!;
    public Guid RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public DateOnly PlannedDate { get; set; }
    public decimal Multiplier { get; set; } = 1m;
    public int SortOrder { get; set; }
}

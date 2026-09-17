using WhatToCook.Api.Domain.Shopping;

namespace WhatToCook.Api.Domain.Planning;

public sealed class MealPlan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateOnly DateFrom { get; set; }
    public DateOnly DateTo { get; set; }
    public bool HasGeneratedShoppingList { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<PlannedRecipe> PlannedRecipes { get; set; } = [];
    public ShoppingList? ShoppingList { get; set; }
}

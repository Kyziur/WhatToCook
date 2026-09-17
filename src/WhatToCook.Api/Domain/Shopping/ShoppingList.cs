using WhatToCook.Api.Domain.Planning;

namespace WhatToCook.Api.Domain.Shopping;

public sealed class ShoppingList
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MealPlanId { get; set; }
    public MealPlan MealPlan { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<ShoppingListItem> Items { get; set; } = [];
}

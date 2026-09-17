namespace WhatToCook.Api.Domain.Shopping;

public sealed class ShoppingListItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ShoppingListId { get; set; }
    public ShoppingList ShoppingList { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? NormalizedName { get; set; }
    public string? QuantityText { get; set; }
    public string? Unit { get; set; }
    public ChecklistState State { get; set; } = ChecklistState.NieMam;
    public bool IsManual { get; set; }
    public int SortOrder { get; set; }
}

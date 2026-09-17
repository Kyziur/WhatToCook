namespace WhatToCook.Api.Domain.Recipes;

public sealed class RecipeIngredient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RecipeId { get; set; }
    public Guid IngredientId { get; set; }
    public string? QuantityText { get; set; }
    public string? Unit { get; set; }
    public int SortOrder { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public Ingredient Ingredient { get; set; } = null!;
}

namespace WhatToCook.Api.Domain.Recipes;

public sealed class Ingredient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DisplayName { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public List<RecipeIngredient> RecipeIngredients { get; set; } = [];
}

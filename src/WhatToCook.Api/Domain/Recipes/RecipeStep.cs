namespace WhatToCook.Api.Domain.Recipes;

public sealed class RecipeStep
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RecipeId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public Recipe Recipe { get; set; } = null!;
}

namespace WhatToCook.Api.Domain.Recipes;

public sealed class RecipeTag
{
    public Guid RecipeId { get; set; }
    public Guid TagId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}

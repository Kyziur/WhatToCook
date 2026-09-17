namespace WhatToCook.Api.Domain.Recipes;

public sealed class Tag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DisplayName { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public List<RecipeTag> RecipeTags { get; set; } = [];
}

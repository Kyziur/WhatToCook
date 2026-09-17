namespace WhatToCook.Api.Domain.Recipes;

public sealed class RecipeImportIngredient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DraftId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? QuantityText { get; set; }
    public string? Unit { get; set; }
    public int SortOrder { get; set; }
    public RecipeImportDraft Draft { get; set; } = null!;
}

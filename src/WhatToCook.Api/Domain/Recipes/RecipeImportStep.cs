namespace WhatToCook.Api.Domain.Recipes;

public sealed class RecipeImportStep
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DraftId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public RecipeImportDraft Draft { get; set; } = null!;
}

namespace WhatToCook.Api.Domain.Recipes;

public sealed class RecipeImportIssue
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DraftId { get; set; }
    public string FieldPath { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public RecipeImportIssueSeverity Severity { get; set; } = RecipeImportIssueSeverity.Blocker;
    public int SortOrder { get; set; }
    public RecipeImportDraft Draft { get; set; } = null!;
}

public enum RecipeImportIssueSeverity
{
    Blocker = 1,
    Warning = 2,
}

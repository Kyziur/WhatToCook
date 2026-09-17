namespace WhatToCook.Api.Domain.Recipes;

public sealed class RecipeImportDraft
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public RecipeImportSourceType SourceType { get; set; }
    public RecipeImportStatus Status { get; set; } = RecipeImportStatus.NeedsReview;
    public string? SourceUrl { get; set; }
    public string? RawContent { get; set; }
    public string? Title { get; set; }
    public int? Servings { get; set; }
    public string? Source { get; set; }
    public Guid? FinalizedRecipeId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<RecipeImportIngredient> Ingredients { get; set; } = [];
    public List<RecipeImportStep> Steps { get; set; } = [];
    public List<RecipeImportTag> Tags { get; set; } = [];
    public List<RecipeImportIssue> Issues { get; set; } = [];
}

public enum RecipeImportSourceType
{
    Url = 1,
    Photo = 2,
}

public enum RecipeImportStatus
{
    NeedsReview = 1,
    ReadyToFinalize = 2,
    Finalized = 3,
}

namespace WhatToCook.Api.IntegrationTests.Specs;

internal static class RecipeApiContracts
{
    public static RecipeUpsertPayload CreateRecipePayload(string title)
    {
        return new RecipeUpsertPayload
        {
            Title = title,
            Servings = 4,
            Source = "https://example.com",
            Ingredients = [new RecipeIngredientPayload("Pomidor", "2", "szt")],
            Steps = ["Pokroj skladniki", "Wymieszaj"],
            Tags = ["obiad", "szybkie"],
        };
    }

    internal sealed class RecipeUpsertPayload
    {
        public string Title { get; set; } = string.Empty;
        public int Servings { get; set; }
        public string? Source { get; set; }
        public List<RecipeIngredientPayload> Ingredients { get; set; } = [];
        public List<string> Steps { get; set; } = [];
        public List<string> Tags { get; set; } = [];
    }

    internal sealed class RecipeIngredientPayload(string name, string? quantityText, string? unit)
    {
        public string Name { get; set; } = name;
        public string? QuantityText { get; set; } = quantityText;
        public string? Unit { get; set; } = unit;
    }

    internal sealed class RecipeSummaryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? MainPhotoPath { get; set; }
        public bool IsActive { get; set; }
        public IReadOnlyList<string> Tags { get; set; } = [];
        public IReadOnlyList<string> Ingredients { get; set; } = [];
    }

    internal sealed class RecipeDetailsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Servings { get; set; }
        public string? Source { get; set; }
        public string? MainPhotoPath { get; set; }
        public bool IsActive { get; set; }
        public IReadOnlyList<RecipeIngredientDto> Ingredients { get; set; } = [];
        public IReadOnlyList<string> Steps { get; set; } = [];
    }

    internal sealed class RecipeIngredientDto
    {
        public string Ingredient { get; set; } = string.Empty;
        public string? QuantityText { get; set; }
        public string? Unit { get; set; }
    }

    internal sealed class CreateRecipeImportFromUrlPayload
    {
        public string Url { get; set; } = string.Empty;
    }

    internal sealed class RecipeImportDraftDto
    {
        public Guid Id { get; set; }
        public string SourceType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? SourceUrl { get; set; }
        public string? Title { get; set; }
        public int? Servings { get; set; }
        public string? Source { get; set; }
        public bool CanFinalize { get; set; }
        public Guid? FinalizedRecipeId { get; set; }
        public IReadOnlyList<RecipeImportIngredientDto> Ingredients { get; set; } = [];
        public IReadOnlyList<string> Steps { get; set; } = [];
        public IReadOnlyList<string> Tags { get; set; } = [];
        public IReadOnlyList<RecipeImportIssueDto> Issues { get; set; } = [];
    }

    internal sealed class RecipeImportIngredientDto
    {
        public string Name { get; set; } = string.Empty;
        public string? QuantityText { get; set; }
        public string? Unit { get; set; }
    }

    internal sealed class RecipeImportIssueDto
    {
        public string FieldPath { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
    }

    internal sealed class UpdateRecipeImportDraftPayload
    {
        public string? Title { get; set; }
        public int? Servings { get; set; }
        public string? Source { get; set; }
        public List<RecipeImportIngredientPayload> Ingredients { get; set; } = [];
        public List<string> Steps { get; set; } = [];
        public List<string> Tags { get; set; } = [];
    }

    internal sealed class RecipeImportIngredientPayload(
        string name,
        string? quantityText,
        string? unit
    )
    {
        public string Name { get; set; } = name;
        public string? QuantityText { get; set; } = quantityText;
        public string? Unit { get; set; } = unit;
    }
}

using Microsoft.AspNetCore.Components;
using WhatToCook.Web.Components.Features.Recipes.Import.Services;

namespace WhatToCook.Web.Components.Features.Recipes.Import.Pages;

public partial class RecipeImportReviewPage : IDisposable
{
    private static readonly IReadOnlyList<UnitOption> AllowedUnits =
    [
        new("", "brak jednostki"),
        new("szt", "szt."),
        new("g", "g"),
        new("kg", "kg"),
        new("ml", "ml"),
        new("l", "l"),
        new("lyzeczka", "łyżeczka"),
        new("lyzka", "łyżka"),
        new("szklanka", "szklanka"),
        new("opakowanie", "opakowanie"),
    ];

    [Inject]
    private IRecipeImportService RecipeImportService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public Guid DraftId { get; set; }

    private string? Title { get; set; }
    private string ServingsText { get; set; } = string.Empty;
    private string? Source { get; set; }
    private string? SourceUrl { get; set; }
    private string DraftStatusLabel { get; set; } = string.Empty;
    private Guid? FinalizedRecipeId { get; set; }
    private List<IngredientRow> Ingredients { get; set; } = [];
    private List<StepRow> Steps { get; set; } = [];
    private List<string> Tags { get; set; } = [];
    private List<RecipeImportIssue> Issues { get; set; } = [];
    private string NewTag { get; set; } = string.Empty;
    private string FeedbackMessage { get; set; } = string.Empty;
    private bool IsError { get; set; }
    private bool IsLoading { get; set; }
    private bool IsSaving { get; set; }
    private bool LoadFailed { get; set; }
    private bool CanFinalize { get; set; }
    private readonly CancellationTokenSource lifetimeCts = new();

    public void Dispose()
    {
        lifetimeCts.Cancel();
        lifetimeCts.Dispose();
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadDraftAsync();
    }

    private async Task LoadDraftAsync()
    {
        IsLoading = true;
        LoadFailed = false;
        FeedbackMessage = string.Empty;

        RecipeImportDraft? draft;
        try
        {
            draft = await RecipeImportService.GetDraftAsync(DraftId, lifetimeCts.Token);
        }
        catch (OperationCanceledException) when (lifetimeCts.IsCancellationRequested)
        {
            return;
        }
        catch
        {
            IsLoading = false;
            LoadFailed = true;
            FeedbackMessage = "Nie udało się załadować szkicu importu. Spróbuj ponownie za chwilę.";
            IsError = true;
            return;
        }
        IsLoading = false;

        if (draft is null)
        {
            LoadFailed = true;
            return;
        }

        ApplyDraft(draft);
    }

    private async Task SaveDraftAsync()
    {
        if (IsSaving)
        {
            return;
        }

        IsSaving = true;
        FeedbackMessage = string.Empty;
        IsError = false;

        RecipeImportUpdateResult result;
        try
        {
            result = await RecipeImportService.UpdateDraftAsync(
                DraftId,
                BuildRequest(),
                lifetimeCts.Token
            );
        }
        catch (OperationCanceledException) when (lifetimeCts.IsCancellationRequested)
        {
            IsSaving = false;
            return;
        }
        catch
        {
            IsSaving = false;
            FeedbackMessage = "Nie udało się zapisać szkicu importu. Spróbuj ponownie za chwilę.";
            IsError = true;
            return;
        }
        IsSaving = false;

        if (!result.IsSuccess || result.Draft is null)
        {
            FeedbackMessage = result.ErrorMessage ?? "Nie udalo sie zapisac szkicu.";
            IsError = true;
            return;
        }

        ApplyDraft(result.Draft);
        FeedbackMessage = "Szkic importu zostal zapisany.";
    }

    private async Task FinalizeAsync()
    {
        if (IsSaving || !CanFinalize)
        {
            return;
        }

        IsSaving = true;
        FeedbackMessage = string.Empty;
        IsError = false;

        RecipeImportFinalizeResult result;
        try
        {
            result = await RecipeImportService.FinalizeDraftAsync(DraftId, lifetimeCts.Token);
        }
        catch (OperationCanceledException) when (lifetimeCts.IsCancellationRequested)
        {
            IsSaving = false;
            return;
        }
        catch
        {
            IsSaving = false;
            FeedbackMessage = "Nie udało się sfinalizować importu. Spróbuj ponownie za chwilę.";
            IsError = true;
            return;
        }
        IsSaving = false;

        if (!result.IsSuccess || result.Recipe is null)
        {
            if (result.Draft is not null)
            {
                ApplyDraft(result.Draft);
            }

            FeedbackMessage = result.ErrorMessage ?? "Nie udalo sie sfinalizowac importu.";
            IsError = true;
            return;
        }

        var returnUrl = Uri.EscapeDataString("/recipes/import");
        NavigationManager.NavigateTo($"/recipes/{result.Recipe.Id}?returnUrl={returnUrl}");
    }

    private RecipeImportDraftUpdateRequest BuildRequest()
    {
        return new RecipeImportDraftUpdateRequest(
            string.IsNullOrWhiteSpace(Title) ? null : Title.Trim(),
            int.TryParse(ServingsText, out var servings) ? servings : null,
            string.IsNullOrWhiteSpace(Source) ? null : Source.Trim(),
            Ingredients
                .Select(x => new RecipeImportIngredient(
                    x.Name.Trim(),
                    string.IsNullOrWhiteSpace(x.QuantityText) ? null : x.QuantityText.Trim(),
                    string.IsNullOrWhiteSpace(x.Unit) ? null : x.Unit.Trim()
                ))
                .ToList(),
            Steps.Select(x => x.Text.Trim()).ToList(),
            Tags.ToList()
        );
    }

    private void ApplyDraft(RecipeImportDraft draft)
    {
        Title = draft.Title;
        ServingsText = draft.Servings?.ToString() ?? string.Empty;
        Source = draft.Source;
        SourceUrl = draft.SourceUrl;
        DraftStatusLabel = draft.Status;
        FinalizedRecipeId = draft.FinalizedRecipeId;
        CanFinalize = draft.CanFinalize;
        Issues = draft.Issues.ToList();
        Ingredients = draft
            .Ingredients.Select(x => new IngredientRow
            {
                Name = x.Name,
                QuantityText = x.QuantityText,
                Unit = string.IsNullOrWhiteSpace(x.Unit) ? string.Empty : x.Unit,
            })
            .ToList();
        Steps = draft.Steps.Select(x => new StepRow { Text = x }).ToList();
        Tags = draft.Tags.ToList();
    }

    private void AddIngredient()
    {
        Ingredients.Add(new IngredientRow());
    }

    private void RemoveIngredient(Guid ingredientId)
    {
        Ingredients.RemoveAll(x => x.Id == ingredientId);
    }

    private void AddStep()
    {
        Steps.Add(new StepRow());
    }

    private void RemoveStep(Guid stepId)
    {
        Steps.RemoveAll(x => x.Id == stepId);
    }

    private void MergeStepWithPrevious(Guid stepId)
    {
        var index = Steps.FindIndex(x => x.Id == stepId);
        if (index <= 0)
        {
            return;
        }

        var previous = Steps[index - 1];
        var current = Steps[index];
        previous.Text = JoinStepTexts(previous.Text, current.Text);
        Steps.RemoveAt(index);
    }

    private void AddTag()
    {
        var trimmed = NewTag.Trim();
        if (trimmed.Length == 0 || Tags.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        Tags.Add(trimmed);
        NewTag = string.Empty;
    }

    private void RemoveTag(string tag)
    {
        Tags.RemoveAll(x => string.Equals(x, tag, StringComparison.OrdinalIgnoreCase));
    }

    private string GetRecipeLink(Guid recipeId)
    {
        var returnUrl = Uri.EscapeDataString("/recipes/import");
        return $"/recipes/{recipeId}?returnUrl={returnUrl}";
    }

    private sealed class IngredientRow
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string? QuantityText { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    private sealed class StepRow
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Text { get; set; } = string.Empty;
    }

    private sealed record UnitOption(string Value, string Label);

    private static string JoinStepTexts(string first, string second)
    {
        var trimmedFirst = first.Trim();
        var trimmedSecond = second.Trim();
        if (trimmedFirst.Length == 0)
        {
            return trimmedSecond;
        }

        if (trimmedSecond.Length == 0)
        {
            return trimmedFirst;
        }

        return $"{trimmedFirst}{Environment.NewLine}{Environment.NewLine}{trimmedSecond}";
    }
}

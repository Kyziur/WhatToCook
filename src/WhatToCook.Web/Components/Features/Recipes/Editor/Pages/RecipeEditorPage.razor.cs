using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using WhatToCook.Web.Components.Features.Recipes.Editor.Services;
using WhatToCook.Web.Components.Shared;

namespace WhatToCook.Web.Components.Features.Recipes.Editor.Pages;

public partial class RecipeEditorPage : IAsyncDisposable
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
    private IJSRuntime JsRuntime { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private IRecipeEditorService RecipeEditorService { get; set; } = null!;

    [Parameter]
    public Guid? RecipeId { get; set; }

    [SupplyParameterFromQuery(Name = "returnUrl")]
    public string? ReturnUrl { get; set; }

    private RecipeEditorModel Model { get; } = new();
    private EditContext EditContext { get; set; } = null!;
    private string FeedbackMessage { get; set; } = string.Empty;
    private bool FeedbackIsError { get; set; }
    private bool IsDirty { get; set; }
    private bool IsSaving { get; set; }
    private bool IsInteractive { get; set; }
    private bool IsLoading { get; set; }
    private bool HasAttemptedSubmit { get; set; }
    private bool FocusFeedbackAfterRender { get; set; }
    private Guid? LoadedRecipeId { get; set; }
    private List<string> ServerValidationErrors { get; } = [];
    private IBrowserFile? SelectedPhoto { get; set; }
    private InputFile? PhotoInput { get; set; }
    private string? SelectedPhotoName { get; set; }
    private string? SelectedPhotoPreviewUrl { get; set; }
    private string? CurrentPhotoPath { get; set; }
    private string NewTag { get; set; } = string.Empty;
    private IReadOnlyList<string> IngredientSuggestions { get; set; } = [];
    private IReadOnlyList<string> TagSuggestions { get; set; } = [];
    private InputText? TitleInput { get; set; }
    private InputNumber<int>? ServingsInput { get; set; }
    private ElementReference ValidationSummaryElement { get; set; }
    private ElementReference FeedbackElement { get; set; }
    private readonly CancellationTokenSource lifetimeCts = new();

    private IReadOnlyList<string> FilteredTagSuggestions =>
        TagSuggestions
            .Where(tag => !Model.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            .Where(tag =>
                string.IsNullOrWhiteSpace(NewTag)
                || SearchTextNormalizer
                    .Normalize(tag)
                    .Contains(SearchTextNormalizer.Normalize(NewTag), StringComparison.Ordinal)
            )
            .Take(10)
            .ToList();

    private bool HasUnsavedChanges => IsDirty;

    public async ValueTask DisposeAsync()
    {
        lifetimeCts.Cancel();
        lifetimeCts.Dispose();

        try
        {
            await ReleaseSelectedPhotoPreviewAsync();
        }
        catch (JSDisconnectedException) { }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            IsInteractive = true;
            await InvokeAsync(StateHasChanged);
        }

        if (FocusFeedbackAfterRender)
        {
            FocusFeedbackAfterRender = false;
            await FeedbackElement.FocusAsync();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        EditContext = CreateEditContext();

        if (Model.Ingredients.Count == 0)
        {
            AddIngredient();
        }

        if (Model.Steps.Count == 0)
        {
            AddStep();
        }

        await RefreshIngredientSuggestionsAsync(string.Empty);
        await RefreshTagSuggestionsAsync();
        IsDirty = false;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!RecipeId.HasValue || LoadedRecipeId == RecipeId)
        {
            return;
        }

        IsLoading = true;
        FeedbackMessage = string.Empty;
        FeedbackIsError = false;
        ServerValidationErrors.Clear();

        RecipeEditorDraft? recipe;
        try
        {
            recipe = await RecipeEditorService.GetByIdAsync(RecipeId.Value, lifetimeCts.Token);
        }
        catch (OperationCanceledException) when (lifetimeCts.IsCancellationRequested)
        {
            return;
        }
        catch
        {
            FeedbackMessage =
                "Nie udało się pobrać przepisu do edycji. Spróbuj ponownie za chwilę.";
            FeedbackIsError = true;
            IsLoading = false;
            return;
        }
        if (recipe is null)
        {
            FeedbackMessage = "Nie znaleziono przepisu do edycji.";
            FeedbackIsError = true;
            IsLoading = false;
            return;
        }

        LoadedRecipeId = recipe.Id;
        Model.Title = recipe.Title;
        Model.Servings = recipe.Servings;
        Model.Source = recipe.Source;
        Model.Tags = recipe.Tags.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        Model.Ingredients = recipe
            .Ingredients.Select(
                (x, index) => IngredientRow.From(index, x.Name, x.QuantityText, x.Unit)
            )
            .ToList();
        Model.Steps = recipe.Steps.Select((x, index) => StepRow.From(index, x)).ToList();
        CurrentPhotoPath = recipe.MainPhotoPath;
        IsDirty = false;
        IsLoading = false;

        EditContext = CreateEditContext();
        await RefreshIngredientSuggestionsAsync(string.Empty);
        await RefreshTagSuggestionsAsync();
    }

    private EditContext CreateEditContext()
    {
        var editContext = new EditContext(Model);
        editContext.OnFieldChanged += (_, _) => IsDirty = true;
        return editContext;
    }

    private async Task SaveRecipeAsync()
    {
        if (IsSaving)
        {
            return;
        }

        IsSaving = true;
        HasAttemptedSubmit = true;
        FeedbackMessage = string.Empty;
        FeedbackIsError = false;
        ServerValidationErrors.Clear();

        try
        {
            if (!EditContext.Validate())
            {
                IsSaving = false;
                await FocusFirstDataAnnotationErrorAsync();
                return;
            }

            var hasIngredient = Model.Ingredients.Any(x => !string.IsNullOrWhiteSpace(x.Name));
            var hasStep = Model.Steps.Any(x => !string.IsNullOrWhiteSpace(x.Text));
            if (!hasIngredient || !hasStep)
            {
                if (!hasIngredient)
                {
                    ServerValidationErrors.Add("Dodaj co najmniej jeden składnik.");
                }

                if (!hasStep)
                {
                    ServerValidationErrors.Add("Dodaj co najmniej jeden krok.");
                }

                IsSaving = false;
                await InvokeAsync(StateHasChanged);
                await FocusFirstBusinessErrorAsync(hasIngredient, hasStep);
                return;
            }

            var request = new RecipeSaveRequest(
                Model.Title.Trim(),
                Model.Servings,
                string.IsNullOrWhiteSpace(Model.Source) ? null : Model.Source.Trim(),
                Model
                    .Ingredients.OrderBy(x => x.SortOrder)
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                    .Select(x => new RecipeSaveIngredient(
                        x.Name.Trim(),
                        string.IsNullOrWhiteSpace(x.QuantityText) ? null : x.QuantityText.Trim(),
                        string.IsNullOrWhiteSpace(x.Unit) ? null : x.Unit.Trim()
                    ))
                    .ToList(),
                Model
                    .Steps.OrderBy(x => x.SortOrder)
                    .Select(x => x.Text.Trim())
                    .Where(x => x.Length > 0)
                    .ToList(),
                Model.Tags
            );

            var saveResult = await RecipeEditorService.SaveAsync(
                RecipeId,
                request,
                lifetimeCts.Token
            );

            if (!saveResult.IsSuccess || saveResult.RecipeId is null)
            {
                IsSaving = false;
                FeedbackMessage = saveResult.ErrorMessage ?? "Nie udało się zapisać przepisu.";
                FeedbackIsError = true;
                ServerValidationErrors.AddRange(
                    saveResult.ValidationErrors.SelectMany(x => x.Value).Distinct()
                );
                FocusFeedbackAfterRender = ServerValidationErrors.Count == 0;
                await InvokeAsync(StateHasChanged);
                if (ServerValidationErrors.Count > 0)
                {
                    await ValidationSummaryElement.FocusAsync();
                }
                return;
            }

            if (SelectedPhoto is not null)
            {
                await using var stream = SelectedPhoto.OpenReadStream(
                    maxAllowedSize: 10 * 1024 * 1024
                );
                var uploadResult = await RecipeEditorService.UploadPhotoAsync(
                    saveResult.RecipeId.Value,
                    stream,
                    SelectedPhoto.Name,
                    SelectedPhoto.ContentType,
                    lifetimeCts.Token
                );

                if (!uploadResult.IsSuccess)
                {
                    IsSaving = false;
                    FeedbackMessage =
                        uploadResult.ErrorMessage
                        ?? "Przepis zapisany, ale zdjęcie nie zostało przesłane.";
                    FeedbackIsError = true;
                    return;
                }

                await ReleaseSelectedPhotoPreviewAsync();
                SelectedPhoto = null;
                SelectedPhotoName = null;
                CurrentPhotoPath = uploadResult.MainPhotoPath;
            }

            IsSaving = false;
            IsDirty = false;
            HasAttemptedSubmit = false;
            FeedbackMessage = RecipeId.HasValue
                ? "Zmiany przepisu zostały zapisane."
                : "Nowy przepis został zapisany.";
            FeedbackIsError = false;

            if (!RecipeId.HasValue)
            {
                var encodedReturnUrl = Uri.EscapeDataString(SafeReturnUrl);
                NavigationManager.NavigateTo(
                    $"/recipes/{saveResult.RecipeId.Value}?returnUrl={encodedReturnUrl}"
                );
            }
        }
        catch (OperationCanceledException) when (lifetimeCts.IsCancellationRequested)
        {
            IsSaving = false;
        }
        catch
        {
            IsSaving = false;
            FeedbackMessage = "Nie udało się zapisać przepisu. Spróbuj ponownie za chwilę.";
            FeedbackIsError = true;
        }
    }

    private void AddIngredient()
    {
        Model.Ingredients.Add(
            IngredientRow.From(Model.Ingredients.Count, string.Empty, string.Empty, string.Empty)
        );
        IsDirty = true;
    }

    private void RemoveIngredient(Guid ingredientId)
    {
        if (Model.Ingredients.Count == 1)
        {
            return;
        }

        Model.Ingredients.RemoveAll(x => x.Id == ingredientId);
        ReorderIngredients();
        IsDirty = true;
    }

    private void AddStep()
    {
        Model.Steps.Add(StepRow.From(Model.Steps.Count, string.Empty));
        IsDirty = true;
    }

    private void RemoveStep(Guid stepId)
    {
        if (Model.Steps.Count == 1)
        {
            return;
        }

        Model.Steps.RemoveAll(x => x.Id == stepId);
        ReorderSteps();
        IsDirty = true;
    }

    private void MoveStepUp(Guid stepId)
    {
        var index = Model.Steps.FindIndex(x => x.Id == stepId);
        if (index <= 0)
        {
            return;
        }

        (Model.Steps[index - 1], Model.Steps[index]) = (Model.Steps[index], Model.Steps[index - 1]);
        ReorderSteps();
        IsDirty = true;
    }

    private void MoveStepDown(Guid stepId)
    {
        var index = Model.Steps.FindIndex(x => x.Id == stepId);
        if (index < 0 || index == Model.Steps.Count - 1)
        {
            return;
        }

        (Model.Steps[index + 1], Model.Steps[index]) = (Model.Steps[index], Model.Steps[index + 1]);
        ReorderSteps();
        IsDirty = true;
    }

    private void AddTag()
    {
        var tag = NewTag.Trim();
        if (tag.Length == 0 || Model.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        Model.Tags.Add(tag);
        NewTag = string.Empty;
        IsDirty = true;
        _ = RefreshTagSuggestionsAsync();
    }

    private void AddSuggestedTag(string suggestion)
    {
        NewTag = suggestion;
        AddTag();
    }

    private void RemoveTag(string tag)
    {
        Model.Tags.RemoveAll(x => string.Equals(x, tag, StringComparison.OrdinalIgnoreCase));
        IsDirty = true;
    }

    private async Task RefreshIngredientSuggestionsAsync(string? phrase)
    {
        try
        {
            IngredientSuggestions = await RecipeEditorService.GetIngredientSuggestionsAsync(
                phrase ?? string.Empty,
                lifetimeCts.Token
            );
        }
        catch (OperationCanceledException) when (lifetimeCts.IsCancellationRequested) { }
        catch
        {
            IngredientSuggestions = [];
            FeedbackMessage = "Nie udało się pobrać sugestii składników.";
            FeedbackIsError = true;
        }
    }

    private async Task RefreshTagSuggestionsAsync()
    {
        try
        {
            TagSuggestions = await RecipeEditorService.GetTagSuggestionsAsync(
                NewTag,
                lifetimeCts.Token
            );
        }
        catch (OperationCanceledException) when (lifetimeCts.IsCancellationRequested) { }
        catch
        {
            TagSuggestions = [];
            FeedbackMessage = "Nie udało się pobrać sugestii tagów.";
            FeedbackIsError = true;
        }
    }

    private async Task OnIngredientNameInput(ChangeEventArgs args, IngredientRow ingredient)
    {
        ingredient.Name = args.Value?.ToString() ?? string.Empty;
        IsDirty = true;
        await RefreshIngredientSuggestionsAsync(ingredient.Name);
    }

    private async Task OnNewTagInput(ChangeEventArgs args)
    {
        NewTag = args.Value?.ToString() ?? string.Empty;
        await RefreshTagSuggestionsAsync();
    }

    private Task OnNewTagKeyDown(KeyboardEventArgs args)
    {
        if (
            string.Equals(args.Key, "Enter", StringComparison.OrdinalIgnoreCase)
            || string.Equals(args.Key, "NumpadEnter", StringComparison.OrdinalIgnoreCase)
        )
        {
            AddTag();
        }

        return Task.CompletedTask;
    }

    private async Task HandlePhotoSelected(InputFileChangeEventArgs args)
    {
        await ReleaseSelectedPhotoPreviewAsync();
        SelectedPhoto = args.File;
        SelectedPhotoName = args.File.Name;
        SelectedPhotoPreviewUrl = await JsRuntime.InvokeAsync<string?>(
            "whatToCook.createObjectUrl",
            PhotoInput!.Element
        );
        IsDirty = true;
    }

    private async Task ReleaseSelectedPhotoPreviewAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedPhotoPreviewUrl))
        {
            return;
        }

        await JsRuntime.InvokeVoidAsync("whatToCook.revokeObjectUrl", SelectedPhotoPreviewUrl);
        SelectedPhotoPreviewUrl = null;
    }

    private void ReorderIngredients()
    {
        for (var index = 0; index < Model.Ingredients.Count; index++)
        {
            Model.Ingredients[index].SortOrder = index;
        }
    }

    private void ReorderSteps()
    {
        for (var index = 0; index < Model.Steps.Count; index++)
        {
            Model.Steps[index].SortOrder = index;
        }
    }

    private bool HasFieldError(string propertyName) =>
        HasAttemptedSubmit
        && EditContext.GetValidationMessages(new FieldIdentifier(Model, propertyName)).Any();

    private bool IsFirstIngredientInvalid(IngredientRow ingredient) =>
        HasAttemptedSubmit
        && Model.Ingredients.Count > 0
        && Model.Ingredients[0].Id == ingredient.Id
        && Model.Ingredients.All(x => string.IsNullOrWhiteSpace(x.Name));

    private bool IsFirstStepInvalid(StepRow step) =>
        HasAttemptedSubmit
        && Model.Steps.Count > 0
        && Model.Steps[0].Id == step.Id
        && Model.Steps.All(x => string.IsNullOrWhiteSpace(x.Text));

    private async Task FocusFirstDataAnnotationErrorAsync()
    {
        await InvokeAsync(StateHasChanged);
        if (HasFieldError(nameof(Model.Title)) && TitleInput?.Element is { } titleElement)
        {
            await titleElement.FocusAsync();
            return;
        }

        if (HasFieldError(nameof(Model.Servings)) && ServingsInput?.Element is { } servingsElement)
        {
            await servingsElement.FocusAsync();
        }
    }

    private async Task FocusFirstBusinessErrorAsync(bool hasIngredient, bool hasStep)
    {
        if (!hasIngredient && Model.Ingredients.Count > 0)
        {
            await Model.Ingredients[0].InputElement.FocusAsync();
            return;
        }

        if (!hasStep && Model.Steps.FirstOrDefault()?.Input?.Element is { } stepElement)
        {
            await stepElement.FocusAsync();
        }
    }

    private async Task ConfirmInternalNavigation(LocationChangingContext context)
    {
        if (!HasUnsavedChanges)
        {
            return;
        }

        var shouldLeave = await JsRuntime.InvokeAsync<bool>(
            "confirm",
            "Masz niezapisane zmiany. Czy na pewno chcesz opuścić edytor?"
        );

        if (!shouldLeave)
        {
            context.PreventNavigation();
        }
    }

    private string SafeReturnUrl =>
        string.IsNullOrWhiteSpace(ReturnUrl)
        || !ReturnUrl.StartsWith("/", StringComparison.Ordinal)
        || ReturnUrl.StartsWith("//", StringComparison.Ordinal)
            ? "/"
            : ReturnUrl;

    private static string GetIngredientNameInputId(IngredientRow ingredient) =>
        $"ingredient-{ingredient.Id:N}-name";

    private static string GetIngredientQuantityInputId(IngredientRow ingredient) =>
        $"ingredient-{ingredient.Id:N}-quantity";

    private static string GetIngredientUnitInputId(IngredientRow ingredient) =>
        $"ingredient-{ingredient.Id:N}-unit";

    private static string GetStepInputId(StepRow step) => $"step-{step.Id:N}-text";

    private sealed record UnitOption(string Value, string Label);

    private sealed class RecipeEditorModel
    {
        [Required(ErrorMessage = "Tytuł jest wymagany.")]
        public string Title { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Liczba porcji musi być dodatnia.")]
        public int Servings { get; set; } = 1;

        public string? Source { get; set; }

        public List<IngredientRow> Ingredients { get; set; } = [];
        public List<StepRow> Steps { get; set; } = [];
        public List<string> Tags { get; set; } = [];
    }

    private sealed class IngredientRow
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int SortOrder { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? QuantityText { get; set; }
        public string? Unit { get; set; }
        public ElementReference InputElement { get; set; }

        public static IngredientRow From(
            int sortOrder,
            string name,
            string? quantityText,
            string? unit
        ) =>
            new()
            {
                SortOrder = sortOrder,
                Name = name,
                QuantityText = quantityText,
                Unit = string.IsNullOrWhiteSpace(unit) ? string.Empty : unit,
            };
    }

    private sealed class StepRow
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int SortOrder { get; set; }
        public string Text { get; set; } = string.Empty;
        public InputTextArea? Input { get; set; }

        public static StepRow From(int sortOrder, string text) =>
            new() { SortOrder = sortOrder, Text = text };
    }
}

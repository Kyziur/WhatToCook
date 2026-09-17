using System.Globalization;
using Microsoft.AspNetCore.Components;
using WhatToCook.Web.Components.Features.Planning.Components;
using WhatToCook.Web.Components.Features.Planning.Services;
using WhatToCook.Web.Components.Features.Recipes.Shared;
using WhatToCook.Web.Components.Features.Shopping.Services;
using WhatToCook.Web.Components.Shared;

namespace WhatToCook.Web.Components.Features.Planning.Pages;

public partial class MealPlannerPage : IDisposable
{
    private static readonly CultureInfo PolishCulture = CultureInfo.GetCultureInfo("pl-PL");
    private static readonly StringComparer PolishIgnoreCaseComparer = StringComparer.Create(
        PolishCulture,
        ignoreCase: true
    );

    private readonly Dictionary<string, string> selectedRecipePerDay = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> dayRecipeSearchTerms = new(StringComparer.Ordinal);

    [Inject]
    private IMealPlanService MealPlanService { get; set; } = null!;

    [Inject]
    private IRecipeCatalogService RecipeCatalogService { get; set; } = null!;

    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = null!;

    private IShoppingListService? ShoppingListService =>
        ServiceProvider.GetService(typeof(IShoppingListService)) as IShoppingListService;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private List<MealPlanSummary> Plans { get; set; } = [];
    private List<RecipeCatalogItem> ActiveRecipes { get; set; } = [];
    private Guid? SelectedPlanId { get; set; }
    private EditablePlan? CurrentPlan { get; set; }
    private DateOnly SelectedDay { get; set; }
    private MealSlot SelectedSlot { get; set; } = MealSlot.Dinner;
    private bool IsRecipePickerOpen { get; set; }
    private ShoppingListDetails? EmbeddedShoppingList { get; set; }
    private DateOnly NewPlanDateFrom { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    private DateOnly NewPlanDateTo { get; set; } = DateOnly.FromDateTime(DateTime.Today).AddDays(6);
    private string NewPlanName { get; set; } = string.Empty;
    private bool IsCreatePlanExpanded { get; set; }
    private bool IsPlanSettingsOpen { get; set; }
    private bool IsSelectingPlan { get; set; }
    private bool IsInteractive { get; set; }
    private bool IsDirty { get; set; }
    private bool IsRegenerationConfirmationOpen { get; set; }
    private PlannerLoadState LoadState { get; set; } = PlannerLoadState.Loading;
    private PlannerOperation CurrentOperation { get; set; }
    private bool IsBusy => CurrentOperation != PlannerOperation.None || IsSelectingPlan;
    private string ToastMessage { get; set; } = string.Empty;
    private string LoadErrorMessage { get; set; } = string.Empty;
    private bool ToastIsError { get; set; }
    private CancellationTokenSource? toastCts;
    private Guid? ActiveMealActionEntryId { get; set; }
    private readonly CancellationTokenSource lifetimeCts = new();

    public void Dispose()
    {
        toastCts?.Cancel();
        toastCts?.Dispose();
        lifetimeCts.Cancel();
        lifetimeCts.Dispose();
    }

    protected override Task OnInitializedAsync() => LoadAsync();

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        IsInteractive = true;
        StateHasChanged();
    }

    private async Task LoadAsync()
    {
        LoadState = PlannerLoadState.Loading;
        LoadErrorMessage = string.Empty;
        try
        {
            await RefreshActiveRecipesAsync();
            await ReloadPlansAsync();
            LoadState = PlannerLoadState.Loaded;
        }
        catch (OperationCanceledException) when (lifetimeCts.IsCancellationRequested) { }
        catch
        {
            Plans = [];
            CurrentPlan = null;
            LoadErrorMessage = "Sprawdź połączenie i spróbuj ponownie.";
            LoadState = PlannerLoadState.Error;
        }
    }

    private async Task ReloadPlansAsync()
    {
        Plans = (await MealPlanService.GetPlansAsync(lifetimeCts.Token)).ToList();

        if (Plans.Count == 0)
        {
            CurrentPlan = null;
            SelectedPlanId = null;
            IsCreatePlanExpanded = true;
            return;
        }

        var selected =
            SelectedPlanId is null ? Plans[0].Id
            : Plans.Any(x => x.Id == SelectedPlanId.Value) ? SelectedPlanId.Value
            : Plans[0].Id;
        await SelectPlanAsync(selected);
    }

    private async Task SelectPlanAsync(Guid planId)
    {
        if (IsSelectingPlan)
        {
            return;
        }

        if (SelectedPlanId == planId && CurrentPlan is not null)
        {
            return;
        }

        IsSelectingPlan = true;
        SelectedPlanId = planId;
        CurrentPlan = null;
        try
        {
            var plan = await MealPlanService.GetPlanAsync(planId, lifetimeCts.Token);
            if (plan is null)
            {
                ShowToast("Nie udało się załadować wybranego planu.", isError: true);
                return;
            }

            CurrentPlan = EditablePlan.From(plan);
            var today = DateOnly.FromDateTime(DateTime.Today);
            SelectedDay = today >= plan.DateFrom && today <= plan.DateTo ? today : plan.DateFrom;
            selectedRecipePerDay.Clear();
            dayRecipeSearchTerms.Clear();
            ActiveMealActionEntryId = null;
            IsDirty = false;
            IsRegenerationConfirmationOpen = false;
            await LoadEmbeddedShoppingListAsync(planId);
        }
        catch (OperationCanceledException) when (lifetimeCts.IsCancellationRequested) { }
        catch
        {
            ShowToast("Nie udało się załadować wybranego planu.", isError: true);
        }
        finally
        {
            IsSelectingPlan = false;
        }
    }

    private async Task CreatePlanAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (NewPlanDateTo < NewPlanDateFrom)
        {
            ShowToast("Data końcowa planu nie może być przed datą początkową.", isError: true);
            return;
        }

        CurrentOperation = PlannerOperation.Creating;
        try
        {
            var created = await MealPlanService.CreatePlanAsync(
                new MealPlanCreateRequest(
                    string.IsNullOrWhiteSpace(NewPlanName) ? null : NewPlanName.Trim(),
                    NewPlanDateFrom,
                    NewPlanDateTo
                ),
                lifetimeCts.Token
            );

            if (created is null)
            {
                ShowToast("Nie udało się utworzyć planu.", isError: true);
                return;
            }

            Plans = (await MealPlanService.GetPlansAsync(lifetimeCts.Token)).ToList();
            await SelectPlanAsync(created.Id);
            IsCreatePlanExpanded = false;
            NewPlanName = string.Empty;
            ShowToast("Plan utworzony.", isError: false);
        }
        catch (OperationCanceledException) when (lifetimeCts.IsCancellationRequested) { }
        catch
        {
            ShowToast("Nie udało się utworzyć planu.", isError: true);
        }
        finally
        {
            CurrentOperation = PlannerOperation.None;
        }
    }

    private void ToggleCreatePlan() => IsCreatePlanExpanded = !IsCreatePlanExpanded;

    private void TogglePlanSettings() => IsPlanSettingsOpen = !IsPlanSettingsOpen;

    private void OnCreateDateFromChanged(ChangeEventArgs args)
    {
        var parsed = ParseDate(args.Value?.ToString(), NewPlanDateFrom);
        NewPlanDateFrom = parsed;
        if (NewPlanDateTo < NewPlanDateFrom)
        {
            NewPlanDateTo = NewPlanDateFrom;
        }
    }

    private void OnCreateDateToChanged(ChangeEventArgs args)
    {
        NewPlanDateTo = ParseDate(args.Value?.ToString(), NewPlanDateTo);
    }

    private void OnPlanNameInput(ChangeEventArgs args)
    {
        if (CurrentPlan is null)
        {
            return;
        }

        CurrentPlan.Name = args.Value?.ToString() ?? string.Empty;
        MarkDirty();
    }

    private void OnPlanDateFromChanged(ChangeEventArgs args)
    {
        if (CurrentPlan is null)
        {
            return;
        }

        CurrentPlan.DateFrom = ParseDate(args.Value?.ToString(), CurrentPlan.DateFrom);
        if (CurrentPlan.DateTo < CurrentPlan.DateFrom)
        {
            CurrentPlan.DateTo = CurrentPlan.DateFrom;
        }

        DropEntriesOutsideCurrentRange();
        EnsureSelectedDayInRange();
        MarkDirty();
    }

    private void OnPlanDateToChanged(ChangeEventArgs args)
    {
        if (CurrentPlan is null)
        {
            return;
        }

        CurrentPlan.DateTo = ParseDate(args.Value?.ToString(), CurrentPlan.DateTo);
        DropEntriesOutsideCurrentRange();
        EnsureSelectedDayInRange();
        MarkDirty();
    }

    private void EnsureSelectedDayInRange()
    {
        if (CurrentPlan is null)
        {
            return;
        }

        if (SelectedDay < CurrentPlan.DateFrom || SelectedDay > CurrentPlan.DateTo)
        {
            SelectedDay = CurrentPlan.DateFrom;
        }
    }

    private void SelectDay(DateOnly day)
    {
        if (CurrentPlan is null || day < CurrentPlan.DateFrom || day > CurrentPlan.DateTo || IsBusy)
        {
            return;
        }

        SelectedDay = day;
        ActiveMealActionEntryId = null;
    }

    private async Task OnPlanSelectorChangedAsync(ChangeEventArgs args)
    {
        if (Guid.TryParse(args.Value?.ToString(), out var planId))
        {
            await SelectPlanAsync(planId);
        }
    }

    private IReadOnlyList<DateOnly> GetVisibleDays()
    {
        if (CurrentPlan is null)
        {
            return [];
        }

        var offset = Math.Max(0, SelectedDay.DayNumber - CurrentPlan.DateFrom.DayNumber);
        var weekStart = CurrentPlan.DateFrom.AddDays((offset / 7) * 7);
        return EnumerateDays(weekStart, Min(weekStart.AddDays(6), CurrentPlan.DateTo)).ToList();
    }

    private bool CanMoveWeek(int direction)
    {
        if (CurrentPlan is null)
        {
            return false;
        }

        var target = SelectedDay.AddDays(direction * 7);
        return target >= CurrentPlan.DateFrom && target <= CurrentPlan.DateTo;
    }

    private void MoveWeek(int direction)
    {
        if (CanMoveWeek(direction))
        {
            SelectedDay = SelectedDay.AddDays(direction * 7);
            ActiveMealActionEntryId = null;
        }
    }

    private void SelectToday()
    {
        if (CurrentPlan is null)
        {
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        SelectedDay =
            today >= CurrentPlan.DateFrom && today <= CurrentPlan.DateTo
                ? today
                : CurrentPlan.DateFrom;
        ActiveMealActionEntryId = null;
    }

    private string FormatVisibleWeekRange()
    {
        var days = GetVisibleDays();
        return days.Count == 0 ? string.Empty : FormatPlanDateRange(days[0], days[^1]);
    }

    private static DateOnly Min(DateOnly left, DateOnly right) => left <= right ? left : right;

    private void OpenRecipePicker(DateOnly day, MealSlot slot)
    {
        SelectDay(day);
        SelectedSlot = slot;
        IsRecipePickerOpen = true;
        selectedRecipePerDay[day.ToString("yyyy-MM-dd")] = string.Empty;
        dayRecipeSearchTerms[day.ToString("yyyy-MM-dd")] = string.Empty;
    }

    private void CloseRecipePicker()
    {
        IsRecipePickerOpen = false;
    }

    private void DropEntriesOutsideCurrentRange()
    {
        if (CurrentPlan is null)
        {
            return;
        }

        CurrentPlan.Entries.RemoveAll(x =>
            x.PlannedDate < CurrentPlan.DateFrom || x.PlannedDate > CurrentPlan.DateTo
        );

        foreach (var day in EnumerateDays(CurrentPlan.DateFrom, CurrentPlan.DateTo))
        {
            NormalizeSortOrderForDay(day);
        }
    }

    private IEnumerable<RecipeCatalogItem> GetSelectableRecipesForDay(DateOnly day)
    {
        if (CurrentPlan is null)
        {
            return [];
        }

        var takenRecipeIds = CurrentPlan
            .Entries.Where(x => x.PlannedDate == day)
            .Select(x => x.RecipeId)
            .ToHashSet();

        var searchTerm = SearchTextNormalizer.Normalize(GetDayRecipeSearchTerm(day));
        var candidates = ActiveRecipes.Where(x => !takenRecipeIds.Contains(x.Id));

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            candidates = candidates.Where(x =>
                SearchTextNormalizer
                    .Normalize(x.Title)
                    .Contains(searchTerm, StringComparison.Ordinal)
            );
        }

        return candidates.OrderBy(x => x.Title, PolishIgnoreCaseComparer);
    }

    private List<EditableEntry> GetEntriesForDay(DateOnly day) =>
        CurrentPlan?.Entries.Where(x => x.PlannedDate == day).OrderBy(x => x.SortOrder).ToList()
        ?? [];

    private List<EditableEntry> GetEntriesForSlot(DateOnly day, MealSlot slot) =>
        GetEntriesForDay(day).Where(x => MealSlotOrdering.GetSlot(x.SortOrder) == slot).ToList();

    private string GetDaySelection(DateOnly day) =>
        selectedRecipePerDay.TryGetValue(day.ToString("yyyy-MM-dd"), out var value)
            ? value
            : string.Empty;

    private void SetDaySelection(DateOnly day, string? value)
    {
        selectedRecipePerDay[day.ToString("yyyy-MM-dd")] = value ?? string.Empty;
    }

    private bool CanAddSelectedRecipeForDay(DateOnly day) =>
        Guid.TryParse(GetDaySelection(day), out _);

    private string GetDayRecipeSearchTerm(DateOnly day) =>
        dayRecipeSearchTerms.TryGetValue(day.ToString("yyyy-MM-dd"), out var value)
            ? value
            : string.Empty;

    private void SetDayRecipeSearchTerm(DateOnly day, string? value)
    {
        dayRecipeSearchTerms[day.ToString("yyyy-MM-dd")] = value ?? string.Empty;
    }

    private void AddSelectedRecipeForDay(DateOnly day)
    {
        if (CurrentPlan is null)
        {
            return;
        }

        var selectedValue = GetDaySelection(day);
        if (!Guid.TryParse(selectedValue, out var recipeId))
        {
            return;
        }

        if (CurrentPlan.Entries.Any(x => x.PlannedDate == day && x.RecipeId == recipeId))
        {
            return;
        }

        var recipe = ActiveRecipes.SingleOrDefault(x => x.Id == recipeId);
        if (recipe is null)
        {
            return;
        }

        var nextPosition = GetEntriesForSlot(day, SelectedSlot).Count;

        CurrentPlan.Entries.Add(
            new EditableEntry
            {
                RowId = Guid.NewGuid(),
                EntryId = null,
                RecipeId = recipeId,
                RecipeTitle = recipe.Title,
                RecipeIsActive = true,
                PlannedDate = day,
                Multiplier = 1m,
                SortOrder = MealSlotOrdering.Encode(SelectedSlot, nextPosition),
            }
        );

        selectedRecipePerDay[day.ToString("yyyy-MM-dd")] = string.Empty;
        IsRecipePickerOpen = false;
        ActiveMealActionEntryId = null;
        MarkDirty();
    }

    private void AddRecipeToSelectedSlot(Guid recipeId)
    {
        SetDaySelection(SelectedDay, recipeId.ToString());
        AddSelectedRecipeForDay(SelectedDay);
    }

    private void RemoveEntry(Guid rowId)
    {
        if (CurrentPlan is null)
        {
            return;
        }

        var entry = CurrentPlan.Entries.SingleOrDefault(x => x.RowId == rowId);
        if (entry is null)
        {
            return;
        }

        var day = entry.PlannedDate;
        CurrentPlan.Entries.Remove(entry);
        NormalizeSortOrderForDay(day);
        MarkDirty();
        ActiveMealActionEntryId = null;
    }

    private void MoveEntryUp(DateOnly day, Guid rowId)
    {
        if (CurrentPlan is null)
        {
            return;
        }

        var slot = MealSlotOrdering.GetSlot(
            CurrentPlan.Entries.Single(x => x.RowId == rowId).SortOrder
        );
        var entries = CurrentPlan
            .Entries.Where(x =>
                x.PlannedDate == day && MealSlotOrdering.GetSlot(x.SortOrder) == slot
            )
            .OrderBy(x => x.SortOrder)
            .ToList();
        var index = entries.FindIndex(x => x.RowId == rowId);
        if (index <= 0)
        {
            return;
        }

        (entries[index - 1].SortOrder, entries[index].SortOrder) = (
            entries[index].SortOrder,
            entries[index - 1].SortOrder
        );
        NormalizeSortOrderForDay(day);
        MarkDirty();
    }

    private void MoveEntryDown(DateOnly day, Guid rowId)
    {
        if (CurrentPlan is null)
        {
            return;
        }

        var slot = MealSlotOrdering.GetSlot(
            CurrentPlan.Entries.Single(x => x.RowId == rowId).SortOrder
        );
        var entries = CurrentPlan
            .Entries.Where(x =>
                x.PlannedDate == day && MealSlotOrdering.GetSlot(x.SortOrder) == slot
            )
            .OrderBy(x => x.SortOrder)
            .ToList();
        var index = entries.FindIndex(x => x.RowId == rowId);
        if (index < 0 || index >= entries.Count - 1)
        {
            return;
        }

        (entries[index + 1].SortOrder, entries[index].SortOrder) = (
            entries[index].SortOrder,
            entries[index + 1].SortOrder
        );
        NormalizeSortOrderForDay(day);
        MarkDirty();
    }

    private void NormalizeSortOrderForDay(DateOnly day)
    {
        if (CurrentPlan is null)
        {
            return;
        }

        foreach (var slot in MealSlotOrdering.Slots)
        {
            var ordered = CurrentPlan
                .Entries.Where(x =>
                    x.PlannedDate == day && MealSlotOrdering.GetSlot(x.SortOrder) == slot.Slot
                )
                .OrderBy(x => x.SortOrder)
                .ToList();
            for (var index = 0; index < ordered.Count; index++)
            {
                ordered[index].SortOrder = MealSlotOrdering.Encode(slot.Slot, index);
            }
        }
    }

    private void UpdateMultiplier(Guid rowId, string? rawValue)
    {
        if (CurrentPlan is null)
        {
            return;
        }

        var entry = CurrentPlan.Entries.SingleOrDefault(x => x.RowId == rowId);
        if (entry is null)
        {
            return;
        }

        if (
            !decimal.TryParse(
                rawValue,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var parsed
            )
        )
        {
            return;
        }

        entry.Multiplier = parsed;
        MarkDirty();
    }

    private bool IsMealActionMenuOpen(Guid rowId) => ActiveMealActionEntryId == rowId;

    private void ToggleMealActionMenu(Guid rowId)
    {
        ActiveMealActionEntryId = IsMealActionMenuOpen(rowId) ? null : rowId;
    }

    private void AdjustMultiplier(Guid rowId, decimal delta)
    {
        if (CurrentPlan?.Entries.SingleOrDefault(x => x.RowId == rowId) is not { } entry)
        {
            return;
        }

        entry.Multiplier = Math.Max(0.5m, entry.Multiplier + delta);
        MarkDirty();
        ActiveMealActionEntryId = null;
    }

    private async Task SavePlanAsync()
    {
        if (IsBusy || !IsDirty)
        {
            return;
        }

        if (CurrentPlan?.HasGeneratedShoppingList == true)
        {
            IsRegenerationConfirmationOpen = true;
            return;
        }

        await SavePlanInternalAsync(regenerateShoppingList: true);
    }

    private void CancelRegeneration()
    {
        IsRegenerationConfirmationOpen = false;
    }

    private async Task ConfirmRegenerationAsync()
    {
        if (IsBusy || !IsDirty)
        {
            return;
        }

        IsRegenerationConfirmationOpen = false;
        await SavePlanInternalAsync(regenerateShoppingList: true);
    }

    private async Task SavePlanInternalAsync(bool regenerateShoppingList)
    {
        if (CurrentPlan is null || SelectedPlanId is null || IsBusy)
        {
            return;
        }

        CurrentOperation = PlannerOperation.Saving;
        try
        {
            var saveRequest = new MealPlanSaveRequest(
                CurrentPlan.Name.Trim(),
                CurrentPlan.DateFrom,
                CurrentPlan.DateTo,
                regenerateShoppingList,
                CurrentPlan
                    .Entries.OrderBy(x => x.PlannedDate)
                    .ThenBy(x => x.SortOrder)
                    .Select(x => new MealPlanEntry(
                        x.EntryId,
                        x.RecipeId,
                        x.RecipeTitle,
                        x.RecipeIsActive,
                        x.PlannedDate,
                        x.Multiplier,
                        x.SortOrder
                    ))
                    .ToList()
            );

            var saveResult = await MealPlanService.SavePlanAsync(
                SelectedPlanId.Value,
                saveRequest,
                lifetimeCts.Token
            );
            if (saveResult.RegenerationRequired)
            {
                IsRegenerationConfirmationOpen = true;
                return;
            }

            if (!saveResult.IsSuccess || saveResult.Plan is null)
            {
                ShowToast(
                    saveResult.ErrorMessage ?? "Nie udało się zapisać planu. Spróbuj ponownie.",
                    isError: true
                );
                return;
            }

            IsRegenerationConfirmationOpen = false;

            CurrentPlan = EditablePlan.From(saveResult.Plan);
            Plans = (await MealPlanService.GetPlansAsync(lifetimeCts.Token)).ToList();
            EnsureSelectedDayInRange();
            IsDirty = false;
            if (!saveResult.Plan.HasGeneratedShoppingList)
            {
                var generated = await MealPlanService.MarkShoppingGeneratedAsync(
                    SelectedPlanId.Value,
                    lifetimeCts.Token
                );
                if (generated)
                {
                    CurrentPlan.HasGeneratedShoppingList = true;
                }
            }
            if (regenerateShoppingList && ShoppingListService is not null)
            {
                EmbeddedShoppingList = await ShoppingListService.GetForPlanAsync(
                    SelectedPlanId.Value,
                    lifetimeCts.Token
                );
            }
            else
            {
                await LoadEmbeddedShoppingListAsync(SelectedPlanId.Value);
            }
            ShowToast("Plan zapisany, a lista zakupów została uaktualniona.", isError: false);
        }
        catch (OperationCanceledException) when (lifetimeCts.IsCancellationRequested) { }
        catch
        {
            ShowToast("Nie udało się zapisać planu. Spróbuj ponownie.", isError: true);
        }
        finally
        {
            CurrentOperation = PlannerOperation.None;
        }
    }

    private async Task MarkShoppingGeneratedAsync()
    {
        if (SelectedPlanId is null || IsBusy)
        {
            return;
        }

        CurrentOperation = PlannerOperation.GeneratingShopping;
        try
        {
            var marked = await MealPlanService.MarkShoppingGeneratedAsync(
                SelectedPlanId.Value,
                lifetimeCts.Token
            );
            if (!marked)
            {
                ShowToast("Nie udało się wygenerować listy zakupów dla tego planu.", isError: true);
                return;
            }

            if (CurrentPlan is not null)
            {
                CurrentPlan.HasGeneratedShoppingList = true;
            }

            ShowToast("Lista zakupów została wygenerowana.", isError: false);
            await LoadEmbeddedShoppingListAsync(SelectedPlanId.Value);
        }
        catch (OperationCanceledException) when (lifetimeCts.IsCancellationRequested) { }
        catch
        {
            ShowToast("Nie udało się wygenerować listy zakupów dla tego planu.", isError: true);
        }
        finally
        {
            CurrentOperation = PlannerOperation.None;
        }
    }

    private void MarkDirty()
    {
        IsDirty = true;
    }

    private async Task LoadEmbeddedShoppingListAsync(Guid planId)
    {
        if (ShoppingListService is null)
        {
            EmbeddedShoppingList = null;
            return;
        }

        try
        {
            EmbeddedShoppingList = await ShoppingListService.GetForPlanAsync(
                planId,
                lifetimeCts.Token
            );
        }
        catch (OperationCanceledException) when (lifetimeCts.IsCancellationRequested) { }
        catch
        {
            EmbeddedShoppingList = null;
        }
    }

    private async Task ToggleEmbeddedShoppingItemAsync(ShoppingListItem item)
    {
        if (SelectedPlanId is null || IsBusy || ShoppingListService is null)
        {
            return;
        }

        var nextState =
            item.State == ShoppingChecklistState.Mam
                ? ShoppingChecklistState.NieMam
                : ShoppingChecklistState.Mam;
        EmbeddedShoppingList = await ShoppingListService.SetItemStateAsync(
            SelectedPlanId.Value,
            item.Id,
            nextState,
            lifetimeCts.Token
        );
    }

    private async Task RefreshActiveRecipesAsync()
    {
        ActiveRecipes = (
            await RecipeCatalogService.GetActiveRecipesAsync(lifetimeCts.Token)
        ).ToList();
    }

    private RecipeCatalogItem? GetRecipeForEntry(EditableEntry entry) =>
        ActiveRecipes.SingleOrDefault(x => x.Id == entry.RecipeId);

    private int GetPlannedDayCount() =>
        CurrentPlan?.Entries.Select(x => x.PlannedDate).Distinct().Count() ?? 0;

    private int GetPlanDayCount() =>
        CurrentPlan is null ? 0 : CurrentPlan.DateTo.DayNumber - CurrentPlan.DateFrom.DayNumber + 1;

    private int GetShoppingItemCount() => EmbeddedShoppingList?.Items.Count ?? 0;

    private static IEnumerable<DateOnly> EnumerateDays(DateOnly from, DateOnly to)
    {
        for (var day = from; day <= to; day = day.AddDays(1))
        {
            yield return day;
        }
    }

    private static string ToDayLabel(DateOnly day)
    {
        var dayName = day.DayOfWeek switch
        {
            DayOfWeek.Monday => "Poniedziałek",
            DayOfWeek.Tuesday => "Wtorek",
            DayOfWeek.Wednesday => "Środa",
            DayOfWeek.Thursday => "Czwartek",
            DayOfWeek.Friday => "Piątek",
            DayOfWeek.Saturday => "Sobota",
            DayOfWeek.Sunday => "Niedziela",
            _ => day.DayOfWeek.ToString(),
        };
        return $"{dayName}, {day.ToString("d MMMM yyyy", PolishCulture)}";
    }

    private static string GetDayShortLabel(DateOnly day) =>
        day.DayOfWeek switch
        {
            DayOfWeek.Monday => "Pon",
            DayOfWeek.Tuesday => "Wt",
            DayOfWeek.Wednesday => "Śr",
            DayOfWeek.Thursday => "Czw",
            DayOfWeek.Friday => "Pt",
            DayOfWeek.Saturday => "Sob",
            DayOfWeek.Sunday => "Niedz",
            _ => day.DayOfWeek.ToString(),
        };

    private string GetDayEntryCountLabel(DateOnly day)
    {
        var count = CurrentPlan?.Entries.Count(x => x.PlannedDate == day) ?? 0;
        return count switch
        {
            0 => "bez przepisów",
            1 => "1 przepis",
            _ => $"{count} przepisy",
        };
    }

    private static DateOnly ParseDate(string? value, DateOnly fallback) =>
        DateOnly.TryParse(value, out var parsed) ? parsed : fallback;

    private static string ToDateInput(DateOnly value) => value.ToString("yyyy-MM-dd");

    private static string FormatMultiplier(decimal value) =>
        value.ToString("0.###", CultureInfo.InvariantCulture);

    private static string GetDayRecipeSelectId(DateOnly day) => $"day-recipe-{day:yyyyMMdd}";

    private static string GetDayRecipeSearchInputId(DateOnly day) =>
        $"day-recipe-search-{day:yyyyMMdd}";

    private static string GetDayAddRecipeButtonId(DateOnly day) => $"day-add-recipe-{day:yyyyMMdd}";

    private static string GetEntryMultiplierInputId(Guid rowId) => $"entry-multiplier-{rowId:N}";

    private string GetShoppingLinkForCurrentPlan() =>
        SelectedPlanId.HasValue ? $"/shopping?planId={SelectedPlanId.Value}" : "/shopping";

    private static string GetPlanChipLabel(MealPlanSummary plan) =>
        $"{plan.Name} ({FormatPlanDateRange(plan.DateFrom, plan.DateTo)})";

    private static string FormatPlanDateRange(DateOnly from, DateOnly to)
    {
        if (from.Year == to.Year && from.Month == to.Month)
        {
            return $"{from.Day}-{to.Day} {from.ToString("MMMM yyyy", PolishCulture)}";
        }

        if (from.Year == to.Year)
        {
            return $"{from.ToString("d MMM", PolishCulture)} - {to.ToString("d MMM yyyy", PolishCulture)}";
        }

        return $"{from.ToString("d MMM yyyy", PolishCulture)} - {to.ToString("d MMM yyyy", PolishCulture)}";
    }

    private void ShowToast(string message, bool isError)
    {
        ToastMessage = message;
        ToastIsError = isError;

        toastCts?.Cancel();
        toastCts?.Dispose();
        toastCts = new CancellationTokenSource();
        _ = ClearToastAfterDelayAsync(toastCts.Token);
    }

    private async Task ClearToastAfterDelayAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            ToastMessage = string.Empty;
            await InvokeAsync(StateHasChanged);
        }
        catch (TaskCanceledException) { }
    }

    private enum PlannerLoadState
    {
        Loading,
        Loaded,
        Error,
    }

    private enum PlannerOperation
    {
        None,
        Creating,
        Saving,
        GeneratingShopping,
    }

    private sealed class EditablePlan
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly DateFrom { get; set; }
        public DateOnly DateTo { get; set; }
        public bool HasGeneratedShoppingList { get; set; }
        public List<EditableEntry> Entries { get; set; } = [];

        public static EditablePlan From(MealPlanDetails source) =>
            new()
            {
                Id = source.Id,
                Name = source.Name,
                DateFrom = source.DateFrom,
                DateTo = source.DateTo,
                HasGeneratedShoppingList = source.HasGeneratedShoppingList,
                Entries = source
                    .Entries.Select(x => new EditableEntry
                    {
                        RowId = Guid.NewGuid(),
                        EntryId = x.Id,
                        RecipeId = x.RecipeId,
                        RecipeTitle = x.RecipeTitle,
                        RecipeIsActive = x.RecipeIsActive,
                        PlannedDate = x.PlannedDate,
                        Multiplier = x.Multiplier,
                        SortOrder = x.SortOrder,
                    })
                    .ToList(),
            };
    }

    private sealed class EditableEntry
    {
        public Guid RowId { get; set; }
        public Guid? EntryId { get; set; }
        public Guid RecipeId { get; set; }
        public string RecipeTitle { get; set; } = string.Empty;
        public bool RecipeIsActive { get; set; }
        public DateOnly PlannedDate { get; set; }
        public decimal Multiplier { get; set; }
        public int SortOrder { get; set; }
    }
}

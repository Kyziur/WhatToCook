using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using WhatToCook.Web.Components.Features.Planning.Components;
using WhatToCook.Web.Components.Features.Planning.Pages;
using WhatToCook.Web.Components.Features.Planning.Services;
using WhatToCook.Web.Components.Features.Recipes.Shared;
using WhatToCook.Web.Components.Features.Shopping.Services;

namespace WhatToCook.Web.ComponentTests.Specs;

public sealed class MealPlanningSpecificationTests
{
    [Theory]
    [InlineData(MealSlot.Breakfast, 0, 1000)]
    [InlineData(MealSlot.SecondBreakfast, 0, 2000)]
    [InlineData(MealSlot.Dinner, 0, 3000)]
    [InlineData(MealSlot.Supper, 0, 4000)]
    public void MealSlotOrdering_ShouldEncodeStableRanges(
        MealSlot slot,
        int position,
        int expectedSortOrder
    )
    {
        Assert.Equal(expectedSortOrder, MealSlotOrdering.Encode(slot, position));
        Assert.Equal(slot, MealSlotOrdering.GetSlot(expectedSortOrder));
    }

    [Fact]
    public void MealSlotOrdering_ShouldPreserveLegacyDinnerAndClampPosition()
    {
        Assert.Equal(MealSlot.Dinner, MealSlotOrdering.GetSlot(0));
        Assert.Equal(MealSlot.Dinner, MealSlotOrdering.GetSlot(999));
        Assert.Equal(1000, MealSlotOrdering.Encode(MealSlot.Breakfast, -1));
        Assert.Equal(1999, MealSlotOrdering.Encode(MealSlot.Breakfast, 5000));
    }

    [Fact]
    public void PlannerWithoutPlans_ShouldShowEmptyStateWithCreatePath()
    {
        using var context = CreateContext();
        context.Services.AddSingleton<IMealPlanService>(new FakeMealPlanService([]));
        context.Services.AddSingleton<IRecipeCatalogService>(new FakeRecipeCatalogService([]));

        var cut = context.Render<MealPlannerPage>();

        Assert.Contains("Zaplanuj pierwszy tydzień", cut.Markup);
        Assert.Contains("Utwórz plan", cut.Markup);
    }

    [Fact]
    public void CreatePlanWithDefaultName_ShouldAllowEditingName()
    {
        using var context = CreateContext();
        var mealPlanService = new FakeMealPlanService([]);
        context.Services.AddSingleton<IMealPlanService>(mealPlanService);
        context.Services.AddSingleton<IRecipeCatalogService>(new FakeRecipeCatalogService([]));

        var cut = context.Render<MealPlannerPage>();

        cut.Find("#new-date-from").Change("2026-05-01");
        cut.Find("#new-date-to").Change("2026-05-07");
        cut.FindAll("button")
            .Single(x =>
                x.TextContent.Trim().Equals("Utwórz plan", StringComparison.OrdinalIgnoreCase)
            )
            .Click();

        Assert.Contains("Plan utworzony.", cut.Markup);
        Assert.Contains("Plan tydzień 01.05 - 07.05", cut.Markup);

        cut.FindAll("button").Single(x => x.TextContent.Trim() == "Edytuj plan").Click();
        cut.Find("#plan-name").Input("Plan rodzinny");
        cut.FindAll("button")
            .Single(x =>
                x.TextContent.Trim().Equals("Zapisz plan", StringComparison.OrdinalIgnoreCase)
            )
            .Click();

        Assert.NotNull(mealPlanService.LastSaveRequest);
        Assert.Equal("Plan rodzinny", mealPlanService.LastSaveRequest!.Name);
        Assert.Contains("Plan zapisany, a lista zakupów została uaktualniona.", cut.Markup);
    }

    [Fact]
    public void SaveChangedPlanWithShoppingList_ShouldRequireRegenerationConfirmation()
    {
        using var context = CreateContext();
        var existingPlan = new MealPlanDetails(
            Guid.NewGuid(),
            "Plan testowy",
            new DateOnly(2026, 5, 1),
            new DateOnly(2026, 5, 7),
            true,
            []
        );
        var mealPlanService = new FakeMealPlanService([existingPlan]);

        context.Services.AddSingleton<IMealPlanService>(mealPlanService);
        context.Services.AddSingleton<IRecipeCatalogService>(new FakeRecipeCatalogService([]));

        var cut = context.Render<MealPlannerPage>();
        cut.FindAll("button").Single(x => x.TextContent.Trim() == "Edytuj plan").Click();
        cut.Find("#plan-name").Input("Plan po zmianie");
        cut.FindAll("button")
            .Single(x =>
                x.TextContent.Trim().Equals("Zapisz plan", StringComparison.OrdinalIgnoreCase)
            )
            .Click();

        Assert.Equal(0, mealPlanService.SaveCalls);
        Assert.Contains("Lista zakupów wymaga regeneracji", cut.Markup);
        cut.FindAll("button").Single(x => x.TextContent.Trim() == "Anuluj").Click();
        Assert.Equal(0, mealPlanService.SaveCalls);
        Assert.Contains("Masz niezapisane zmiany", cut.Markup);

        cut.FindAll("button")
            .Single(x =>
                x.TextContent.Trim().Equals("Zapisz plan", StringComparison.OrdinalIgnoreCase)
            )
            .Click();
        cut.FindAll("button").Single(x => x.TextContent.Trim() == "Potwierdź regenerację").Click();

        Assert.NotNull(mealPlanService.LastSaveRequest);
        Assert.True(mealPlanService.LastSaveRequest!.RegenerateShoppingList);
        Assert.Contains("Plan zapisany, a lista zakupów została uaktualniona.", cut.Markup);
    }

    [Fact]
    public void PlannerWithPlans_ShouldSelectPlanAndKeepCreationCollapsed()
    {
        using var context = CreateContext();
        var existingPlan = new MealPlanDetails(
            Guid.NewGuid(),
            "Plan rodzinny",
            new DateOnly(2026, 5, 1),
            new DateOnly(2026, 5, 7),
            false,
            []
        );
        context.Services.AddSingleton<IMealPlanService>(new FakeMealPlanService([existingPlan]));
        context.Services.AddSingleton<IRecipeCatalogService>(new FakeRecipeCatalogService([]));

        var cut = context.Render<MealPlannerPage>();

        Assert.Contains("Plan rodzinny", cut.Markup);
        var planSelector = cut.Find("#plan-selector");
        Assert.NotNull(cut.Find($"label[for='{planSelector.Id}']"));
        Assert.Empty(cut.FindAll("#new-date-from"));
        Assert.Contains("Nowy plan", cut.Markup);
    }

    [Fact]
    public void Planner_ShouldShowCompactDayPickerAndOneActiveDayEditor()
    {
        using var context = CreateContext();
        var existingPlan = new MealPlanDetails(
            Guid.NewGuid(),
            "Plan tygodniowy",
            new DateOnly(2026, 5, 1),
            new DateOnly(2026, 5, 7),
            false,
            []
        );
        context.Services.AddSingleton<IMealPlanService>(new FakeMealPlanService([existingPlan]));
        context.Services.AddSingleton<IRecipeCatalogService>(new FakeRecipeCatalogService([]));

        var cut = context.Render<MealPlannerPage>();

        Assert.Equal(7, cut.FindAll(".planner-grid-day-header").Count);
        Assert.Equal(29, cut.FindAll("section[data-day]").Count);
        Assert.NotNull(cut.Find("section[data-day='2026-05-01']"));
        Assert.NotNull(cut.Find(".planner-dashboard-header"));

        cut.Find("button[data-day-target='2026-05-02']").Click();

        Assert.Equal(29, cut.FindAll("section[data-day]").Count);
        Assert.NotNull(cut.Find("section[data-day='2026-05-02']"));
    }

    [Fact]
    public void PlannerLoading_ShouldNotRenderFalseEmptyState()
    {
        using var context = CreateContext();
        var plansCompletion = new TaskCompletionSource<IReadOnlyList<MealPlanSummary>>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var mealPlans = new FakeMealPlanService([]) { PlansCompletion = plansCompletion };
        context.Services.AddSingleton<IMealPlanService>(mealPlans);
        context.Services.AddSingleton<IRecipeCatalogService>(new FakeRecipeCatalogService([]));

        var cut = context.Render<MealPlannerPage>();

        Assert.Contains("Ładowanie planu posiłków", cut.Markup);
        Assert.DoesNotContain("Zaplanuj pierwszy tydzień", cut.Markup);

        plansCompletion.SetResult([]);
        cut.WaitForAssertion(() => Assert.Contains("Zaplanuj pierwszy tydzień", cut.Markup));
    }

    [Fact]
    public void PlannerLoadFailure_RetryShouldRecover()
    {
        using var context = CreateContext();
        var mealPlans = new FakeMealPlanService([]) { PlanLoadFailuresRemaining = 1 };
        context.Services.AddSingleton<IMealPlanService>(mealPlans);
        context.Services.AddSingleton<IRecipeCatalogService>(new FakeRecipeCatalogService([]));

        var cut = context.Render<MealPlannerPage>();

        cut.WaitForAssertion(() => Assert.Contains("Nie można załadować planera", cut.Markup));
        Assert.DoesNotContain("Zaplanuj pierwszy tydzień", cut.Markup);
        cut.FindAll("button")
            .Single(x => x.TextContent.Contains("Spróbuj ponownie", StringComparison.Ordinal))
            .Click();

        cut.WaitForAssertion(() => Assert.Contains("Zaplanuj pierwszy tydzień", cut.Markup));
    }

    [Fact]
    public async Task PlannerSaveWhileRunning_ShouldDisableActionsAndExecuteOnce()
    {
        using var context = CreateContext();
        var existingPlan = new MealPlanDetails(
            Guid.NewGuid(),
            "Plan rodzinny",
            new DateOnly(2026, 5, 1),
            new DateOnly(2026, 5, 7),
            false,
            []
        );
        var saveCompletion = new TaskCompletionSource<MealPlanSaveResult>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var mealPlans = new FakeMealPlanService([existingPlan]) { SaveCompletion = saveCompletion };
        context.Services.AddSingleton<IMealPlanService>(mealPlans);
        context.Services.AddSingleton<IRecipeCatalogService>(new FakeRecipeCatalogService([]));
        var cut = context.Render<MealPlannerPage>();
        cut.FindAll("button").Single(x => x.TextContent.Trim() == "Edytuj plan").Click();
        cut.Find("#plan-name").Input("Plan po zmianie");

        var firstSave = cut.FindAll("button")
            .Single(x => x.TextContent.Trim().Equals("Zapisz plan", StringComparison.Ordinal))
            .ClickAsync(new MouseEventArgs());
        cut.WaitForAssertion(() =>
        {
            var button = cut.FindAll("button")
                .Single(x =>
                    x.TextContent.Trim().Equals("Zapisywanie...", StringComparison.Ordinal)
                );
            Assert.True(button.HasAttribute("disabled"));
        });
        await cut.FindAll("button")
            .Single(x => x.TextContent.Trim().Equals("Zapisywanie...", StringComparison.Ordinal))
            .ClickAsync(new MouseEventArgs());
        Assert.Equal(1, mealPlans.SaveCalls);

        saveCompletion.SetResult(
            MealPlanSaveResult.Success(existingPlan with { Name = "Plan po zmianie" })
        );
        await firstSave;
        cut.WaitForAssertion(() =>
            Assert.Contains("Plan zapisany, a lista zakupów została uaktualniona.", cut.Markup)
        );
    }

    private sealed class FakeMealPlanService(IReadOnlyList<MealPlanDetails> plans)
        : IMealPlanService
    {
        private readonly Dictionary<Guid, MealPlanDetails> plansById = plans.ToDictionary(x =>
            x.Id
        );

        public MealPlanSaveRequest? LastSaveRequest { get; private set; }
        public int SaveCalls { get; private set; }
        public TaskCompletionSource<IReadOnlyList<MealPlanSummary>>? PlansCompletion { get; set; }
        public int PlanLoadFailuresRemaining { get; set; }
        public TaskCompletionSource<MealPlanSaveResult>? SaveCompletion { get; set; }

        public async Task<IReadOnlyList<MealPlanSummary>> GetPlansAsync(
            CancellationToken cancellationToken = default
        )
        {
            if (PlanLoadFailuresRemaining > 0)
            {
                PlanLoadFailuresRemaining--;
                throw new HttpRequestException("Test planner failure.");
            }

            var result = plansById
                .Values.Select(x => new MealPlanSummary(
                    x.Id,
                    x.Name,
                    x.DateFrom,
                    x.DateTo,
                    x.HasGeneratedShoppingList,
                    x.Entries.Count
                ))
                .OrderByDescending(x => x.DateFrom)
                .ToList();
            return PlansCompletion is null
                ? result
                : await PlansCompletion.Task.WaitAsync(cancellationToken);
        }

        public Task<MealPlanDetails?> GetPlanAsync(
            Guid planId,
            CancellationToken cancellationToken = default
        )
        {
            plansById.TryGetValue(planId, out var value);
            return Task.FromResult(value);
        }

        public Task<MealPlanDetails?> CreatePlanAsync(
            MealPlanCreateRequest request,
            CancellationToken cancellationToken = default
        )
        {
            var created = new MealPlanDetails(
                Guid.NewGuid(),
                string.IsNullOrWhiteSpace(request.Name)
                    ? $"Plan tydzień {request.DateFrom:dd.MM} - {request.DateTo:dd.MM}"
                    : request.Name!,
                request.DateFrom,
                request.DateTo,
                false,
                []
            );
            plansById[created.Id] = created;
            return Task.FromResult<MealPlanDetails?>(created);
        }

        public async Task<MealPlanSaveResult> SavePlanAsync(
            Guid planId,
            MealPlanSaveRequest request,
            CancellationToken cancellationToken = default
        )
        {
            SaveCalls++;
            LastSaveRequest = request;

            if (SaveCompletion is not null)
            {
                var result = await SaveCompletion.Task.WaitAsync(cancellationToken);
                if (result.Plan is not null)
                {
                    plansById[planId] = result.Plan;
                }

                return result;
            }

            var saved = new MealPlanDetails(
                planId,
                request.Name,
                request.DateFrom,
                request.DateTo,
                true,
                request.Entries.ToList()
            );
            plansById[planId] = saved;
            return MealPlanSaveResult.Success(saved);
        }

        public Task<bool> MarkShoppingGeneratedAsync(
            Guid planId,
            CancellationToken cancellationToken = default
        )
        {
            if (!plansById.TryGetValue(planId, out var existing))
            {
                return Task.FromResult(false);
            }

            plansById[planId] = existing with { HasGeneratedShoppingList = true };
            return Task.FromResult(true);
        }
    }

    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddSingleton<IShoppingListService>(new EmptyShoppingListService());
        return context;
    }

    private sealed class EmptyShoppingListService : IShoppingListService
    {
        public Task<ShoppingListDetails?> GetForPlanAsync(
            Guid planId,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<ShoppingListDetails?>(null);

        public Task<IReadOnlyList<RecipeRecommendation>?> GetRecipeRecommendationsAsync(
            Guid planId,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<IReadOnlyList<RecipeRecommendation>?>([]);

        public Task<ShoppingListDetails?> SetItemStateAsync(
            Guid planId,
            Guid itemId,
            ShoppingChecklistState state,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<ShoppingListDetails?>(null);

        public Task<ShoppingListDetails?> AddManualItemAsync(
            Guid planId,
            string text,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<ShoppingListDetails?>(null);

        public Task<string?> GetCopyTextAsync(
            Guid planId,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<string?>(null);
    }

    private sealed class FakeRecipeCatalogService(IReadOnlyList<RecipeCatalogItem> items)
        : IRecipeCatalogService
    {
        public Task<IReadOnlyList<RecipeCatalogItem>> GetActiveRecipesAsync(
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult(
                items.Where(x => x.IsActive).ToList() as IReadOnlyList<RecipeCatalogItem>
            );
        }

        public Task<RecipeCatalogItem?> GetByIdAsync(
            Guid recipeId,
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult(items.SingleOrDefault(x => x.Id == recipeId));
        }

        public Task<bool> ArchiveAsync(Guid recipeId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }
}

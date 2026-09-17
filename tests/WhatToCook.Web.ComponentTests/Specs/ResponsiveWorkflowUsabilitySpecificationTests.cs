using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WhatToCook.Web.Components.Features.Planning.Components;
using WhatToCook.Web.Components.Features.Planning.Services;

namespace WhatToCook.Web.ComponentTests.Specs;

public sealed class ResponsiveWorkflowUsabilitySpecificationTests
{
    [Fact]
    public void AddToPlanPicker_ShouldAddRecipeOnceToSelectedDay()
    {
        using var context = new BunitContext();
        var recipeId = Guid.NewGuid();
        var plan = CreatePlan(hasGeneratedShoppingList: false);
        var mealPlans = new FakeMealPlanService(plan);
        context.Services.AddSingleton<IMealPlanService>(mealPlans);

        var cut = context.Render<AddToPlanPicker>(parameters =>
            parameters.Add(x => x.RecipeId, recipeId).Add(x => x.RecipeTitle, "Zupa dyniowa")
        );

        cut.FindAll("button")
            .Single(x => x.TextContent.Trim().Equals("Dodaj do planu", StringComparison.Ordinal))
            .Click();
        cut.Find("select[id$='-day']").Change("2026-09-01");
        cut.FindAll("button")
            .Single(x =>
                x.TextContent.Trim().Equals("Dodaj do wybranego dnia", StringComparison.Ordinal)
            )
            .Click();

        Assert.Equal(1, mealPlans.SaveCalls);
        var added = Assert.Single(mealPlans.LastSaveRequest!.Entries);
        Assert.Equal(recipeId, added.RecipeId);
        Assert.Equal(plan.DateFrom, added.PlannedDate);
        Assert.Equal(1m, added.Multiplier);
        Assert.Contains("Dodano „Zupa dyniowa”", cut.Markup);
    }

    [Fact]
    public void AddToPlanPicker_ShouldBlockDuplicateRecipeOnTheSameDay()
    {
        using var context = new BunitContext();
        var recipeId = Guid.NewGuid();
        var plan = CreatePlan(
            hasGeneratedShoppingList: false,
            entries:
            [
                new MealPlanEntry(
                    Guid.NewGuid(),
                    recipeId,
                    "Zupa dyniowa",
                    true,
                    new DateOnly(2026, 9, 1),
                    1m,
                    0
                ),
            ]
        );
        var mealPlans = new FakeMealPlanService(plan);
        context.Services.AddSingleton<IMealPlanService>(mealPlans);

        var cut = context.Render<AddToPlanPicker>(parameters =>
            parameters.Add(x => x.RecipeId, recipeId).Add(x => x.RecipeTitle, "Zupa dyniowa")
        );
        cut.FindAll("button")
            .Single(x => x.TextContent.Trim().Equals("Dodaj do planu", StringComparison.Ordinal))
            .Click();
        cut.Find("select[id$='-day']").Change("2026-09-01");
        cut.FindAll("button")
            .Single(x =>
                x.TextContent.Trim().Equals("Dodaj do wybranego dnia", StringComparison.Ordinal)
            )
            .Click();

        Assert.Equal(0, mealPlans.SaveCalls);
        Assert.Contains("jest już zaplanowany na wybrany dzień", cut.Markup);
    }

    [Fact]
    public void AddToPlanPicker_WithGeneratedShoppingList_ShouldRequireRegenerationConfirmation()
    {
        using var context = new BunitContext();
        var plan = CreatePlan(hasGeneratedShoppingList: true);
        var mealPlans = new FakeMealPlanService(plan);
        context.Services.AddSingleton<IMealPlanService>(mealPlans);

        var cut = context.Render<AddToPlanPicker>(parameters =>
            parameters.Add(x => x.RecipeId, Guid.NewGuid()).Add(x => x.RecipeTitle, "Zupa dyniowa")
        );
        cut.FindAll("button")
            .Single(x => x.TextContent.Trim().Equals("Dodaj do planu", StringComparison.Ordinal))
            .Click();
        cut.FindAll("button")
            .Single(x =>
                x.TextContent.Trim().Equals("Dodaj do wybranego dnia", StringComparison.Ordinal)
            )
            .Click();

        Assert.Equal(0, mealPlans.SaveCalls);
        Assert.Contains("Lista zakupów wymaga regeneracji", cut.Markup);
        cut.FindAll("button").Single(x => x.TextContent.Trim() == "Anuluj").Click();
        Assert.Equal(0, mealPlans.SaveCalls);

        cut.FindAll("button")
            .Single(x =>
                x.TextContent.Trim().Equals("Dodaj do wybranego dnia", StringComparison.Ordinal)
            )
            .Click();
        cut.FindAll("button").Single(x => x.TextContent.Trim() == "Potwierdź regenerację").Click();

        Assert.Equal(1, mealPlans.SaveCalls);
        Assert.True(mealPlans.LastSaveRequest!.RegenerateShoppingList);
        Assert.Contains("Dodano „Zupa dyniowa”", cut.Markup);
    }

    [Fact]
    public void AddToPlanPicker_WhenBackendRequiresRegeneration_ShouldRetryWithOriginalPendingEntry()
    {
        using var context = new BunitContext();
        var recipeId = Guid.NewGuid();
        var plan = CreatePlan(hasGeneratedShoppingList: false);
        var mealPlans = new FakeMealPlanService(plan) { RequireRegenerationOnNextSave = true };
        context.Services.AddSingleton<IMealPlanService>(mealPlans);

        var cut = context.Render<AddToPlanPicker>(parameters =>
            parameters.Add(x => x.RecipeId, recipeId).Add(x => x.RecipeTitle, "Zupa dyniowa")
        );
        cut.FindAll("button")
            .Single(x => x.TextContent.Trim().Equals("Dodaj do planu", StringComparison.Ordinal))
            .Click();
        cut.FindAll("button")
            .Single(x =>
                x.TextContent.Trim().Equals("Dodaj do wybranego dnia", StringComparison.Ordinal)
            )
            .Click();

        var firstRequest = mealPlans.LastSaveRequest!;
        var firstEntry = Assert.Single(firstRequest.Entries, x => x.RecipeId == recipeId);
        Assert.False(firstRequest.RegenerateShoppingList);
        Assert.Contains("Lista zakupów wymaga regeneracji", cut.Markup);

        cut.FindAll("button").Single(x => x.TextContent.Trim() == "Potwierdź regenerację").Click();

        var confirmedRequest = mealPlans.LastSaveRequest!;
        var confirmedEntry = Assert.Single(confirmedRequest.Entries, x => x.RecipeId == recipeId);
        Assert.Equal(2, mealPlans.SaveCalls);
        Assert.True(confirmedRequest.RegenerateShoppingList);
        Assert.Equal(firstEntry.PlannedDate, confirmedEntry.PlannedDate);
        Assert.Equal(firstEntry.SortOrder, confirmedEntry.SortOrder);
    }

    [Fact]
    public void AddToPlanPicker_WhenRegenerationIsCancelled_ShouldUseLatestDayAndSlotOnNextRequest()
    {
        using var context = new BunitContext();
        var recipeId = Guid.NewGuid();
        var plan = CreatePlan(hasGeneratedShoppingList: true);
        var mealPlans = new FakeMealPlanService(plan);
        context.Services.AddSingleton<IMealPlanService>(mealPlans);

        var cut = context.Render<AddToPlanPicker>(parameters =>
            parameters.Add(x => x.RecipeId, recipeId).Add(x => x.RecipeTitle, "Zupa dyniowa")
        );
        cut.FindAll("button")
            .Single(x => x.TextContent.Trim().Equals("Dodaj do planu", StringComparison.Ordinal))
            .Click();
        cut.FindAll("button")
            .Single(x =>
                x.TextContent.Trim().Equals("Dodaj do wybranego dnia", StringComparison.Ordinal)
            )
            .Click();
        cut.FindAll("button").Single(x => x.TextContent.Trim() == "Anuluj").Click();

        cut.Find("select[id$='-day']").Change("2026-09-02");
        cut.Find("select[id$='-slot']").Change(nameof(MealSlot.Breakfast));
        cut.FindAll("button")
            .Single(x =>
                x.TextContent.Trim().Equals("Dodaj do wybranego dnia", StringComparison.Ordinal)
            )
            .Click();
        cut.FindAll("button").Single(x => x.TextContent.Trim() == "Potwierdź regenerację").Click();

        var savedEntry = Assert.Single(
            mealPlans.LastSaveRequest!.Entries,
            x => x.RecipeId == recipeId
        );
        Assert.Equal(1, mealPlans.SaveCalls);
        Assert.True(mealPlans.LastSaveRequest.RegenerateShoppingList);
        Assert.Equal(new DateOnly(2026, 9, 2), savedEntry.PlannedDate);
        Assert.Equal(MealSlotOrdering.Encode(MealSlot.Breakfast, 0), savedEntry.SortOrder);
    }

    private static MealPlanDetails CreatePlan(
        bool hasGeneratedShoppingList,
        IReadOnlyList<MealPlanEntry>? entries = null
    ) =>
        new(
            Guid.NewGuid(),
            "Plan rodzinny",
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 7),
            hasGeneratedShoppingList,
            entries ?? []
        );

    private sealed class FakeMealPlanService(MealPlanDetails initialPlan) : IMealPlanService
    {
        private MealPlanDetails plan = initialPlan;

        public int SaveCalls { get; private set; }
        public MealPlanSaveRequest? LastSaveRequest { get; private set; }
        public bool RequireRegenerationOnNextSave { get; set; }

        public Task<IReadOnlyList<MealPlanSummary>> GetPlansAsync(
            CancellationToken cancellationToken = default
        ) =>
            Task.FromResult(
                (IReadOnlyList<MealPlanSummary>)
                    [
                        new(
                            plan.Id,
                            plan.Name,
                            plan.DateFrom,
                            plan.DateTo,
                            plan.HasGeneratedShoppingList,
                            plan.Entries.Count
                        ),
                    ]
            );

        public Task<MealPlanDetails?> GetPlanAsync(
            Guid planId,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<MealPlanDetails?>(planId == plan.Id ? plan : null);

        public Task<MealPlanDetails?> CreatePlanAsync(
            MealPlanCreateRequest request,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<MealPlanDetails?>(null);

        public Task<MealPlanSaveResult> SavePlanAsync(
            Guid planId,
            MealPlanSaveRequest request,
            CancellationToken cancellationToken = default
        )
        {
            SaveCalls++;
            LastSaveRequest = request;
            if (
                (RequireRegenerationOnNextSave || plan.HasGeneratedShoppingList)
                && !request.RegenerateShoppingList
            )
            {
                RequireRegenerationOnNextSave = false;
                return Task.FromResult(MealPlanSaveResult.RegenerationRequiredResult());
            }

            plan = new MealPlanDetails(
                plan.Id,
                request.Name,
                request.DateFrom,
                request.DateTo,
                plan.HasGeneratedShoppingList,
                request.Entries
            );
            return Task.FromResult(MealPlanSaveResult.Success(plan));
        }

        public Task<bool> MarkShoppingGeneratedAsync(
            Guid planId,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(false);
    }
}

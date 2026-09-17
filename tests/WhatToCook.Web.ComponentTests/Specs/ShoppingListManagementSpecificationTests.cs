using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using WhatToCook.Web.Components.Features.Planning.Services;
using WhatToCook.Web.Components.Features.Shopping.Pages;
using WhatToCook.Web.Components.Features.Shopping.Services;

namespace WhatToCook.Web.ComponentTests.Specs;

public sealed class ShoppingListManagementSpecificationTests
{
    [Fact]
    public void WithoutMealPlans_ShouldShowEmptyState()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<IMealPlanService>(new EmptyMealPlanService());
        context.Services.AddSingleton<IShoppingListService>(new FakeShoppingListService());

        var cut = context.Render<ShoppingListPage>();

        Assert.Contains("Brak planów posiłków", cut.Markup);
        Assert.Contains("Najpierw utwórz plan", cut.Markup);
    }

    [Fact]
    public void GeneratedList_ShouldAllowChecklistToggleAndManualItems()
    {
        using var context = new BunitContext();
        var planId = Guid.NewGuid();
        var plan = new MealPlanSummary(
            planId,
            "Plan testowy",
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 7),
            true,
            1
        );
        var mealPlans = new FakeMealPlanService([plan]);
        var shopping = new FakeShoppingListService();
        shopping.Seed(
            planId,
            "Plan testowy",
            [
                new ShoppingListItem(
                    Guid.NewGuid(),
                    "Pomidor",
                    "2",
                    "szt",
                    ShoppingChecklistState.NieMam,
                    false,
                    0,
                    "2 szt Pomidor"
                ),
            ]
        );

        context.Services.AddSingleton<IMealPlanService>(mealPlans);
        context.Services.AddSingleton<IShoppingListService>(shopping);

        var navigationManager = context.Services.GetRequiredService<NavigationManager>();
        navigationManager.NavigateTo($"/shopping?planId={planId}");

        var cut = context.Render<ShoppingListPage>();

        cut.Find(".shopping-item").Click();
        Assert.Equal("true", cut.Find(".shopping-item").GetAttribute("aria-pressed"));

        cut.Find(".shopping-item").Click();
        Assert.Equal("false", cut.Find(".shopping-item").GetAttribute("aria-pressed"));

        cut.Find("#manual-item-input").Input("Papier do pieczenia");
        cut.FindAll("button")
            .Single(x => x.TextContent.Trim().Equals("Dodaj", StringComparison.OrdinalIgnoreCase))
            .Click();

        Assert.Contains("Papier do pieczenia", cut.Markup);
        Assert.Contains("Dodano ręczną pozycję.", cut.Markup);
    }

    [Fact]
    public void CopyAction_ShouldShowCopyPreviewText()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var planId = Guid.NewGuid();
        var plan = new MealPlanSummary(
            planId,
            "Plan kopiowania",
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 7),
            true,
            1
        );
        context.Services.AddSingleton<IMealPlanService>(new FakeMealPlanService([plan]));
        context.Services.AddSingleton<IShoppingListService>(
            new FakeShoppingListService(
                new ShoppingListDetails(
                    Guid.NewGuid(),
                    planId,
                    "Plan kopiowania",
                    DateTimeOffset.UtcNow,
                    [
                        new ShoppingListItem(
                            Guid.NewGuid(),
                            "Pomidor",
                            "2",
                            "szt",
                            ShoppingChecklistState.NieMam,
                            false,
                            0,
                            "2 szt Pomidor"
                        ),
                    ]
                )
            )
        );

        var navigationManager = context.Services.GetRequiredService<NavigationManager>();
        navigationManager.NavigateTo($"/shopping?planId={planId}");

        var cut = context.Render<ShoppingListPage>();
        cut.FindAll("button")
            .Single(x =>
                x.TextContent.Trim().Equals("Kopiuj jako tekst", StringComparison.OrdinalIgnoreCase)
            )
            .Click();

        Assert.Contains("Podgląd kopiowania", cut.Markup);
        Assert.Contains("Lista zakupów - Plan kopiowania", cut.Markup);
    }

    [Fact]
    public void FirstGeneration_ShouldRunWithoutReplacementWarning()
    {
        using var context = new BunitContext();
        var planId = Guid.NewGuid();
        var mealPlans = new FakeMealPlanService([
            new MealPlanSummary(
                planId,
                "Pierwsza lista",
                new DateOnly(2026, 9, 1),
                new DateOnly(2026, 9, 7),
                false,
                1
            ),
        ]);
        var shopping = new FakeShoppingListService();
        mealPlans.OnShoppingGenerated = () =>
            shopping.Seed(planId, "Pierwsza lista", [CreateItem("Pomidor", "2 szt Pomidor")]);
        context.Services.AddSingleton<IMealPlanService>(mealPlans);
        context.Services.AddSingleton<IShoppingListService>(shopping);
        context
            .Services.GetRequiredService<NavigationManager>()
            .NavigateTo($"/shopping?planId={planId}");

        var cut = context.Render<ShoppingListPage>();
        cut.FindAll("button")
            .Single(x => x.TextContent.Contains("Generuj listę", StringComparison.Ordinal))
            .Click();

        Assert.Equal(1, mealPlans.ShoppingGenerationCalls);
        Assert.DoesNotContain("Regeneracja usunie", cut.Markup);
        Assert.Contains("2 szt Pomidor", cut.Markup);
    }

    [Fact]
    public void ExistingListRegeneration_ShouldRunImmediatelyWithoutConfirmation()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var planId = Guid.NewGuid();
        var mealPlans = new FakeMealPlanService([
            new MealPlanSummary(
                planId,
                "Lista z postępem",
                new DateOnly(2026, 9, 1),
                new DateOnly(2026, 9, 7),
                true,
                1
            ),
        ]);
        var shopping = new FakeShoppingListService();
        shopping.Seed(
            planId,
            "Lista z postępem",
            [
                CreateItem("Pomidor", "2 szt Pomidor", ShoppingChecklistState.Mam),
                CreateItem("Papier do pieczenia", "Papier do pieczenia", isManual: true),
            ]
        );
        context.Services.AddSingleton<IMealPlanService>(mealPlans);
        context.Services.AddSingleton<IShoppingListService>(shopping);
        context
            .Services.GetRequiredService<NavigationManager>()
            .NavigateTo($"/shopping?planId={planId}");

        var cut = context.Render<ShoppingListPage>();
        mealPlans.OnShoppingGenerated = () =>
            shopping.Seed(
                planId,
                "Lista z postępem",
                [
                    CreateItem("Pomidor", "2 szt Pomidor", ShoppingChecklistState.Mam),
                    CreateItem("Papier do pieczenia", "Papier do pieczenia", isManual: true),
                ]
            );

        cut.FindAll("button")
            .Single(x => x.TextContent.Contains("Regeneruj listę", StringComparison.Ordinal))
            .Click();

        Assert.DoesNotContain("Regeneracja usunie", cut.Markup);
        Assert.Contains("Papier do pieczenia", cut.Markup);
        Assert.Contains("Mam", cut.Markup);
        Assert.Equal(1, mealPlans.ShoppingGenerationCalls);
        Assert.Contains("Lista zakupów została zregenerowana.", cut.Markup);
    }

    [Fact]
    public async Task GenerationWhileRunning_ShouldDisableActionsAndExecuteOnce()
    {
        using var context = new BunitContext();
        var planId = Guid.NewGuid();
        var generationCompletion = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var mealPlans = new FakeMealPlanService([
            new MealPlanSummary(
                planId,
                "Powolna lista",
                new DateOnly(2026, 9, 1),
                new DateOnly(2026, 9, 7),
                false,
                1
            ),
        ])
        {
            ShoppingGenerationCompletion = generationCompletion,
        };
        var shopping = new FakeShoppingListService();
        mealPlans.OnShoppingGenerated = () =>
            shopping.Seed(planId, "Powolna lista", [CreateItem("Pomidor", "2 szt Pomidor")]);
        context.Services.AddSingleton<IMealPlanService>(mealPlans);
        context.Services.AddSingleton<IShoppingListService>(shopping);
        context
            .Services.GetRequiredService<NavigationManager>()
            .NavigateTo($"/shopping?planId={planId}");
        var cut = context.Render<ShoppingListPage>();
        var generateButton = cut.FindAll("button")
            .Single(x => x.TextContent.Contains("Generuj listę", StringComparison.Ordinal));

        var firstClick = generateButton.ClickAsync(new MouseEventArgs());
        cut.WaitForAssertion(() =>
        {
            var busyButton = cut.FindAll("button")
                .Single(x => x.TextContent.Contains("Generowanie...", StringComparison.Ordinal));
            Assert.True(busyButton.HasAttribute("disabled"));
        });

        await cut.FindAll("button")
            .Single(x => x.TextContent.Contains("Generowanie...", StringComparison.Ordinal))
            .ClickAsync(new MouseEventArgs());
        Assert.Equal(1, mealPlans.ShoppingGenerationCalls);

        generationCompletion.SetResult(true);
        await firstClick;
        cut.WaitForAssertion(() => Assert.Contains("2 szt Pomidor", cut.Markup));
    }

    [Fact]
    public void LoadingPlans_ShouldNotRenderAnEmptyState()
    {
        using var context = new BunitContext();
        var plansCompletion = new TaskCompletionSource<IReadOnlyList<MealPlanSummary>>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var mealPlans = new FakeMealPlanService([]) { PlansCompletion = plansCompletion };
        context.Services.AddSingleton<IMealPlanService>(mealPlans);
        context.Services.AddSingleton<IShoppingListService>(new FakeShoppingListService());

        var cut = context.Render<ShoppingListPage>();

        Assert.Contains("Ładowanie list zakupów", cut.Markup);
        Assert.DoesNotContain("Brak planów posiłków", cut.Markup);

        plansCompletion.SetResult([]);
        cut.WaitForAssertion(() => Assert.Contains("Brak planów posiłków", cut.Markup));
    }

    [Fact]
    public void FailedPlanLoad_RetryShouldRecoverWithoutShowingFalseEmptyState()
    {
        using var context = new BunitContext();
        var mealPlans = new FakeMealPlanService([]) { PlanLoadFailuresRemaining = 1 };
        context.Services.AddSingleton<IMealPlanService>(mealPlans);
        context.Services.AddSingleton<IShoppingListService>(new FakeShoppingListService());

        var cut = context.Render<ShoppingListPage>();

        cut.WaitForAssertion(() => Assert.Contains("Nie można załadować list zakupów", cut.Markup));
        Assert.DoesNotContain("Brak planów posiłków", cut.Markup);

        cut.FindAll("button")
            .Single(x => x.TextContent.Contains("Spróbuj ponownie", StringComparison.Ordinal))
            .Click();

        cut.WaitForAssertion(() => Assert.Contains("Brak planów posiłków", cut.Markup));
    }

    private static ShoppingListItem CreateItem(
        string name,
        string displayText,
        ShoppingChecklistState state = ShoppingChecklistState.NieMam,
        bool isManual = false
    ) => new(Guid.NewGuid(), name, null, null, state, isManual, 0, displayText);

    private sealed class EmptyMealPlanService : IMealPlanService
    {
        public Task<IReadOnlyList<MealPlanSummary>> GetPlansAsync(
            CancellationToken cancellationToken = default
        ) => Task.FromResult((IReadOnlyList<MealPlanSummary>)[]);

        public Task<MealPlanDetails?> GetPlanAsync(
            Guid planId,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<MealPlanDetails?>(null);

        public Task<MealPlanDetails?> CreatePlanAsync(
            MealPlanCreateRequest request,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<MealPlanDetails?>(null);

        public Task<MealPlanSaveResult> SavePlanAsync(
            Guid planId,
            MealPlanSaveRequest request,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(MealPlanSaveResult.Failure("Unused in this test."));

        public Task<bool> MarkShoppingGeneratedAsync(
            Guid planId,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(false);
    }

    private sealed class FakeMealPlanService(IReadOnlyList<MealPlanSummary> plans)
        : IMealPlanService
    {
        public int ShoppingGenerationCalls { get; private set; }
        public Action? OnShoppingGenerated { get; set; }
        public TaskCompletionSource<bool>? ShoppingGenerationCompletion { get; set; }
        public TaskCompletionSource<IReadOnlyList<MealPlanSummary>>? PlansCompletion { get; set; }
        public int PlanLoadFailuresRemaining { get; set; }

        public async Task<IReadOnlyList<MealPlanSummary>> GetPlansAsync(
            CancellationToken cancellationToken = default
        )
        {
            if (PlanLoadFailuresRemaining > 0)
            {
                PlanLoadFailuresRemaining--;
                throw new HttpRequestException("Test load failure.");
            }

            return PlansCompletion is null
                ? plans
                : await PlansCompletion.Task.WaitAsync(cancellationToken);
        }

        public Task<MealPlanDetails?> GetPlanAsync(
            Guid planId,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<MealPlanDetails?>(null);

        public Task<MealPlanDetails?> CreatePlanAsync(
            MealPlanCreateRequest request,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<MealPlanDetails?>(null);

        public Task<MealPlanSaveResult> SavePlanAsync(
            Guid planId,
            MealPlanSaveRequest request,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(MealPlanSaveResult.Failure("Unused in this test."));

        public async Task<bool> MarkShoppingGeneratedAsync(
            Guid planId,
            CancellationToken cancellationToken = default
        )
        {
            ShoppingGenerationCalls++;
            var generated =
                ShoppingGenerationCompletion is null
                || await ShoppingGenerationCompletion.Task.WaitAsync(cancellationToken);
            if (generated)
            {
                OnShoppingGenerated?.Invoke();
            }

            return generated;
        }
    }

    private sealed class FakeShoppingListService : IShoppingListService
    {
        private readonly Dictionary<Guid, ShoppingListDetails> byPlan = [];
        private readonly Dictionary<Guid, string> copyByPlan = [];

        public FakeShoppingListService() { }

        public FakeShoppingListService(ShoppingListDetails initial)
        {
            byPlan[initial.MealPlanId] = initial;
            copyByPlan[initial.MealPlanId] = BuildCopyText(initial);
        }

        public void Seed(Guid planId, string planName, IReadOnlyList<ShoppingListItem> items)
        {
            var list = new ShoppingListDetails(
                Guid.NewGuid(),
                planId,
                planName,
                DateTimeOffset.UtcNow,
                items
            );
            byPlan[planId] = list;
            copyByPlan[planId] = BuildCopyText(list);
        }

        public Task<ShoppingListDetails?> GetForPlanAsync(
            Guid planId,
            CancellationToken cancellationToken = default
        )
        {
            byPlan.TryGetValue(planId, out var list);
            return Task.FromResult(list);
        }

        public Task<IReadOnlyList<RecipeRecommendation>?> GetRecipeRecommendationsAsync(
            Guid planId,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<IReadOnlyList<RecipeRecommendation>?>([]);

        public Task<ShoppingListDetails?> SetItemStateAsync(
            Guid planId,
            Guid itemId,
            ShoppingChecklistState state,
            CancellationToken cancellationToken = default
        )
        {
            if (!byPlan.TryGetValue(planId, out var list))
            {
                return Task.FromResult<ShoppingListDetails?>(null);
            }

            var updatedItems = list
                .Items.Select(x => x.Id == itemId ? x with { State = state } : x)
                .ToList();
            var updated = list with { Items = updatedItems };
            byPlan[planId] = updated;
            copyByPlan[planId] = BuildCopyText(updated);
            return Task.FromResult<ShoppingListDetails?>(updated);
        }

        public Task<ShoppingListDetails?> AddManualItemAsync(
            Guid planId,
            string text,
            CancellationToken cancellationToken = default
        )
        {
            if (!byPlan.TryGetValue(planId, out var list))
            {
                return Task.FromResult<ShoppingListDetails?>(null);
            }

            var nextSort = list.Items.Select(x => x.SortOrder).DefaultIfEmpty(-1).Max() + 1;
            var updatedItems = list.Items.ToList();
            updatedItems.Add(
                new ShoppingListItem(
                    Guid.NewGuid(),
                    text,
                    null,
                    null,
                    ShoppingChecklistState.NieMam,
                    true,
                    nextSort,
                    text
                )
            );
            var updated = list with { Items = updatedItems };
            byPlan[planId] = updated;
            copyByPlan[planId] = BuildCopyText(updated);

            return Task.FromResult<ShoppingListDetails?>(updated);
        }

        public Task<string?> GetCopyTextAsync(
            Guid planId,
            CancellationToken cancellationToken = default
        )
        {
            copyByPlan.TryGetValue(planId, out var text);
            return Task.FromResult(text);
        }

        private static string BuildCopyText(ShoppingListDetails list)
        {
            var lines = new List<string> { $"Lista zakupów - {list.MealPlanName}", string.Empty };
            lines.AddRange(list.Items.OrderBy(x => x.SortOrder).Select(x => x.DisplayText));
            return string.Join(Environment.NewLine, lines);
        }
    }
}

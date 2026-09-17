using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WhatToCook.Web.Components.Features.Planning.Pages;
using WhatToCook.Web.Components.Features.Planning.Services;
using WhatToCook.Web.Components.Features.Recipes.Library.Pages;
using WhatToCook.Web.Components.Features.Recipes.Shared;
using WhatToCook.Web.Components.Layout;

namespace WhatToCook.Web.ComponentTests.Specs;

public sealed class RecipeAppShellSpecificationTests
{
    [Fact]
    public void EnterApplication_ShouldLandInRecipeLibrary()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<IRecipeCatalogService>(new FakeRecipeCatalogService([]));
        context.Services.AddSingleton<IMealPlanService>(new EmptyMealPlanService());

        var cut = context.Render<RecipeLibraryPage>();
        Assert.Contains("Biblioteka przepisów", cut.Markup);
        Assert.Contains("Biblioteka jest pusta", cut.Markup);
        Assert.Contains("Dodaj pierwszy przepis", cut.Markup);
    }

    [Fact]
    public void MainShell_ShouldExposePrimaryNavigationAndCreateAction()
    {
        using var context = new BunitContext();
        var cut = context.Render<MainLayout>();

        Assert.Contains("Dodaj przepis", cut.Markup);
        Assert.Contains("Biblioteka", cut.Markup);
        Assert.Contains("Planer", cut.Markup);
        Assert.Contains("Zakupy", cut.Markup);
    }

    [Fact]
    public void MainShell_ShouldExposeCompactMobileActionsAndLabeledNavigation()
    {
        using var context = new BunitContext();

        var cut = context.Render<MainLayout>();

        var mobileActions = cut.Find(".shell-mobile-actions");
        var mobileActionToggle = mobileActions.QuerySelector(".shell-mobile-actions-toggle");
        Assert.Equal("Dodaj lub importuj przepis", mobileActionToggle?.GetAttribute("aria-label"));
        mobileActionToggle!.Click();
        Assert.Equal(
            "true",
            cut.Find(".shell-mobile-actions-toggle").GetAttribute("aria-expanded")
        );
        Assert.Contains("Dodaj przepis", mobileActions.TextContent);
        Assert.Contains("Importuj URL", mobileActions.TextContent);
        Assert.Equal("Główna nawigacja", cut.Find("nav").GetAttribute("aria-label"));
    }

    [Fact]
    public void PlannerWithoutPlans_ShouldShowEmptyStateWithCreatePath()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<IMealPlanService>(new EmptyMealPlanService());
        context.Services.AddSingleton<IRecipeCatalogService>(new FakeRecipeCatalogService([]));

        var cut = context.Render<MealPlannerPage>();
        Assert.Contains("Zaplanuj pierwszy tydzień", cut.Markup);
        Assert.Contains("Utwórz plan", cut.Markup);
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

    private sealed class EmptyMealPlanService : IMealPlanService
    {
        public Task<IReadOnlyList<MealPlanSummary>> GetPlansAsync(
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult((IReadOnlyList<MealPlanSummary>)[]);
        }

        public Task<MealPlanDetails?> GetPlanAsync(
            Guid planId,
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult<MealPlanDetails?>(null);
        }

        public Task<MealPlanDetails?> CreatePlanAsync(
            MealPlanCreateRequest request,
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult<MealPlanDetails?>(null);
        }

        public Task<MealPlanSaveResult> SavePlanAsync(
            Guid planId,
            MealPlanSaveRequest request,
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult(MealPlanSaveResult.Failure("Not used in this test."));
        }

        public Task<bool> MarkShoppingGeneratedAsync(
            Guid planId,
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult(false);
        }
    }
}

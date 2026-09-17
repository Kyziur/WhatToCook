using AngleSharp.Dom;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WhatToCook.Web.Components.Features.Planning.Services;
using WhatToCook.Web.Components.Features.Recipes.Details.Pages;
using WhatToCook.Web.Components.Features.Recipes.Library.Pages;
using WhatToCook.Web.Components.Features.Recipes.Shared;

namespace WhatToCook.Web.ComponentTests;

public sealed class RecipeLibraryAndDetailsTests
{
    [Fact]
    public void Library_ShouldRenderAlphabeticallyAndFilterByTitle()
    {
        using var context = CreateContext();
        context.Services.AddSingleton<IRecipeCatalogService>(
            new FakeRecipeCatalogService([
                new RecipeCatalogItem(
                    Guid.NewGuid(),
                    "Zupa pomidorowa",
                    4,
                    null,
                    null,
                    true,
                    ["obiad"],
                    [new RecipeCatalogIngredient("pomidor", null, null)],
                    ["Krok 1"]
                ),
                new RecipeCatalogItem(
                    Guid.NewGuid(),
                    "Grecka sałatka",
                    2,
                    null,
                    null,
                    true,
                    ["szybkie"],
                    [new RecipeCatalogIngredient("feta", null, null)],
                    ["Krok 1"]
                ),
            ])
        );

        var cut = context.Render<RecipeLibraryPage>();
        var links = cut.FindAll(".recipe-title-link");
        Assert.Equal("Grecka sałatka", links[0].TextContent.Trim());
        Assert.Equal("Zupa pomidorowa", links[1].TextContent.Trim());

        cut.Find("#recipe-sort").Change("TitleDescending");
        links = cut.FindAll(".recipe-title-link");
        Assert.Equal("Zupa pomidorowa", links[0].TextContent.Trim());

        cut.Find("#library-search").Input("grecka");
        Assert.Single(cut.FindAll(".recipe-title-link"));
        Assert.Contains("Grecka sałatka", cut.Markup);
    }

    [Fact]
    public void Library_ShouldFilterByTagsAndIngredients()
    {
        using var context = CreateContext();
        context.Services.AddSingleton<IRecipeCatalogService>(
            new FakeRecipeCatalogService([
                new RecipeCatalogItem(
                    Guid.NewGuid(),
                    "Zupa pomidorowa",
                    4,
                    null,
                    null,
                    true,
                    ["obiad", "klasyk"],
                    [
                        new RecipeCatalogIngredient("pomidor", null, null),
                        new RecipeCatalogIngredient("makaron", null, null),
                    ],
                    ["Krok 1"]
                ),
                new RecipeCatalogItem(
                    Guid.NewGuid(),
                    "Grecka sałatka",
                    2,
                    null,
                    null,
                    true,
                    ["szybkie"],
                    [
                        new RecipeCatalogIngredient("pomidor", null, null),
                        new RecipeCatalogIngredient("feta", null, null),
                    ],
                    ["Krok 1"]
                ),
            ])
        );

        var cut = context.Render<RecipeLibraryPage>();

        FindFilterButton(cut, "szybkie").Click();

        var filteredByTag = cut.FindAll(".recipe-title-link");
        Assert.Single(filteredByTag);
        Assert.Equal("Grecka sałatka", filteredByTag[0].TextContent.Trim());

        ToggleIngredientFilter(cut, "pomidor");
        ToggleIngredientFilter(cut, "feta");

        var filteredByIngredients = cut.FindAll(".recipe-title-link");
        Assert.Single(filteredByIngredients);
        Assert.Equal("Grecka sałatka", filteredByIngredients[0].TextContent.Trim());
    }

    [Fact]
    public void Library_ShouldShowEmptyState_WhenNoRecipes()
    {
        using var context = CreateContext();
        context.Services.AddSingleton<IRecipeCatalogService>(new FakeRecipeCatalogService([]));

        var cut = context.Render<RecipeLibraryPage>();
        Assert.Contains("Biblioteka jest pusta", cut.Markup);
    }

    [Fact]
    public void Library_ShouldScaleTagAndIngredientFilters_ForLargeCatalogs()
    {
        using var context = CreateContext();

        var recipes = Enumerable
            .Range(1, 50)
            .Select(index => new RecipeCatalogItem(
                Guid.NewGuid(),
                $"Przepis {index:00}",
                2,
                null,
                null,
                true,
                [$"tag{((index - 1) % 20) + 1:00}"],
                [new RecipeCatalogIngredient($"ing{index:00}", null, null)],
                ["Krok 1"]
            ))
            .ToList();

        context.Services.AddSingleton<IRecipeCatalogService>(new FakeRecipeCatalogService(recipes));

        var cut = context.Render<RecipeLibraryPage>();

        Assert.Contains("Pokaż wszystkie tagi (20)", cut.Markup);
        Assert.Contains("Pokaż wszystkie składniki (50)", cut.Markup);
        Assert.DoesNotContain("tag20", cut.FindAll(".filter-chip-panel")[0].TextContent);
        Assert.DoesNotContain("ing50", cut.FindAll(".filter-chip-panel")[1].TextContent);

        cut.Find("#tag-filter-search").Input("tag20");
        Assert.Contains("tag20", cut.FindAll(".filter-chip-panel")[0].TextContent);

        cut.Find("#ingredient-filter-search").Input("ing50");
        Assert.Contains("ing50", cut.FindAll(".filter-chip-panel")[1].TextContent);
    }

    [Fact]
    public void Library_ShouldLeadWithTitleSearchAndSummarizeResponsiveFilters()
    {
        using var context = CreateContext();
        context.Services.AddSingleton<IRecipeCatalogService>(
            new FakeRecipeCatalogService([
                CreateRecipe("Grecka sałatka", ["szybkie"], ["feta"]),
                CreateRecipe("Zupa pomidorowa", ["obiad"], ["pomidor"]),
            ])
        );

        var cut = context.Render<RecipeLibraryPage>();

        Assert.Equal("library-search", cut.FindAll("input").First().Id);
        var filterToggle = cut.Find("#library-filter-toggle");
        Assert.Equal("false", filterToggle.GetAttribute("aria-expanded"));
        Assert.Contains("2 przepisy", cut.Find(".library-results-summary").TextContent);

        filterToggle.Click();
        Assert.Equal("true", cut.Find("#library-filter-toggle").GetAttribute("aria-expanded"));
        FindFilterButton(cut, "szybkie").Click();

        Assert.Contains("szybkie", cut.Find(".selected-filter-summary").TextContent);
        Assert.Contains("1 przepis", cut.Find(".library-results-summary").TextContent);
    }

    [Fact]
    public void Library_ShouldNormalizeLegacyQuantityUnitAndBracketForFiltering()
    {
        using var context = CreateContext();
        context.Services.AddSingleton<IRecipeCatalogService>(
            new FakeRecipeCatalogService([
                new RecipeCatalogItem(
                    Guid.NewGuid(),
                    "Owsianka",
                    2,
                    null,
                    null,
                    true,
                    [],
                    [new RecipeCatalogIngredient("120 g) napoju owsianego", null, null)],
                    ["Wymieszaj składniki."]
                ),
            ])
        );

        var cut = context.Render<RecipeLibraryPage>();
        cut.Find("#library-filter-toggle").Click();

        Assert.Contains("napoju owsianego", cut.Markup);
        Assert.DoesNotContain(">120 g) napoju owsianego<", cut.Markup);
        ToggleIngredientFilter(cut, "napoju owsianego");

        Assert.Contains("Owsianka", cut.Markup);
        Assert.Contains("1 przepis", cut.Find(".library-results-summary").TextContent);
    }

    [Fact]
    public void Library_ShouldOfferAllAndAnyIngredientMatchingModes()
    {
        using var context = CreateContext();
        context.Services.AddSingleton<IRecipeCatalogService>(
            new FakeRecipeCatalogService([
                CreateRecipe("Zupa pomidorowa", ["obiad"], ["pomidor"]),
                CreateRecipe("Sałatka feta", ["kolacja"], ["feta"]),
            ])
        );

        var cut = context.Render<RecipeLibraryPage>();
        cut.Find("#library-filter-toggle").Click();
        ToggleIngredientFilter(cut, "pomidor");
        ToggleIngredientFilter(cut, "feta");

        Assert.Contains("Brak pasujących przepisów", cut.Markup);
        cut.FindAll(".ingredient-match-mode button")
            .Single(x => x.TextContent.Trim().Equals("Dowolny", StringComparison.Ordinal))
            .Click();

        Assert.Contains("Zupa pomidorowa", cut.Markup);
        Assert.Contains("Sałatka feta", cut.Markup);
    }

    [Fact]
    public void LibraryLoading_ShouldNotRenderFalseEmptyState()
    {
        using var context = CreateContext();
        var completion = new TaskCompletionSource<IReadOnlyList<RecipeCatalogItem>>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        context.Services.AddSingleton<IRecipeCatalogService>(
            new FakeRecipeCatalogService([]) { ActiveRecipesCompletion = completion }
        );

        var cut = context.Render<RecipeLibraryPage>();

        Assert.Contains("Ładowanie biblioteki", cut.Markup);
        Assert.DoesNotContain("Biblioteka jest pusta", cut.Markup);

        completion.SetResult([]);
        cut.WaitForAssertion(() => Assert.Contains("Biblioteka jest pusta", cut.Markup));
    }

    [Fact]
    public void LibraryLoadFailure_RetryShouldRecoverWithoutFalseEmptyState()
    {
        using var context = CreateContext();
        var catalog = new FakeRecipeCatalogService([]) { ActiveRecipeLoadFailuresRemaining = 1 };
        context.Services.AddSingleton<IRecipeCatalogService>(catalog);

        var cut = context.Render<RecipeLibraryPage>();

        cut.WaitForAssertion(() => Assert.Contains("Nie można załadować biblioteki", cut.Markup));
        Assert.DoesNotContain("Biblioteka jest pusta", cut.Markup);
        cut.FindAll("button")
            .Single(x => x.TextContent.Contains("Spróbuj ponownie", StringComparison.Ordinal))
            .Click();

        cut.WaitForAssertion(() => Assert.Contains("Biblioteka jest pusta", cut.Markup));
    }

    [Fact]
    public void Details_ShouldShowIngredientQuantitiesAndUnits()
    {
        using var context = CreateContext();
        var recipeId = Guid.NewGuid();
        var recipe = new RecipeCatalogItem(
            recipeId,
            "Grecka sałatka",
            4,
            "https://example.com/grecka",
            null,
            true,
            ["szybkie"],
            [
                new RecipeCatalogIngredient("pomidor", "2", "szt"),
                new RecipeCatalogIngredient("feta", "150", "g"),
            ],
            ["Pokroj warzywa"]
        );

        context.Services.AddSingleton<IRecipeCatalogService>(
            new FakeRecipeCatalogService([recipe])
        );

        var cut = context.Render<RecipeDetailsPage>(parameters =>
            parameters.Add(x => x.RecipeId, recipeId)
        );

        Assert.Contains("Brak zdjęcia przepisu", cut.Markup);
        Assert.Contains("https://example.com/grecka", cut.Markup);
        Assert.Contains("2 szt pomidor", cut.Markup);
        Assert.Contains("150 g feta", cut.Markup);
        Assert.Contains("<li>Pokroj warzywa</li>", cut.Markup);
    }

    [Fact]
    public void DetailsLoading_ShouldNotClaimRecipeIsMissing()
    {
        using var context = CreateContext();
        var recipeId = Guid.NewGuid();
        var completion = new TaskCompletionSource<RecipeCatalogItem?>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        context.Services.AddSingleton<IRecipeCatalogService>(
            new FakeRecipeCatalogService([]) { RecipeCompletion = completion }
        );

        var cut = context.Render<RecipeDetailsPage>(parameters =>
            parameters.Add(x => x.RecipeId, recipeId)
        );

        Assert.Contains("Ładowanie przepisu", cut.Markup);
        Assert.DoesNotContain("Przepis nie istnieje", cut.Markup);

        completion.SetResult(null);
        cut.WaitForAssertion(() => Assert.Contains("Przepis nie istnieje", cut.Markup));
    }

    [Fact]
    public void DetailsLoadFailure_RetryShouldRecoverToMissingState()
    {
        using var context = CreateContext();
        var catalog = new FakeRecipeCatalogService([]) { RecipeLoadFailuresRemaining = 1 };
        context.Services.AddSingleton<IRecipeCatalogService>(catalog);

        var cut = context.Render<RecipeDetailsPage>(parameters =>
            parameters.Add(x => x.RecipeId, Guid.NewGuid())
        );

        cut.WaitForAssertion(() => Assert.Contains("Nie można załadować przepisu", cut.Markup));
        Assert.DoesNotContain("Przepis nie istnieje", cut.Markup);
        cut.FindAll("button")
            .Single(x => x.TextContent.Contains("Spróbuj ponownie", StringComparison.Ordinal))
            .Click();

        cut.WaitForAssertion(() => Assert.Contains("Przepis nie istnieje", cut.Markup));
    }

    private static RecipeCatalogItem CreateRecipe(
        string title,
        IReadOnlyList<string> tags,
        IReadOnlyList<string> ingredients
    ) =>
        new(
            Guid.NewGuid(),
            title,
            4,
            null,
            null,
            true,
            tags,
            ingredients.Select(x => new RecipeCatalogIngredient(x, null, null)).ToList(),
            ["Krok 1"]
        );

    private static IElement FindFilterButton(
        IRenderedComponent<RecipeLibraryPage> cut,
        string label
    ) =>
        cut.FindAll(".filter-chip-panel button")
            .Single(x =>
                x.TextContent.Trim().StartsWith(label, StringComparison.OrdinalIgnoreCase)
            );

    private static void ToggleIngredientFilter(
        IRenderedComponent<RecipeLibraryPage> cut,
        string label
    ) =>
        cut.FindAll(".ingredient-filter-option")
            .Single(x => x.TextContent.Trim().StartsWith(label, StringComparison.OrdinalIgnoreCase))
            .QuerySelector("input")!
            .Change(true);

    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddSingleton<IMealPlanService>(new EmptyMealPlanService());
        return context;
    }

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
        ) => Task.FromResult(MealPlanSaveResult.Failure("Not used in this test."));

        public Task<bool> MarkShoppingGeneratedAsync(
            Guid planId,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(false);
    }

    private sealed class FakeRecipeCatalogService(IReadOnlyList<RecipeCatalogItem> items)
        : IRecipeCatalogService
    {
        public TaskCompletionSource<
            IReadOnlyList<RecipeCatalogItem>
        >? ActiveRecipesCompletion { get; set; }
        public int ActiveRecipeLoadFailuresRemaining { get; set; }
        public TaskCompletionSource<RecipeCatalogItem?>? RecipeCompletion { get; set; }
        public int RecipeLoadFailuresRemaining { get; set; }

        public async Task<IReadOnlyList<RecipeCatalogItem>> GetActiveRecipesAsync(
            CancellationToken cancellationToken = default
        )
        {
            if (ActiveRecipeLoadFailuresRemaining > 0)
            {
                ActiveRecipeLoadFailuresRemaining--;
                throw new HttpRequestException("Test catalog failure.");
            }

            if (ActiveRecipesCompletion is not null)
            {
                return await ActiveRecipesCompletion.Task.WaitAsync(cancellationToken);
            }

            return items.Where(x => x.IsActive).ToList();
        }

        public async Task<RecipeCatalogItem?> GetByIdAsync(
            Guid recipeId,
            CancellationToken cancellationToken = default
        )
        {
            if (RecipeLoadFailuresRemaining > 0)
            {
                RecipeLoadFailuresRemaining--;
                throw new HttpRequestException("Test recipe failure.");
            }

            return RecipeCompletion is null
                ? items.SingleOrDefault(x => x.Id == recipeId)
                : await RecipeCompletion.Task.WaitAsync(cancellationToken);
        }

        public Task<bool> ArchiveAsync(Guid recipeId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }
}

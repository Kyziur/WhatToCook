using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using WhatToCook.Web.Components.Features.Recipes.Import.Pages;
using WhatToCook.Web.Components.Features.Recipes.Import.Services;
using WhatToCook.Web.Components.Features.Recipes.Shared;

namespace WhatToCook.Web.ComponentTests.Specs;

public sealed class RecipeImportSpecificationTests
{
    [Fact]
    public void ImportPage_ShouldCreateDraftAndNavigateToReview()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<IRecipeImportService>(new FakeRecipeImportService());

        var cut = context.Render<RecipeImportPage>();
        cut.Find("#recipe-import-url").Input("https://example.com/przepis/test");
        Assert.Equal(
            "submit",
            cut.FindAll("button")
                .Single(x =>
                    x.TextContent.Trim()
                        .Equals("Utwórz szkic importu", StringComparison.OrdinalIgnoreCase)
                )
                .GetAttribute("type")
        );
        cut.Find("form").Submit();

        var navigationManager = context.Services.GetRequiredService<NavigationManager>();
        Assert.Contains(
            "/recipes/imports/22222222-2222-2222-2222-222222222222",
            navigationManager.Uri
        );
    }

    [Fact]
    public void ReviewPage_ShouldRenderIssuesAndSaveCorrectedDraft()
    {
        using var context = new BunitContext();
        var importService = new FakeRecipeImportService();
        context.Services.AddSingleton<IRecipeImportService>(importService);

        var cut = context.Render<RecipeImportReviewPage>(parameters =>
            parameters.Add(x => x.DraftId, FakeRecipeImportService.BlockedDraftId)
        );

        Assert.Contains("Liczba porcji musi by", cut.Markup);
        Assert.NotNull(cut.Find("select.ingredient-unit"));
        Assert.Contains("Usuń", cut.Markup);

        cut.Find("#import-servings").Input("4");
        cut.Find("select.ingredient-unit").Change("kg");
        cut.FindAll("button")
            .Single(x =>
                x.TextContent.Trim().Equals("Zapisz szkic", StringComparison.OrdinalIgnoreCase)
            )
            .Click();

        Assert.NotNull(importService.LastUpdateRequest);
        Assert.Equal(4, importService.LastUpdateRequest!.Servings);
        Assert.Equal("kg", importService.LastUpdateRequest.Ingredients[0].Unit);
        Assert.Contains("Szkic importu zostal zapisany.", cut.Markup);
    }

    [Fact]
    public void ReviewPage_ShouldFinalizeAndNavigateToRecipeDetails()
    {
        using var context = new BunitContext();
        var importService = new FakeRecipeImportService();
        context.Services.AddSingleton<IRecipeImportService>(importService);

        var cut = context.Render<RecipeImportReviewPage>(parameters =>
            parameters.Add(x => x.DraftId, FakeRecipeImportService.ReadyDraftId)
        );

        cut.FindAll("button")
            .Single(x =>
                x.TextContent.Trim().Equals("Finalizuj przepis", StringComparison.OrdinalIgnoreCase)
            )
            .Click();

        var navigationManager = context.Services.GetRequiredService<NavigationManager>();
        Assert.Contains("/recipes/33333333-3333-3333-3333-333333333333", navigationManager.Uri);
    }

    [Fact]
    public void ReviewPage_ShouldMergeStepWithPreviousBeforeSaving()
    {
        using var context = new BunitContext();
        var importService = new FakeRecipeImportService();
        context.Services.AddSingleton<IRecipeImportService>(importService);

        var cut = context.Render<RecipeImportReviewPage>(parameters =>
            parameters.Add(x => x.DraftId, FakeRecipeImportService.ReadyDraftId)
        );

        cut.FindAll("button")
            .Single(x =>
                x.TextContent.Trim()
                    .Equals("Złącz z krokiem wyżej", StringComparison.OrdinalIgnoreCase)
            )
            .Click();

        cut.FindAll("button")
            .Single(x =>
                x.TextContent.Trim().Equals("Zapisz szkic", StringComparison.OrdinalIgnoreCase)
            )
            .Click();

        Assert.NotNull(importService.LastUpdateRequest);
        Assert.Single(importService.LastUpdateRequest!.Steps);
        Assert.Contains("Ugotuj makaron.", importService.LastUpdateRequest.Steps[0]);
        Assert.Contains("Wymieszaj z pesto.", importService.LastUpdateRequest.Steps[0]);
    }

    private sealed class FakeRecipeImportService : IRecipeImportService
    {
        public static readonly Guid BlockedDraftId = Guid.Parse(
            "22222222-2222-2222-2222-222222222222"
        );
        public static readonly Guid ReadyDraftId = Guid.Parse(
            "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"
        );

        public RecipeImportDraftUpdateRequest? LastUpdateRequest { get; private set; }

        public Task<RecipeImportCreateResult> CreateFromUrlAsync(
            string url,
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult(
                RecipeImportCreateResult.Success(
                    CreateDraft(
                        BlockedDraftId,
                        canFinalize: false,
                        servings: null,
                        withIssues: true
                    )
                )
            );
        }

        public Task<RecipeImportDraft?> GetDraftAsync(
            Guid draftId,
            CancellationToken cancellationToken = default
        )
        {
            RecipeImportDraft? draft =
                draftId == ReadyDraftId
                    ? CreateDraft(ReadyDraftId, canFinalize: true, servings: 4, withIssues: false)
                    : CreateDraft(
                        BlockedDraftId,
                        canFinalize: false,
                        servings: null,
                        withIssues: true
                    );
            return Task.FromResult<RecipeImportDraft?>(draft);
        }

        public Task<RecipeImportUpdateResult> UpdateDraftAsync(
            Guid draftId,
            RecipeImportDraftUpdateRequest request,
            CancellationToken cancellationToken = default
        )
        {
            LastUpdateRequest = request;
            return Task.FromResult(
                RecipeImportUpdateResult.Success(
                    CreateDraft(
                        draftId,
                        canFinalize: request.Servings > 0,
                        servings: request.Servings,
                        withIssues: request.Servings is null or <= 0
                    )
                )
            );
        }

        public Task<RecipeImportFinalizeResult> FinalizeDraftAsync(
            Guid draftId,
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult(
                RecipeImportFinalizeResult.Success(
                    new RecipeCatalogItem(
                        Guid.Parse("33333333-3333-3333-3333-333333333333"),
                        "Makaron z pesto",
                        4,
                        "https://example.com",
                        null,
                        true,
                        ["obiad"],
                        [new RecipeCatalogIngredient("makaron", "250", "g")],
                        ["Ugotuj makaron."]
                    )
                )
            );
        }

        private static RecipeImportDraft CreateDraft(
            Guid draftId,
            bool canFinalize,
            int? servings,
            bool withIssues
        )
        {
            return new RecipeImportDraft(
                draftId,
                "url",
                canFinalize ? "readytofinalize" : "needsreview",
                "https://example.com/przepis/test",
                "Makaron z pesto",
                servings,
                "https://example.com/przepis/test",
                canFinalize,
                null,
                [new RecipeImportIngredient("makaron", "250", "g")],
                ["Ugotuj makaron.", "Wymieszaj z pesto."],
                ["obiad"],
                withIssues
                    ?
                    [
                        new RecipeImportIssue(
                            "Servings",
                            "validation",
                            "Liczba porcji musi byc dodatnia.",
                            "blocker"
                        ),
                    ]
                    : []
            );
        }
    }
}

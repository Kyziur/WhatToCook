using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using WhatToCook.Web.Components.Features.Recipes.Editor.Pages;
using WhatToCook.Web.Components.Features.Recipes.Editor.Services;

namespace WhatToCook.Web.ComponentTests.Specs;

public sealed class RecipeEditorSpecificationTests
{
    [Fact]
    public void CreateRecipe_ShouldShowValidationForRequiredFields()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddSingleton<IRecipeEditorService>(new FakeRecipeEditorService());

        var cut = context.Render<RecipeEditorPage>();
        cut.Find("#recipe-title").Change(string.Empty);
        SubmitEditor(cut);

        Assert.Contains("Tytuł jest wymagany.", cut.Markup);
        Assert.Equal("true", cut.Find("#recipe-title").GetAttribute("aria-invalid"));
        Assert.Equal(
            "recipe-title-error",
            cut.Find("#recipe-title").GetAttribute("aria-describedby")
        );

        cut.Find("#recipe-title").Change("Nowa pomidorowa");
        cut.Find("input[placeholder='np. pomidor']").Input(string.Empty);
        cut.Find("textarea").Change(string.Empty);
        SubmitEditor(cut);
        Assert.Contains("Dodaj co najmniej jeden składnik.", cut.Markup);
        Assert.Contains("Dodaj co najmniej jeden krok.", cut.Markup);
    }

    [Fact]
    public void CreateRecipe_ShouldSupportAddingAdditionalIngredientAndStep()
    {
        using var context = new BunitContext();
        var editorService = new FakeRecipeEditorService();
        context.Services.AddSingleton<IRecipeEditorService>(editorService);

        var cut = context.Render<RecipeEditorPage>();
        cut.Find("#recipe-title").Change("Nowa pomidorowa");
        cut.Find("input[placeholder='np. pomidor']").Input("Pomidor");
        cut.Find("textarea").Change("Pokroj pomidory");

        cut.FindAll("button")
            .Single(x =>
                x.TextContent.Trim().Equals("Dodaj składnik", StringComparison.OrdinalIgnoreCase)
            )
            .Click();
        cut.FindAll("input[placeholder='np. pomidor']")[1].Input("Cebula");

        cut.FindAll("button")
            .Single(x =>
                x.TextContent.Trim().Equals("Dodaj krok", StringComparison.OrdinalIgnoreCase)
            )
            .Click();
        cut.FindAll("textarea")[1].Change("Podsmaż cebulę");

        SubmitEditor(cut);

        Assert.NotNull(editorService.LastRequest);
        Assert.Equal(2, editorService.LastRequest!.Ingredients.Count);
        Assert.Equal(2, editorService.LastRequest.Steps.Count);
    }

    [Fact]
    public void CreateRecipe_WithValidPayload_ShouldCallApiServiceAndNavigateToDetails()
    {
        using var context = new BunitContext();
        var editorService = new FakeRecipeEditorService();
        context.Services.AddSingleton<IRecipeEditorService>(editorService);

        var cut = context.Render<RecipeEditorPage>();
        cut.Find("#recipe-title").Change("Nowa pomidorowa");
        cut.Find("input[placeholder='np. pomidor']").Input("Pomidor");
        cut.Find("textarea").Change("Pokroj pomidory");
        SubmitEditor(cut);

        Assert.NotNull(editorService.LastRequest);
        Assert.Equal("Nowa pomidorowa", editorService.LastRequest!.Title);

        var navigationManager = context.Services.GetRequiredService<NavigationManager>();
        Assert.Contains("/recipes/11111111-1111-1111-1111-111111111111", navigationManager.Uri);
    }

    [Fact]
    public void Editor_ShouldRenderIngredientAndTagSuggestions()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<IRecipeEditorService>(new FakeRecipeEditorService());

        var cut = context.Render<RecipeEditorPage>();

        Assert.Contains("datalist id=\"ingredient-suggestion-list\"", cut.Markup);
        Assert.Contains("value=\"Pomidor\"", cut.Markup);
        Assert.Contains("datalist id=\"tag-suggestion-list\"", cut.Markup);
        Assert.Contains("value=\"szybkie\"", cut.Markup);
    }

    [Fact]
    public void Editor_ShouldUseSubmitAndLabelDynamicFields()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<IRecipeEditorService>(new FakeRecipeEditorService());

        var cut = context.Render<RecipeEditorPage>();
        var ingredientName = cut.Find("input.ingredient-name");
        var ingredientQuantity = cut.Find("input.ingredient-qty");
        var ingredientUnit = cut.Find("select.ingredient-unit");
        var step = cut.Find(".step-row textarea");

        Assert.Equal("submit", FindSaveButton(cut).GetAttribute("type"));
        Assert.NotNull(cut.Find($"label[for='{ingredientName.Id}']"));
        Assert.NotNull(cut.Find($"label[for='{ingredientQuantity.Id}']"));
        Assert.NotNull(cut.Find($"label[for='{ingredientUnit.Id}']"));
        Assert.NotNull(cut.Find($"label[for='{step.Id}']"));
        Assert.Equal(4, cut.FindAll("details.editor-anchor-section").Count);
        Assert.NotNull(cut.Find(".editor-actions"));
    }

    [Fact]
    public void Editor_ShouldShowConflictFeedbackWithoutValidationSummaryException()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var editorService = new FakeRecipeEditorService
        {
            SaveResult = RecipeSaveResult.Failure("Tytuł przepisu musi być unikalny."),
        };
        context.Services.AddSingleton<IRecipeEditorService>(editorService);

        var cut = context.Render<RecipeEditorPage>();
        cut.Find("#recipe-title").Change("Duplikat");
        cut.Find("input[placeholder='np. pomidor']").Input("Pomidor");
        cut.Find("textarea").Change("Pokrój pomidory");

        SubmitEditor(cut);

        cut.WaitForAssertion(() =>
            Assert.Contains("Tytuł przepisu musi być unikalny.", cut.Markup)
        );
        Assert.Equal("alert", cut.Find("[role='alert']").GetAttribute("role"));
    }

    private sealed class FakeRecipeEditorService : IRecipeEditorService
    {
        public RecipeSaveRequest? LastRequest { get; private set; }
        public RecipeSaveResult? SaveResult { get; set; }

        public Task<RecipeEditorDraft?> GetByIdAsync(
            Guid recipeId,
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult<RecipeEditorDraft?>(null);
        }

        public Task<RecipeSaveResult> SaveAsync(
            Guid? recipeId,
            RecipeSaveRequest request,
            CancellationToken cancellationToken = default
        )
        {
            LastRequest = request;
            return Task.FromResult(
                SaveResult
                    ?? RecipeSaveResult.Success(Guid.Parse("11111111-1111-1111-1111-111111111111"))
            );
        }

        public Task<RecipePhotoUploadResult> UploadPhotoAsync(
            Guid recipeId,
            Stream stream,
            string fileName,
            string? contentType,
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult(RecipePhotoUploadResult.Success("images/fake.jpg"));
        }

        public Task<IReadOnlyList<string>> GetIngredientSuggestionsAsync(
            string query,
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult((IReadOnlyList<string>)["Pomidor", "Ogorek", "Feta"]);
        }

        public Task<IReadOnlyList<string>> GetTagSuggestionsAsync(
            string query,
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult((IReadOnlyList<string>)["szybkie", "obiad", "klasyk"]);
        }
    }

    private static IElement FindSaveButton(IRenderedComponent<RecipeEditorPage> cut)
    {
        return cut.FindAll("button")
            .Single(x =>
                x.TextContent.Trim().Equals("Zapisz przepis", StringComparison.OrdinalIgnoreCase)
            );
    }

    private static void SubmitEditor(IRenderedComponent<RecipeEditorPage> cut) =>
        cut.Find("form").Submit();
}

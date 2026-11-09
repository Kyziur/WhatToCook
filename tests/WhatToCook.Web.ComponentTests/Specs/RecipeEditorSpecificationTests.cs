using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CreateRecipe_WhenPhotoUploadFails_ShouldRetryWithoutCreatingDuplicate(
        bool throwOnUpload
    )
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var editorService = new FakeRecipeEditorService
        {
            UploadResult = RecipePhotoUploadResult.Failure("Nie udało się wysłać zdjęcia."),
            ThrowOnUpload = throwOnUpload,
        };
        context.Services.AddSingleton<IRecipeEditorService>(editorService);

        var cut = context.Render<RecipeEditorPage>();
        cut.Find("#recipe-title").Change("Pomidorowa ze zdjęciem");
        cut.Find("input[placeholder='np. pomidor']").Input("Pomidor");
        cut.Find("textarea").Change("Pokrój pomidor");
        cut.FindComponent<InputFile>()
            .UploadFiles(
                InputFileContent.CreateFromBinary([1, 2, 3], "photo.png", contentType: "image/png")
            );

        SubmitEditor(cut);

        cut.WaitForAssertion(() =>
        {
            Assert.Single(editorService.SavedRecipeIds);
            Assert.Null(editorService.SavedRecipeIds[0]);
            Assert.Contains("Nie udało się", cut.Markup);
        });

        editorService.UploadResult = RecipePhotoUploadResult.Success("images/fake.jpg");
        editorService.ThrowOnUpload = false;
        SubmitEditor(cut);

        cut.WaitForAssertion(() => Assert.Equal(2, editorService.SavedRecipeIds.Count));
        Assert.Equal(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            editorService.SavedRecipeIds[1]
        );
        Assert.Contains(
            "/recipes/11111111-1111-1111-1111-111111111111",
            context.Services.GetRequiredService<NavigationManager>().Uri
        );
    }

    [Fact]
    public void SaveRecipe_AfterApiFailure_ShouldAllowRetry()
    {
        using var context = new BunitContext();
        var editorService = new FakeRecipeEditorService
        {
            SaveResult = RecipeSaveResult.Failure("Chwilowy błąd zapisu."),
        };
        context.Services.AddSingleton<IRecipeEditorService>(editorService);

        var cut = context.Render<RecipeEditorPage>();
        cut.Find("#recipe-title").Change("Krem z dyni");
        cut.Find("input[placeholder='np. pomidor']").Input("Dynia");
        cut.Find("textarea").Change("Ugotuj dynię");

        SubmitEditor(cut);
        Assert.Single(editorService.SavedRecipeIds);
        Assert.Contains("Chwilowy błąd zapisu.", cut.Markup);
        Assert.False(FindSaveButton(cut).HasAttribute("disabled"));

        editorService.SaveResult = null;
        SubmitEditor(cut);

        Assert.Equal(2, editorService.SavedRecipeIds.Count);
        Assert.All(editorService.SavedRecipeIds, recipeId => Assert.Null(recipeId));
        Assert.Contains(
            "/recipes/11111111-1111-1111-1111-111111111111",
            context.Services.GetRequiredService<NavigationManager>().Uri
        );
    }

    [Fact]
    public void Editor_WhenReusedForNewRoute_ShouldDiscardPreviousRecipeState()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var recipeId = Guid.NewGuid();
        var editorService = new FakeRecipeEditorService();
        editorService.Drafts[recipeId] = CreateDraft(recipeId, "Wczytany przepis");
        context.Services.AddSingleton<IRecipeEditorService>(editorService);

        var cut = context.Render<RecipeEditorPage>(parameters =>
            parameters.Add(x => x.RecipeId, recipeId)
        );
        Assert.Contains("Wczytany przepis", cut.Markup);

        cut.Render(parameters => parameters.Add(x => x.RecipeId, (Guid?)null));

        Assert.DoesNotContain("Wczytany przepis", cut.Markup);
        Assert.Empty(cut.FindAll(".recipe-photo-image"));
        Assert.Equal(string.Empty, cut.Find("#recipe-title").GetAttribute("value"));

        cut.Find("#recipe-title").Change("Nowy przepis");
        cut.Find("input[placeholder='np. pomidor']").Input("Cebula");
        cut.Find("textarea").Change("Pokrój cebulę");
        SubmitEditor(cut);

        Assert.Null(Assert.Single(editorService.SavedRecipeIds));
    }

    [Fact]
    public void Editor_WhenSwitchingEditedRecipes_ShouldNotUploadOldSelectedPhoto()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var editorService = new FakeRecipeEditorService();
        editorService.Drafts[firstId] = CreateDraft(firstId, "Przepis pierwszy");
        editorService.Drafts[secondId] = CreateDraft(secondId, "Przepis drugi");
        context.Services.AddSingleton<IRecipeEditorService>(editorService);

        var cut = context.Render<RecipeEditorPage>(parameters =>
            parameters.Add(x => x.RecipeId, firstId)
        );
        cut.FindComponent<InputFile>()
            .UploadFiles(
                InputFileContent.CreateFromBinary([1, 2, 3], "photo.png", contentType: "image/png")
            );

        cut.Render(parameters => parameters.Add(x => x.RecipeId, secondId));
        Assert.Contains("Przepis drugi", cut.Markup);
        SubmitEditor(cut);

        Assert.Equal(secondId, Assert.Single(editorService.SavedRecipeIds));
        Assert.Equal(0, editorService.UploadCalls);
    }

    [Fact]
    public void Editor_WhenTargetRecipeIsMissing_ShouldDisableStaleDraft()
    {
        using var context = new BunitContext();
        var firstId = Guid.NewGuid();
        var missingId = Guid.NewGuid();
        var editorService = new FakeRecipeEditorService();
        editorService.Drafts[firstId] = CreateDraft(firstId, "Poprzedni przepis");
        context.Services.AddSingleton<IRecipeEditorService>(editorService);

        var cut = context.Render<RecipeEditorPage>(parameters =>
            parameters.Add(x => x.RecipeId, firstId)
        );
        cut.Render(parameters => parameters.Add(x => x.RecipeId, missingId));

        Assert.Contains("Nie znaleziono przepisu do edycji.", cut.Markup);
        Assert.True(cut.Find(".editor-form-fields").HasAttribute("disabled"));
        Assert.Empty(editorService.SavedRecipeIds);
    }

    private static RecipeEditorDraft CreateDraft(Guid id, string title) =>
        new(
            id,
            title,
            4,
            "Notatka",
            "images/test.jpg",
            ["obiad"],
            [new RecipeSaveIngredient("Pomidor", "1", "szt")],
            ["Ugotuj pomidor"]
        );

    private sealed class FakeRecipeEditorService : IRecipeEditorService
    {
        public RecipeSaveRequest? LastRequest { get; private set; }
        public RecipeSaveResult? SaveResult { get; set; }
        public RecipePhotoUploadResult? UploadResult { get; set; }
        public bool ThrowOnUpload { get; set; }
        public List<Guid?> SavedRecipeIds { get; } = [];

        public Dictionary<Guid, RecipeEditorDraft> Drafts { get; } = new();
        public int UploadCalls { get; private set; }

        public Task<RecipeEditorDraft?> GetByIdAsync(
            Guid recipeId,
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult(Drafts.GetValueOrDefault(recipeId));
        }

        public Task<RecipeSaveResult> SaveAsync(
            Guid? recipeId,
            RecipeSaveRequest request,
            CancellationToken cancellationToken = default
        )
        {
            LastRequest = request;
            SavedRecipeIds.Add(recipeId);
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
            UploadCalls++;
            if (ThrowOnUpload)
            {
                throw new IOException("Photo stream unavailable");
            }

            return Task.FromResult(
                UploadResult ?? RecipePhotoUploadResult.Success("images/fake.jpg")
            );
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

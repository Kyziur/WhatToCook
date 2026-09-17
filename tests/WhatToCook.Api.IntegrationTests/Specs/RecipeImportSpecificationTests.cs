using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace WhatToCook.Api.IntegrationTests.Specs;

public sealed class RecipeImportSpecificationTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public async Task CreateImportDraftFromSupportedRecipePage_ShouldPersistDraft()
    {
        using var client = factory.CreateClient();
        var url = "https://aniagotuje.pl/przepis/makaron-z-pesto";
        factory.ImportedPagesByUrl[url] = CreateAniaGotujeHtml("Makaron z pesto", "4 porcje");

        var response = await client.PostAsJsonAsync(
            "/api/recipe-imports/url",
            new RecipeApiContracts.CreateRecipeImportFromUrlPayload { Url = url }
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var draft =
            await response.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeImportDraftDto>(
                JsonOptions
            );
        Assert.NotNull(draft);
        Assert.Equal("url", draft!.SourceType);
        Assert.Equal("readytofinalize", draft.Status);
        Assert.True(draft.CanFinalize);
        Assert.Equal("Makaron z pesto", draft.Title);
        Assert.Equal(4, draft.Servings);
        Assert.Equal(url, draft.SourceUrl);
        Assert.Equal(3, draft.Ingredients.Count);
        Assert.Equal(3, draft.Steps.Count);
        Assert.DoesNotContain(
            draft.Steps,
            x => x.Contains("Kopiuj", StringComparison.OrdinalIgnoreCase)
        );
        Assert.DoesNotContain(
            draft.Steps,
            x => x.Contains("Ukryj zdjecia", StringComparison.OrdinalIgnoreCase)
        );

        var reopened = await client.GetFromJsonAsync<RecipeApiContracts.RecipeImportDraftDto>(
            $"/api/recipe-imports/{draft.Id}",
            JsonOptions
        );
        Assert.NotNull(reopened);
        Assert.Equal(draft.Id, reopened!.Id);
        Assert.Equal("Makaron z pesto", reopened.Title);
    }

    [Theory]
    [MemberData(nameof(RealAniaGotujeImportFixtures))]
    public async Task CreateImportDraft_FromRealAniaGotujeVariants_ShouldExtractIngredients(
        string url,
        string html,
        string expectedTitle,
        int expectedServings,
        string expectedIngredientName,
        string? expectedQuantityText,
        string? expectedUnit
    )
    {
        using var client = factory.CreateClient();
        factory.ImportedPagesByUrl[url] = html;

        var response = await client.PostAsJsonAsync(
            "/api/recipe-imports/url",
            new RecipeApiContracts.CreateRecipeImportFromUrlPayload { Url = url }
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var draft =
            await response.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeImportDraftDto>(
                JsonOptions
            );

        Assert.NotNull(draft);
        Assert.Equal(expectedTitle, draft!.Title);
        Assert.Equal(expectedServings, draft.Servings);
        Assert.NotEmpty(draft.Ingredients);
        Assert.NotEmpty(draft.Steps);
        Assert.DoesNotContain(draft.Ingredients, x => x.Name.Contains('<') || x.Name.Contains('>'));

        Assert.Contains(
            draft.Ingredients,
            x =>
                x.Name.Contains(expectedIngredientName, StringComparison.OrdinalIgnoreCase)
                && x.QuantityText == expectedQuantityText
                && x.Unit == expectedUnit
        );
    }

    [Theory]
    [MemberData(nameof(RealRozkosznyImportFixtures))]
    public async Task CreateImportDraft_FromRealRozkosznyVariants_ShouldExtractRecipeContent(
        string url,
        string html,
        string expectedTitle,
        int? expectedServings,
        bool expectedCanFinalize,
        string expectedIngredientName,
        string? expectedQuantityText,
        string? expectedUnit,
        int expectedStepCount
    )
    {
        using var client = factory.CreateClient();
        factory.ImportedPagesByUrl[url] = html;

        var response = await client.PostAsJsonAsync(
            "/api/recipe-imports/url",
            new RecipeApiContracts.CreateRecipeImportFromUrlPayload { Url = url }
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var draft =
            await response.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeImportDraftDto>(
                JsonOptions
            );

        Assert.NotNull(draft);
        Assert.Equal(expectedTitle, draft!.Title);
        Assert.Equal(expectedServings, draft.Servings);
        Assert.Equal(expectedCanFinalize, draft.CanFinalize);
        Assert.NotEmpty(draft.Ingredients);
        Assert.Equal(expectedStepCount, draft.Steps.Count);

        Assert.Contains(
            draft.Ingredients,
            x =>
                x.Name.Contains(expectedIngredientName, StringComparison.OrdinalIgnoreCase)
                && x.QuantityText == expectedQuantityText
                && x.Unit == expectedUnit
        );

        if (!expectedCanFinalize)
        {
            Assert.Contains(draft.Issues, x => x.FieldPath == "Servings");
        }
    }

    [Theory]
    [MemberData(nameof(RealJadlonomiaImportFixtures))]
    public async Task CreateImportDraft_FromRealJadlonomiaVariants_ShouldExtractRecipeContent(
        string url,
        string html,
        string expectedTitle,
        int? expectedServings,
        bool expectedCanFinalize,
        string expectedIngredientName,
        string? expectedQuantityText,
        string? expectedUnit,
        int expectedStepCount
    )
    {
        using var client = factory.CreateClient();
        factory.ImportedPagesByUrl[url] = html;

        var response = await client.PostAsJsonAsync(
            "/api/recipe-imports/url",
            new RecipeApiContracts.CreateRecipeImportFromUrlPayload { Url = url }
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var draft =
            await response.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeImportDraftDto>(
                JsonOptions
            );

        Assert.NotNull(draft);
        Assert.Equal(expectedTitle, draft!.Title);
        Assert.Equal(expectedServings, draft.Servings);
        Assert.Equal(expectedCanFinalize, draft.CanFinalize);
        Assert.NotEmpty(draft.Ingredients);
        Assert.Equal(expectedStepCount, draft.Steps.Count);

        Assert.Contains(
            draft.Ingredients,
            x =>
                x.Name.Contains(expectedIngredientName, StringComparison.OrdinalIgnoreCase)
                && x.QuantityText == expectedQuantityText
                && x.Unit == expectedUnit
        );

        if (!expectedCanFinalize)
        {
            Assert.Contains(draft.Issues, x => x.FieldPath == "Servings");
        }
    }

    [Theory]
    [MemberData(nameof(RealMojeWypiekiImportFixtures))]
    public async Task CreateImportDraft_FromRealMojeWypiekiVariants_ShouldExtractRecipeContent(
        string url,
        string html,
        string expectedTitle,
        int? expectedServings,
        bool expectedCanFinalize,
        string expectedIngredientName,
        string? expectedQuantityText,
        string? expectedUnit,
        int expectedStepCount
    )
    {
        using var client = factory.CreateClient();
        factory.ImportedPagesByUrl[url] = html;

        var response = await client.PostAsJsonAsync(
            "/api/recipe-imports/url",
            new RecipeApiContracts.CreateRecipeImportFromUrlPayload { Url = url }
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var draft =
            await response.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeImportDraftDto>(
                JsonOptions
            );

        Assert.NotNull(draft);
        Assert.Equal(expectedTitle, draft!.Title);
        Assert.Equal(expectedServings, draft.Servings);
        Assert.Equal(expectedCanFinalize, draft.CanFinalize);
        Assert.NotEmpty(draft.Ingredients);
        Assert.Equal(expectedStepCount, draft.Steps.Count);

        Assert.Contains(
            draft.Ingredients,
            x =>
                x.Name.Contains(expectedIngredientName, StringComparison.OrdinalIgnoreCase)
                && x.QuantityText == expectedQuantityText
                && x.Unit == expectedUnit
        );

        if (!expectedCanFinalize)
        {
            Assert.Contains(draft.Issues, x => x.FieldPath == "Servings");
        }
    }

    [Fact]
    public async Task CreateImportDraft_ShouldIgnoreNonRecipeListItemsBeforeIngredientSection()
    {
        using var client = factory.CreateClient();
        var url = "https://aniagotuje.pl/przepis/noisy-drozdzowki";
        factory.ImportedPagesByUrl[url] = CreateNoisyAniaGotujeHtml();

        var response = await client.PostAsJsonAsync(
            "/api/recipe-imports/url",
            new RecipeApiContracts.CreateRecipeImportFromUrlPayload { Url = url }
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var draft =
            await response.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeImportDraftDto>(
                JsonOptions
            );

        Assert.NotNull(draft);
        Assert.Equal(3, draft!.Ingredients.Count);
        Assert.All(draft.Ingredients, x => Assert.True(x.Name.Length < 200));
        Assert.DoesNotContain(
            draft.Ingredients,
            x => x.Name.Contains("Logowanie", StringComparison.OrdinalIgnoreCase)
        );
    }

    [Fact]
    public async Task CreateImportDraft_ShouldRejectInvalidUrl()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/recipe-imports/url",
            new RecipeApiContracts.CreateRecipeImportFromUrlPayload { Url = "nie-url" }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateImportDraft_ShouldReportUnsupportedSource()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/recipe-imports/url",
            new RecipeApiContracts.CreateRecipeImportFromUrlPayload
            {
                Url = "https://example.com/przepis/test",
            }
        );

        Assert.Equal((HttpStatusCode)422, response.StatusCode);
    }

    [Fact]
    public async Task FinalizeDraft_ShouldBlockWhenRequiredFieldsAreMissing()
    {
        using var client = factory.CreateClient();
        var url = "https://aniagotuje.pl/przepis/blokowany-draft";
        factory.ImportedPagesByUrl[url] = CreateAniaGotujeHtml(
            "Blokowany draft",
            "keksowka 11 x 30 cm"
        );

        var createResponse = await client.PostAsJsonAsync(
            "/api/recipe-imports/url",
            new RecipeApiContracts.CreateRecipeImportFromUrlPayload { Url = url }
        );
        createResponse.EnsureSuccessStatusCode();
        var draft =
            await createResponse.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeImportDraftDto>(
                JsonOptions
            );

        Assert.NotNull(draft);
        Assert.False(draft!.CanFinalize);
        Assert.Contains(draft.Issues, x => x.FieldPath == "Servings");

        var finalizeResponse = await client.PostAsync(
            $"/api/recipe-imports/{draft.Id}/finalize",
            content: null
        );

        Assert.Equal(HttpStatusCode.BadRequest, finalizeResponse.StatusCode);
        var blocked =
            await finalizeResponse.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeImportDraftDto>(
                JsonOptions
            );
        Assert.NotNull(blocked);
        Assert.Contains(blocked!.Issues, x => x.FieldPath == "Servings");
    }

    [Fact]
    public async Task CorrectDraftAndFinalize_ShouldCreateRecipeAndMarkDraftFinalized()
    {
        using var client = factory.CreateClient();
        var url = "https://aniagotuje.pl/przepis/kurczak-z-ryzem";
        factory.ImportedPagesByUrl[url] = CreateAniaGotujeHtml(
            "Kurczak z ryzem",
            "keksowka 11 x 30 cm"
        );

        var createResponse = await client.PostAsJsonAsync(
            "/api/recipe-imports/url",
            new RecipeApiContracts.CreateRecipeImportFromUrlPayload { Url = url }
        );
        createResponse.EnsureSuccessStatusCode();
        var createdDraft =
            await createResponse.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeImportDraftDto>(
                JsonOptions
            );

        var updatePayload = new RecipeApiContracts.UpdateRecipeImportDraftPayload
        {
            Title = "Kurczak z ryzem",
            Servings = 4,
            Source = url,
            Ingredients =
            [
                new RecipeApiContracts.RecipeImportIngredientPayload("kurczak", "500", "g"),
                new RecipeApiContracts.RecipeImportIngredientPayload("ryz", "250", "g"),
            ],
            Steps = ["Podsmaz kurczaka.", "Ugotuj ryz i podaj razem."],
            Tags = ["obiad"],
        };

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/recipe-imports/{createdDraft!.Id}",
            updatePayload
        );
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated =
            await updateResponse.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeImportDraftDto>(
                JsonOptions
            );
        Assert.NotNull(updated);
        Assert.True(updated!.CanFinalize);
        Assert.Empty(updated.Issues);

        var finalizeResponse = await client.PostAsync(
            $"/api/recipe-imports/{createdDraft.Id}/finalize",
            content: null
        );
        Assert.Equal(HttpStatusCode.Created, finalizeResponse.StatusCode);
        var recipe =
            await finalizeResponse.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeDetailsDto>(
                JsonOptions
            );
        Assert.NotNull(recipe);
        Assert.Equal("Kurczak z ryzem", recipe!.Title);
        Assert.Equal(4, recipe.Servings);

        var finalized = await client.GetFromJsonAsync<RecipeApiContracts.RecipeImportDraftDto>(
            $"/api/recipe-imports/{createdDraft.Id}",
            JsonOptions
        );
        Assert.NotNull(finalized);
        Assert.Equal("finalized", finalized!.Status);
        Assert.Equal(recipe.Id, finalized.FinalizedRecipeId);
    }

    [Fact]
    public async Task FinalizeDraft_ShouldBlockOnDuplicateNormalizedTitle()
    {
        using var client = factory.CreateClient();
        await CreateRecipe(client, "Piernik domowy");

        var url = "https://aniagotuje.pl/przepis/piernik-domowy";
        factory.ImportedPagesByUrl[url] = CreateAniaGotujeHtml("Piernik domowy", "8 porcji");

        var createResponse = await client.PostAsJsonAsync(
            "/api/recipe-imports/url",
            new RecipeApiContracts.CreateRecipeImportFromUrlPayload { Url = url }
        );
        createResponse.EnsureSuccessStatusCode();
        var draft =
            await createResponse.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeImportDraftDto>(
                JsonOptions
            );

        Assert.NotNull(draft);
        Assert.False(draft!.CanFinalize);
        Assert.Contains(draft.Issues, x => x.Code == "duplicate-title");

        var finalizeResponse = await client.PostAsync(
            $"/api/recipe-imports/{draft.Id}/finalize",
            content: null
        );
        Assert.Equal(HttpStatusCode.BadRequest, finalizeResponse.StatusCode);
    }

    private static string CreateAniaGotujeHtml(string title, string servingsText)
    {
        return $$"""
            <html>
              <body>
                <article itemtype="https://schema.org/Recipe" class="row">
                  <meta itemprop="recipeYield" content="{{servingsText}}">
                  <div class="article-content col-12">
                    <h1 itemprop="name">{{title}}</h1>
                    <div id="article-content-body123" itemprop="recipeInstructions" class="article-content-body">
                      <div class="article-intro">
                        <p><strong>{{title}}</strong> to prosty obiad na co dzien.</p>
                      </div>
                      <p class="recipe-info">
                        <strong>Liczba porcji:</strong> {{servingsText}} <br/>
                      </p>
                      <div id="recipeIngredients">
                        <p class="ing-header"><strong>Skladniki</strong></p>
                        <ul class="recipe-ing-list">
                          <li><span itemprop="recipeIngredient"><span class="ingredient">250 g makaronu</span></span></li>
                          <li><span itemprop="recipeIngredient"><span class="ingredient">3 lyzki pesto</span></span></li>
                          <li><span itemprop="recipeIngredient"><span class="ingredient">30 g parmezanu</span></span></li>
                        </ul>
                      </div>
                    </div>
                    <div data-nosnippet="" class="copy-share-lock-con">
                      <div class="share-ingredients">
                        <div class="share-head">Skladniki:</div>
                        <div class="btn copyIcon-button">Kopiuj</div>
                      </div>
                      <div class="wake-photo-con">
                        <div class="btn toggle-photos">Ukryj zdjecia</div>
                      </div>
                    </div>
                    <div>
                      <h2>{{title}}</h2>
                      <p>Ugotuj makaron al dente w osolonej wodzie.</p>
                      <div class="img-placeholder">
                        <img alt="przykladowe zdjecie" src="https://cdn.example.test/makaron.jpg">
                      </div>
                      <p>Podgrzej pesto z odrobina wody z gotowania makaronu.</p>
                      <h3>Wykonczenie</h3>
                      <p>Wymieszaj makaron z pesto i posyp parmezanem przed podaniem.</p>
                    </div>
                    <div id="vote"></div>
                  </div>
                </article>
              </body>
            </html>
            """;
    }

    public static TheoryData<
        string,
        string,
        string,
        int,
        string,
        string?,
        string?
    > RealAniaGotujeImportFixtures() =>
        new()
        {
            {
                "https://aniagotuje.pl/przepis/drozdzowki-z-rabarbarem-i-truskawkami",
                CreateRealAniaGotujeFixture2026(),
                "Drożdżówki z rabarbarem i truskawkami",
                12,
                "mąka pszenna uniwersalna",
                "650",
                "g"
            },
            {
                "https://aniagotuje.pl/przepis/racuchy-z-rabarbarem",
                CreateRealAniaGotujeFixture2024(),
                "Racuchy z rabarbarem",
                12,
                "mąki pszennej",
                "2",
                "szklanka"
            },
            {
                "https://aniagotuje.pl/przepis/calzone",
                CreateRealAniaGotujeFixture2020(),
                "Calzone",
                4,
                "mąki pszennej",
                "ponad 2,5",
                "szklanka"
            },
        };

    public static TheoryData<
        string,
        string,
        string,
        int?,
        bool,
        string,
        string?,
        string?,
        int
    > RealRozkosznyImportFixtures() =>
        new()
        {
            {
                "https://www.rozkoszny.pl/wiosenna-zupa-jarzynowa-ze-szparagami-i-pistou/",
                CreateRealRozkosznyFixture2026(),
                "Wiosenna zupa jarzynowa ze szparagami i pistou",
                4,
                true,
                "groszku mrozonego",
                "200",
                "g",
                6
            },
            {
                "https://www.rozkoszny.pl/rabarbarowy-placek-z-budyniem-i-kruszonka/",
                CreateRealRozkosznyFixture2025(),
                "Rabarbarowy placek z budyniem i kruszonka",
                null,
                false,
                "maki pszennej",
                "3/4",
                "szklanka",
                6
            },
            {
                "https://www.rozkoszny.pl/piklowany-rabarbar/",
                CreateRealRozkosznyFixture2019(),
                "Piklowany rabarbar",
                2,
                true,
                "octu z czerwonego wina",
                "1/2",
                "szklanka",
                2
            },
        };

    public static TheoryData<
        string,
        string,
        string,
        int?,
        bool,
        string,
        string?,
        string?,
        int
    > RealJadlonomiaImportFixtures() =>
        new()
        {
            {
                "https://jadlonomia.com/przepisy/mistrzowska-salatka-z-bobu-i-awokado/",
                CreateRealJadlonomiaFixture2016(),
                "Mistrzowska sałatka z bobu i awokado",
                2,
                true,
                "bobu",
                "500",
                "g",
                4
            },
            {
                "https://jadlonomia.com/przepisy/proste-ciasto-z-truskawkami/",
                CreateRealJadlonomiaFixture2026(),
                "Proste ciasto z truskawkami",
                null,
                false,
                "maki pszennej",
                "2",
                "szklanka",
                3
            },
            {
                "https://jadlonomia.com/przepisy/kremowy-weganski-sernik/",
                CreateRealJadlonomiaFixture2021(),
                "Jogurtowy sernik",
                null,
                false,
                "jogurtu kokosowego",
                "800",
                "g",
                3
            },
            {
                "https://jadlonomia.com/przepisy/granola-jabkowa/",
                CreateRealJadlonomiaFixture2014(),
                "Granola jablkowa",
                null,
                false,
                "platkow owsianych",
                "1",
                "szklanka",
                2
            },
        };

    public static TheoryData<
        string,
        string,
        string,
        int?,
        bool,
        string,
        string?,
        string?,
        int
    > RealMojeWypiekiImportFixtures() =>
        new()
        {
            {
                "https://mojewypieki.com/przepis/pistacjowa-panna-cotta",
                CreateRealMojeWypiekiFixture2026(),
                "Pistacjowa panna cotta",
                4,
                true,
                "śmietanki kremówki",
                "300",
                "ml",
                3
            },
            {
                "https://mojewypieki.com/przepis/brookie-ciasteczkowe-brownie",
                CreateRealMojeWypiekiFixture2024(),
                "Brookie ciasteczkowe brownie",
                null,
                false,
                "mąki pszennej",
                "180",
                "g",
                3
            },
            {
                "https://mojewypieki.com/przepis/sernik-z-rosa",
                CreateRealMojeWypiekiFixture2019(),
                "Sernik z rosa",
                null,
                false,
                "masła",
                "125",
                "g",
                3
            },
        };

    private static string CreateRealAniaGotujeFixture2026() =>
        """
            <html>
              <body>
                <article itemtype="https://schema.org/Recipe" class="row">
                  <meta itemprop="recipeYield" content="12 sporych drożdżówek">
                  <div class="article-content col-12">
                    <h1 itemprop="name">Drożdżówki z rabarbarem i truskawkami</h1>
                    <div id="article-content-body123" itemprop="recipeInstructions" class="article-content-body">
                      <div id="recipeIngredients">
                        <p class="ing-header"><strong>Składniki na drożdżówki</strong></p>
                        <ul class="recipe-ing-list">
                          <li><i class="icon icon-circle"></i><span itemprop="recipeIngredient"><span class="ingredient">mąka pszenna uniwersalna</span> <span class="qty">650 g - 4 niepełne szklanki</span></span></li>
                          <li><i class="icon icon-circle"></i><span itemprop="recipeIngredient"><span class="ingredient">masło lub kostka roślinna do pieczenia</span> <span class="qty">100 g</span></span></li>
                          <li><i class="icon icon-circle"></i><span itemprop="recipeIngredient"><span class="ingredient">cukier drobny</span> <span class="qty">100 g</span></span></li>
                          <li><i class="icon icon-circle"></i><span itemprop="recipeIngredient"><span class="ingredient">mleko, najlepiej tłuste</span> <span class="qty">250 ml</span></span></li>
                        </ul>
                      </div>
                    </div>
                    <div>
                      <h2>Krok 1: Przygotuj składniki</h2>
                      <p>Przygotuj sobie wszystkie składniki na ciasto drożdżowe i odważ mąkę, mleko oraz drożdże.</p>
                      <h2>Krok 2: Zrób rozczyn</h2>
                      <p>Drożdże rozetrzyj z mlekiem, cukrem i odrobiną mąki, a następnie odstaw rozczyn do wyrośnięcia.</p>
                    </div>
                    <div id="vote"></div>
                  </div>
                </article>
              </body>
            </html>
            """;

    private static string CreateRealAniaGotujeFixture2024() =>
        """
            <html>
              <body>
                <article itemtype="https://schema.org/Recipe" class="row">
                  <meta itemprop="recipeYield" content="12 dużych racuchów">
                  <div class="article-content col-12">
                    <h1 itemprop="name">Racuchy z rabarbarem</h1>
                    <div id="article-content-body123" itemprop="recipeInstructions" class="article-content-body">
                      <div id="recipeIngredients">
                        <p class="ing-header"><strong>Składniki</strong></p>
                        <ul class="recipe-ing-list">
                          <li><i class="icon icon-circle"></i><span itemprop="recipeIngredient"><span class="ingredient">2 szklanki mąki pszennej np. tortowej - 320 g</span></span></li>
                          <li><i class="icon icon-circle"></i><span itemprop="recipeIngredient"><span class="ingredient">1 szklanka mleka (może być roślinne) - 250 ml</span></span></li>
                          <li><i class="icon icon-circle"></i><span itemprop="recipeIngredient"><span class="ingredient">40 g świeżych drożdży lub 14 g drożdży instant</span></span></li>
                        </ul>
                      </div>
                    </div>
                    <div>
                      <h2>Krok 1: Zrób zaczyn</h2>
                      <p>Połącz drożdże z mlekiem i częścią mąki, a następnie odstaw zaczyn na kilka minut.</p>
                      <h2>Krok 2: Usmaż racuchy</h2>
                      <p>Wymieszaj ciasto z rabarbarem i smaż racuchy na złoto z obu stron.</p>
                    </div>
                    <div id="vote"></div>
                  </div>
                </article>
              </body>
            </html>
            """;

    private static string CreateRealAniaGotujeFixture2020() =>
        """
            <html>
              <body>
                <article itemtype="https://schema.org/Recipe" class="row">
                  <meta itemprop="recipeYield" content="4 bardzo duże pierogi">
                  <div class="article-content col-12">
                    <h1 itemprop="name">Calzone</h1>
                    <div id="article-content-body123" itemprop="recipeInstructions" class="article-content-body">
                      <div id="recipeIngredients">
                        <p class="ing-header"><strong>Składniki na ciasto</strong></p>
                        <ul class="recipe-ing-list">
                          <li><i class="icon icon-circle"></i><span itemprop="recipeIngredient"><span class="ingredient">ponad 2,5 szklanki mąki pszennej - najlepiej typ. 00* - 450 gramów</span></span></li>
                          <li><i class="icon icon-circle"></i><span itemprop="recipeIngredient"><span class="ingredient">1 szklanka ciepłej wody - 250 ml</span></span></li>
                          <li><i class="icon icon-circle"></i><span itemprop="recipeIngredient"><span class="ingredient">2 łyżki delikatnej oliwy z oliwek</span></span></li>
                        </ul>
                      </div>
                    </div>
                    <div>
                      <h2>Krok 1: Przygotuj ciasto</h2>
                      <p>Wyrób elastyczne ciasto z mąki, wody, oliwy oraz drożdży i odstaw je do wyrośnięcia.</p>
                      <h2>Krok 2: Uformuj calzone</h2>
                      <p>Napełnij placki farszem, złóż je na pół i piecz do mocnego zrumienienia.</p>
                    </div>
                    <div id="vote"></div>
                  </div>
                </article>
              </body>
            </html>
            """;

    private static string CreateNoisyAniaGotujeHtml() =>
        """
            <html>
              <body>
                <ul class="top-nav">
                  <li><a href="/login">Logowanie</a></li>
                  <li><a href="/kontakt">Współpraca</a></li>
                </ul>
                <article itemtype="https://schema.org/Recipe" class="row">
                  <meta itemprop="recipeYield" content="12 sporych drożdżówek">
                  <div class="article-content col-12">
                    <h1 itemprop="name">Drożdżówki z rabarbarem i truskawkami</h1>
                    <div id="article-content-body123" itemprop="recipeInstructions" class="article-content-body">
                      <script type="application/ld+json">
                        {"@type":"Recipe","name":"Drożdżówki z rabarbarem i truskawkami"}
                      </script>
                      <div id="recipeIngredients">
                        <p class="ing-header"><strong>Składniki na drożdżówki</strong></p>
                        <ul class="recipe-ing-list">
                          <li><i class="icon icon-circle"></i><span itemprop="recipeIngredient"><span class="ingredient">mąka pszenna uniwersalna</span> <span class="qty">650 g - 4 niepełne szklanki</span></span></li>
                          <li><i class="icon icon-circle"></i><span itemprop="recipeIngredient"><span class="ingredient">mleko, najlepiej tłuste</span> <span class="qty">250 ml</span></span></li>
                          <li><i class="icon icon-circle"></i><span itemprop="recipeIngredient"><span class="ingredient">świeże drożdże</span> <span class="qty">40 g</span></span></li>
                        </ul>
                      </div>
                    </div>
                    <div>
                      <h2>Krok 1: Przygotuj składniki</h2>
                      <p>Przygotuj sobie wszystkie składniki na ciasto drożdżowe.</p>
                      <h2>Krok 2: Zrób rozczyn</h2>
                      <p>Drożdże rozetrzyj z mlekiem i odrobiną mąki.</p>
                    </div>
                    <div id="vote"></div>
                  </div>
                </article>
              </body>
            </html>
            """;

    private static string CreateRealRozkosznyFixture2026() =>
        """
            <html>
              <body>
                <article class="elementor-location-single">
                  <h1>Wiosenna zupa jarzynowa ze szparagami i pistou</h1>
                  <div class="elementor-widget-theme-post-content">
                    <div class="elementor-widget-container">
                      <p>Najbardziej wiosenna zupa jarzynowa, jaka mozna sobie tylko wymarzyc.</p>
                      <p class="has-text-align-center has-background" style="background-color:#f3f3f3">
                        <strong>Wiosenna zupa jarzynowa ze szparagami i pistou</strong><br>
                        <em>4-6 porcji</em><br>
                        1 cebula, obrana i poszatkowana<br>
                        4 zabki czosnku<br>
                        200 g groszku mrozonego<br>
                        1 lyzka soku z cytryny<br>
                        sol morska drobnoziarnista, swiezo mielony pieprz
                      </p>
                      <p>1. Przygotuj zupe: W duzym garnku rozgrzej oliwe i zeszklij cebule.</p>
                      <p>2. Dodaj czosnek, wode i pozostale skladniki bazy, a potem zagotuj.</p>
                      <p>3. Dodaj groszek i gotuj, az lekko zmieknie.</p>
                      <p>4. Pokroj rzodkiewki i odstaw je do lodowatej wody.</p>
                      <p>5. Przygotuj pistou z ziol, parmezanu, cytryny i oliwy.</p>
                      <p>6. Podawaj zupe z pistou i dodatkami.</p>
                      <p class="has-background" style="background-color:#f3f3f3">Rady/porady<br>Poza sezonem szparagi mozna zastapic fasolka szparagowa.</p>
                    </div>
                  </div>
                </article>
              </body>
            </html>
            """;

    private static string CreateRealRozkosznyFixture2025() =>
        """
            <html>
              <body>
                <article class="elementor-location-single">
                  <h1>Rabarbarowy placek z budyniem i kruszonka</h1>
                  <div class="elementor-widget-theme-post-content">
                    <div class="elementor-widget-container">
                      <p class="has-text-align-center has-background" style="background-color:#f3f3f3">
                        <strong>Rabarbarowy placek z budyniem i kruszonka</strong><br>
                        <em>tortownica 23 cm</em><br>
                        Kruszonka<br>
                        3/4 szklanki (100 g) maki pszennej<br>
                        1/4 szklanki (50 g) cukru bialego<br>
                        50 g masla
                      </p>
                      <p class="has-text-align-center has-background" style="background-color:#f3f3f3">
                        Budyn lux<br>
                        1 szklanka (250 ml) mleka<br>
                        2 zoltka z duzych jajek<br>
                        3 lyzki cukru
                      </p>
                      <p class="has-text-align-center has-background" style="background-color:#f3f3f3">
                        Ciasto<br>
                        120 g masla, w temperaturze pokojowej<br>
                        1 1/2 szklanki (200 g) maki pszennej<br>
                        300 g rabarbaru
                      </p>
                      <p>1. Przygotuj kruszonke: Polacz make, cukier i sol, a potem dodaj maslo.</p>
                      <p>2. Przygotuj budyn lux: Podgrzej mleko, dodaj zoltka i gotuj do zgestnienia.</p>
                      <p>3. Przygotuj ciasto rabarbarowe: Rozgrzej piekarnik do 180 C i przygotuj forme.</p>
                      <p>4. Wymieszaj rabarbar ze skorka pomaranczowa i cukrem.</p>
                      <p>5. Przeloz mase do tortownicy, dodaj budyn, rabarbar i kruszonke, a potem piecz.</p>
                      <p>6. Swietnie smakuje na cieplo i na zimno.</p>
                      <p class="has-background" style="background-color:#f3f3f3">Rady/porady<br>Rabarbar mozna zastapic truskawkami.</p>
                    </div>
                  </div>
                </article>
              </body>
            </html>
            """;

    private static string CreateRealRozkosznyFixture2019() =>
        """
            <html>
              <body>
                <article class="elementor-location-single">
                  <h1>Piklowany rabarbar</h1>
                  <div class="elementor-widget-theme-post-content">
                    <div class="elementor-widget-container">
                      <p style="text-align: center;">
                        <strong>Piklowany rabarbar</strong><br>
                        <em>2 male sloiki</em><br>
                        3 lodygi rabarbaru<br>
                        1/2 szklanki octu z czerwonego wina<br>
                        1/4 szklanki cukru<br>
                        1/2 szklanki wody<br>
                        1/2 lyzeczki soli<br>
                        2 galazki rozmarynu
                      </p>
                      <p>1. Rabarbar pokroj w plasterki i umiesc go w sloiczkach wraz z rozmarynem.</p>
                      <p>2. Zagotuj ocet, cukier, wode i sol, a potem zalej rabarbar i odstaw do wystygniecia.</p>
                      <p><strong>Rady/porady</strong><br>1. Piklowany rabarbar mozna przechowywac do 2 tygodni w lodowce.</p>
                    </div>
                  </div>
                </article>
              </body>
            </html>
            """;

    private static string CreateRealJadlonomiaFixture2026() =>
        """
            <html>
              <body>
                <article>
                  <h1 class="h1">Proste ciasto z truskawkami</h1>
                  <a id="PrintIcon"></a>
                  <h2 itemprop="name">Proste ciasto z truskawkami</h2>
                  <ul class="list-unstyled font-semi-bold">
                    <li>Czas przygotowania: 20 minut</li>
                  </ul>
                  <div class="font-bold subtitle">Skladniki na tortownice 24 cm:</div>
                  <p>
                    2 szklanki maki pszennej<br />
                    3/4 szklanki cukru<br />
                    500 g truskawek
                  </p>
                  <div class="font-bold subtitle big-margin-top">Przygotowanie:</div>
                  <div class="hyphenate">
                    <ol>
                      <li>Wymieszaj make z cukrem i proszkiem do pieczenia.</li>
                      <li>Dodaj mokre skladniki i przelej ciasto do formy.</li>
                      <li>Na wierzchu uloz truskawki i piecz do suchego patyczka.</li>
                    </ol>
                    <p>Porady:</p>
                    <p>Najlepiej smakuje jeszcze lekko cieple.</p>
                  </div>
                </article>
              </body>
            </html>
            """;

    private static string CreateRealJadlonomiaFixture2016() =>
        """
            <html>
              <body>
                <article>
                  <h1 class="h1">Mistrzowska sałatka z bobu i awokado</h1>
                  <div class="recipe-card">
                    <a id="PrintIcon"></a>
                    <h2 itemprop="name">Mistrzowska sałatka z bobu i awokado</h2>
                    <ul class="list-unstyled font-semi-bold">
                      <li>Czas przygotowania: do 30 minut</li>
                    </ul>
                    <div class="font-bold subtitle">Składniki na 2 - 4 porcje:</div>
                    <p>500 g bobu</p>
                    <p>
                      1/4 czerwonej cebuli<br />
                      5 suszonych pomidorów<br />
                      5 pomidorków koktajlowych<br />
                      1/2 dojrzałego awokado<br />
                      1 - 2 łyżki soku z cytryny<br />
                      do podania: pietruszka
                    </p>
                    <div class="font-bold subtitle big-margin-top">Przygotowanie:</div>
                    <div class="hyphenate">
                      <ol>
                        <li>W garnku zagotować delikatnie osoloną wodę i ugotować bób do miękkości.</li>
                        <li>W międzyczasie posiekać cebulę, pomidory i awokado skropić sokiem z cytryny.</li>
                        <li>Ugotowany bób odcedzić, przygotować dressing i posiekać pietruszkę.</li>
                        <li>Wszystko delikatnie wymieszać i podać z dużą ilością pietruszki.</li>
                      </ol>
                    </div>
                  </div>
                </article>
              </body>
            </html>
            """;

    private static string CreateRealJadlonomiaFixture2021() =>
        """
            <html>
              <body>
                <article>
                  <h1 class="h1">Jogurtowy sernik</h1>
                  <a id="PrintIcon"></a>
                  <h2 itemprop="name">Jogurtowy sernik</h2>
                  <div class="font-bold subtitle">Skladniki na srednia tortownice 26 - 28 cm:</div>
                  <p>
                    <strong>Na mase:</strong><br />
                    800 g jogurtu kokosowego<br />
                    1/2 szklanki cukru<br />
                    3 lyzki skrobi ziemniaczanej
                  </p>
                  <div class="font-bold subtitle big-margin-top">Przygotowanie:</div>
                  <div class="hyphenate">
                    <ol>
                      <li>Wszystkie skladniki zmiksuj tylko do polaczenia.</li>
                      <li>Przelej mase do formy i wyrownaj wierzch.</li>
                      <li>Piecz do chwili, az srodek pozostanie lekko drgajacy.</li>
                    </ol>
                  </div>
                </article>
              </body>
            </html>
            """;

    private static string CreateRealJadlonomiaFixture2014() =>
        """
            <html>
              <body>
                <article>
                  <h1 class="h1">Granola jablkowa</h1>
                  <a id="PrintIcon"></a>
                  <h2 itemprop="name">Granola jablkowa</h2>
                  <div class="font-bold subtitle">Skladniki na duzy sloik:</div>
                  <p>
                    1 szklanka platkow owsianych<br />
                    1/2 szklanki pestek dyni<br />
                    2 jablka
                  </p>
                  <div class="font-bold subtitle big-margin-top">Przygotowanie:</div>
                  <div class="hyphenate">
                    <ol>
                      <li>Polacz suche skladniki z tartym jablkiem.</li>
                      <li>Piecz granole, mieszajac kilka razy podczas pieczenia.</li>
                    </ol>
                  </div>
                </article>
              </body>
            </html>
            """;

    private static string CreateRealMojeWypiekiFixture2026() =>
        """
            <html>
              <body>
                <article>
                  <h1>Pistacjowa panna cotta</h1>
                  <div class="article__content">
                    <blockquote>Delikatny i kremowy deser pistacjowy.</blockquote>
                    <p><span>Składniki na 4 porcje.</span></p>
                    <p><u>Składniki na pistacjową panna cottę:</u></p>
                    <ul>
                      <li>300 ml śmietanki kremówki 30%</li>
                      <li>200 ml mleka</li>
                      <li>80 g pasty pistacjowej</li>
                    </ul>
                    <p>Żelatynę zalej wodą i odstaw do napęcznienia.</p>
                    <p>Śmietankę podgrzej z mlekiem i pastą pistacjową, a potem dodaj żelatynę.</p>
                    <p>Rozlej deser do pucharków i schładzaj do stężenia.</p>
                    <p><u>Ponadto:</u></p>
                    <ul>
                      <li>2 łyżki pistacji do dekoracji</li>
                    </ul>
                    <p>Smacznego!</p>
                  </div>
                </article>
              </body>
            </html>
            """;

    private static string CreateRealMojeWypiekiFixture2024() =>
        """
            <html>
              <body>
                <article>
                  <h1>Brookie ciasteczkowe brownie</h1>
                  <div class="article__content">
                    <p><u>Składniki na masę ciasteczkową:</u></p>
                    <ul>
                      <li>180 g mąki pszennej</li>
                      <li>120 g masła</li>
                      <li>80 g cukru</li>
                    </ul>
                    <p><u>Składniki na masę brownie:</u></p>
                    <ul>
                      <li>200 g gorzkiej czekolady</li>
                      <li>150 g masła</li>
                      <li>3 jajka</li>
                    </ul>
                    <p>Przygotuj masę ciasteczkową i odstaw ją do schłodzenia.</p>
                    <p>Masę brownie przelej do formy, a na wierzchu rozłóż ciasto ciasteczkowe.</p>
                    <p>Piecz do chwili, aż środek pozostanie lekko wilgotny.</p>
                    <p>Smacznego!</p>
                  </div>
                </article>
              </body>
            </html>
            """;

    private static string CreateRealMojeWypiekiFixture2019() =>
        """
            <html>
              <body>
                <article>
                  <h1>Sernik z rosa</h1>
                  <div class="article__content">
                    <h2><u>Składniki na spód:</u></h2>
                    <ul>
                      <li>125 g masła</li>
                      <li>250 g mąki pszennej</li>
                      <li>1 jajko</li>
                    </ul>
                    <h2><u>Składniki na masę serową:</u></h2>
                    <ul>
                      <li>1 kg twarogu sernikowego</li>
                      <li>200 g cukru</li>
                      <li>4 żółtka</li>
                    </ul>
                    <p>Zagnieć składniki na spód i wylep nimi formę.</p>
                    <p>Masę serową przelej na podpieczony spód.</p>
                    <p>Piecz sernik, a następnie wyłóż na niego pianę i dopiecz.</p>
                    <p>Smacznego!</p>
                  </div>
                </article>
              </body>
            </html>
            """;

    private static async Task<RecipeApiContracts.RecipeDetailsDto> CreateRecipe(
        HttpClient client,
        string title
    )
    {
        var response = await client.PostAsJsonAsync(
            "/api/recipes",
            RecipeApiContracts.CreateRecipePayload(title)
        );
        response.EnsureSuccessStatusCode();
        var recipe = await response.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeDetailsDto>(
            JsonOptions
        );
        return recipe!;
    }
}

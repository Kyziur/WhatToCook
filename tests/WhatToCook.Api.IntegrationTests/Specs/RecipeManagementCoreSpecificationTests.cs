using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SkiaSharp;

namespace WhatToCook.Api.IntegrationTests.Specs;

public sealed class RecipeManagementCoreSpecificationTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Theory]
    [InlineData(200, true)]
    [InlineData(201, false)]
    public async Task CreateRecipe_ShouldValidateUnicodeScalarLimitsAtDatabaseBoundary(
        int length,
        bool accepted
    )
    {
        using var client = factory.CreateClient();
        var uniqueSuffix = Guid.NewGuid().ToString("N");
        var title = string.Concat(Enumerable.Repeat("😀", length - uniqueSuffix.Length))
            + uniqueSuffix;
        var payload = RecipeApiContracts.CreateRecipePayload(title);
        payload.Ingredients =
        [
            new RecipeApiContracts.RecipeIngredientPayload(
                string.Concat(Enumerable.Repeat("😀", length - uniqueSuffix.Length)) + uniqueSuffix,
                "1",
                "szt"
            ),
        ];

        using var response = await client.PostAsJsonAsync("/api/recipes", payload);
        Assert.Equal(accepted ? HttpStatusCode.Created : HttpStatusCode.BadRequest, response.StatusCode);

        if (accepted)
        {
            var created =
                await response.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeDetailsDto>(
                    JsonOptions
                );
            Assert.NotNull(created);
            Assert.Equal(title, created.Title);
            Assert.Single(created.Ingredients);
            Assert.Equal(payload.Ingredients[0].Name, created.Ingredients[0].Ingredient);
        }
    }

    [Fact]
    public async Task SaveNewRecipeWithCompleteCoreFields_ShouldPersistRecipe()
    {
        using var client = factory.CreateClient();
        var payload = RecipeApiContracts.CreateRecipePayload("Zupa Pomidorowa");

        var createResponse = await client.PostAsJsonAsync("/api/recipes", payload);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created =
            await createResponse.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeDetailsDto>(
                JsonOptions
            );
        Assert.NotNull(created);

        var getResponse = await client.GetAsync($"/api/recipes/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var recipe =
            await getResponse.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeDetailsDto>(
                JsonOptions
            );
        Assert.NotNull(recipe);
        Assert.Equal("Zupa Pomidorowa", recipe!.Title);
        Assert.Equal(4, recipe.Servings);
        Assert.True(recipe.IsActive);
    }

    [Fact]
    public async Task EditExistingRecipe_ShouldPersistLatestVersion()
    {
        using var client = factory.CreateClient();
        var created = await CreateRecipe(client, "Mizeria");

        var updatePayload = RecipeApiContracts.CreateRecipePayload("Mizeria klasyczna");
        updatePayload.Servings = 2;
        updatePayload.Steps = ["Pokroj ogorki", "Dodaj smietane", "Dopraw i podaj"];

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/recipes/{created.Id}",
            updatePayload
        );
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated =
            await updateResponse.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeDetailsDto>(
                JsonOptions
            );
        Assert.NotNull(updated);
        Assert.Equal("Mizeria klasyczna", updated!.Title);
        Assert.Equal(2, updated.Servings);
        Assert.Equal(["Pokroj ogorki", "Dodaj smietane", "Dopraw i podaj"], updated.Steps);
    }

    [Fact]
    public async Task SaveRecipe_ShouldRejectDuplicateTitleAfterNormalization()
    {
        using var client = factory.CreateClient();

        var first = await client.PostAsJsonAsync(
            "/api/recipes",
            RecipeApiContracts.CreateRecipePayload("Krem z Brokolow")
        );
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var duplicate = await client.PostAsJsonAsync(
            "/api/recipes",
            RecipeApiContracts.CreateRecipePayload("  kReM Z Brokolow ")
        );
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveRecipeWithNormalizedNameCollisions_ShouldPreserveIngredientQuantities(
        bool update
    )
    {
        using var client = factory.CreateClient();
        var payload = RecipeApiContracts.CreateRecipePayload(
            $"Normalized collision {Guid.NewGuid():N}"
        );
        payload.Ingredients =
        [
            new RecipeApiContracts.RecipeIngredientPayload("Łosoś", "1", "kg"),
            new RecipeApiContracts.RecipeIngredientPayload("Losos", "2", "kg"),
        ];
        payload.Tags = ["Łosoś", "Losos"];

        RecipeApiContracts.RecipeDetailsDto saved;
        if (update)
        {
            var original = await CreateRecipe(client, $"Before normalization {Guid.NewGuid():N}");
            var response = await client.PutAsJsonAsync($"/api/recipes/{original.Id}", payload);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            saved = (
                await response.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeDetailsDto>(
                    JsonOptions
                )
            )!;
        }
        else
        {
            saved = await CreateRecipe(client, payload);
        }

        Assert.Equal(2, saved.Ingredients.Count);
        Assert.Contains(saved.Ingredients, x => x.QuantityText == "1" && x.Unit == "kg");
        Assert.Contains(saved.Ingredients, x => x.QuantityText == "2" && x.Unit == "kg");
        Assert.Single(saved.Ingredients.Select(x => x.Ingredient).Distinct());

        var library = await client.GetFromJsonAsync<List<RecipeApiContracts.RecipeSummaryDto>>(
            "/api/recipes",
            JsonOptions
        );
        Assert.NotNull(library);
        var recipe = Assert.Single(library, x => x.Id == saved.Id);
        Assert.Single(recipe.Tags);
    }

    [Fact]
    public async Task ArchiveRecipe_ShouldRemoveItFromLibraryResults()
    {
        using var client = factory.CreateClient();
        var created = await CreateRecipe(client, "Archiwizowany przepis");

        var archiveResponse = await client.PostAsync(
            $"/api/recipes/{created.Id}/archive",
            content: null
        );
        Assert.Equal(HttpStatusCode.NoContent, archiveResponse.StatusCode);

        var libraryResponse = await client.GetAsync("/api/recipes");
        libraryResponse.EnsureSuccessStatusCode();
        var library = await libraryResponse.Content.ReadFromJsonAsync<
            List<RecipeApiContracts.RecipeSummaryDto>
        >(JsonOptions);
        Assert.NotNull(library);
        Assert.DoesNotContain(library!, x => x.Id == created.Id);
    }

    [Fact]
    public async Task LibrarySummary_ShouldIncludeDisplayNamesAndIngredientFiltersData()
    {
        using var client = factory.CreateClient();
        var payload = RecipeApiContracts.CreateRecipePayload("Sałatka testowa");
        payload.Ingredients =
        [
            new RecipeApiContracts.RecipeIngredientPayload("Ogórek", "1", "szt"),
            new RecipeApiContracts.RecipeIngredientPayload("Feta", "200", "g"),
        ];
        payload.Tags = ["szybkie", "kolacja"];

        var created = await CreateRecipe(client, payload);

        var response = await client.GetAsync("/api/recipes");
        response.EnsureSuccessStatusCode();

        var library = await response.Content.ReadFromJsonAsync<
            List<RecipeApiContracts.RecipeSummaryDto>
        >(JsonOptions);

        Assert.NotNull(library);
        var createdRecipe = Assert.Single(library!, x => x.Id == created.Id);
        Assert.True(createdRecipe.IsActive);
        Assert.Contains("szybkie", createdRecipe.Tags);
        Assert.Contains("Kolacja", createdRecipe.Tags);
        Assert.Contains("Ogórek", createdRecipe.Ingredients);
        Assert.Contains("Feta", createdRecipe.Ingredients);
    }

    [Fact]
    public async Task SaveRecipeWithCombinedIngredientText_ShouldSplitQuantityUnitAndName()
    {
        using var client = factory.CreateClient();
        var payload = RecipeApiContracts.CreateRecipePayload("Owsianka z napojem owsianym");
        payload.Ingredients =
        [
            new RecipeApiContracts.RecipeIngredientPayload("120g napoju owsianego", null, null),
        ];

        var created = await CreateRecipe(client, payload);

        var ingredient = Assert.Single(created.Ingredients);
        Assert.Equal("napoju owsianego", ingredient.Ingredient);
        Assert.Equal("120", ingredient.QuantityText);
        Assert.Equal("g", ingredient.Unit);
    }

    [Fact]
    public async Task SaveRecipeWithUploadedPhoto_ShouldStoreMainPhotoPath()
    {
        using var client = factory.CreateClient();
        var created = await CreateRecipe(client, "Zupa z fotografia");

        var multipart = new MultipartFormDataContent();
        multipart.Add(
            new ByteArrayContent(
                Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///ywAAAAAAQABAAACAUwAOw==")
            ),
            "file",
            "photo.gif"
        );
        var uploadResponse = await client.PostAsync($"/api/recipes/{created.Id}/photo", multipart);
        Assert.Equal(HttpStatusCode.OK, uploadResponse.StatusCode);

        var uploadJson = await uploadResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var photoPath = uploadJson.GetProperty("mainPhotoPath").GetString();
        Assert.False(string.IsNullOrWhiteSpace(photoPath));
        Assert.True(File.Exists(Path.Combine(factory.StorageRootPath, photoPath!)));
    }

    [Theory]
    [InlineData(SKEncodedImageFormat.Png, ".png")]
    [InlineData(SKEncodedImageFormat.Jpeg, ".jpg")]
    [InlineData(SKEncodedImageFormat.Webp, ".webp")]
    public async Task UploadingSupportedRasterFormats_ShouldPersistImage(
        SKEncodedImageFormat format,
        string extension
    )
    {
        using var bitmap = new SKBitmap(2, 2);
        bitmap.Erase(SKColors.Red);
        using var image = SKImage.FromBitmap(bitmap);
        using var encoded = image.Encode(format, 90);
        Assert.NotNull(encoded);

        using var client = factory.CreateClient();
        var created = await CreateRecipe(client, $"Upload {format} {Guid.NewGuid():N}");

        using var multipart = new MultipartFormDataContent();
        multipart.Add(new ByteArrayContent(encoded.ToArray()), "file", $"photo{extension}");

        var uploadResponse = await client.PostAsync($"/api/recipes/{created.Id}/photo", multipart);
        Assert.Equal(HttpStatusCode.OK, uploadResponse.StatusCode);

        var uploadJson = await uploadResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var photoPath = uploadJson.GetProperty("mainPhotoPath").GetString();
        Assert.NotNull(photoPath);
        Assert.EndsWith(extension, photoPath, StringComparison.OrdinalIgnoreCase);
        Assert.True(File.Exists(Path.Combine(factory.StorageRootPath, photoPath)));
    }

    [Fact]
    public async Task UploadingNonImageBytes_ShouldBeRejected()
    {
        using var client = factory.CreateClient();
        var created = await CreateRecipe(client, "Przepis odrzucone zdjecie");

        using var multipart = new MultipartFormDataContent();
        multipart.Add(new ByteArrayContent([1, 2, 3, 4]), "file", "photo.jpg");

        var response = await client.PostAsync($"/api/recipes/{created.Id}/photo", multipart);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadingAnInvalidImageHeader_ShouldBeRejected()
    {
        using var client = factory.CreateClient();
        var created = await CreateRecipe(client, "Przepis z uszkodzonym zdjeciem");

        using var multipart = new MultipartFormDataContent();
        multipart.Add(new ByteArrayContent([0xff, 0xd8, 0xff, 0xd9]), "file", "photo.jpg");

        var response = await client.PostAsync($"/api/recipes/{created.Id}/photo", multipart);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SearchByTitleAndIngredient_ShouldUseNormalizedMatching()
    {
        using var client = factory.CreateClient();
        var payload = RecipeApiContracts.CreateRecipePayload("Tosty z Serem");
        payload.Ingredients =
        [
            new RecipeApiContracts.RecipeIngredientPayload("Zolty Ser", "200", "g"),
            new RecipeApiContracts.RecipeIngredientPayload("Chleb", "4", "szt"),
        ];

        var created = await CreateRecipe(client, payload);

        var byTitleResponse = await client.GetAsync("/api/recipes/search?title=tosty");
        Assert.Equal(HttpStatusCode.OK, byTitleResponse.StatusCode);
        var byTitle = await byTitleResponse.Content.ReadFromJsonAsync<
            List<RecipeApiContracts.RecipeSummaryDto>
        >(JsonOptions);
        Assert.NotNull(byTitle);
        Assert.Contains(byTitle!, x => x.Id == created.Id);

        var byIngredientResponse = await client.GetAsync(
            "/api/recipes/search?ingredient=zolty%20ser"
        );
        Assert.Equal(HttpStatusCode.OK, byIngredientResponse.StatusCode);
        var byIngredient = await byIngredientResponse.Content.ReadFromJsonAsync<
            List<RecipeApiContracts.RecipeSummaryDto>
        >(JsonOptions);
        Assert.NotNull(byIngredient);
        Assert.Contains(byIngredient!, x => x.Id == created.Id);
    }

    [Fact]
    public async Task SaveRecipeWithoutPhoto_ShouldKeepMainPhotoPathEmpty()
    {
        using var client = factory.CreateClient();
        var payload = RecipeApiContracts.CreateRecipePayload("Zupa bez zdjecia");
        var created = await CreateRecipe(client, payload);
        Assert.Null(created.MainPhotoPath);
    }

    [Fact]
    public async Task SaveRecipeWithPlainTextSource_ShouldReturnSourceAsText()
    {
        using var client = factory.CreateClient();
        var payload = RecipeApiContracts.CreateRecipePayload("Zrodlo tekstowe");
        payload.Source = "Przepis od babci";

        var created = await CreateRecipe(client, payload);
        Assert.Equal("Przepis od babci", created.Source);
    }

    [Fact]
    public async Task SaveIngredientWithoutQuantity_ShouldBeAccepted()
    {
        using var client = factory.CreateClient();
        var payload = RecipeApiContracts.CreateRecipePayload("Bez ilosci");
        payload.Ingredients =
        [
            new RecipeApiContracts.RecipeIngredientPayload("sol", null, null),
            new RecipeApiContracts.RecipeIngredientPayload("ziemniaki", "1", "kg"),
        ];

        var created = await CreateRecipe(client, payload);
        Assert.Contains(created.Ingredients, x => x.Ingredient == "sol" && x.QuantityText is null);
    }

    [Fact]
    public async Task AddIngredientFromExistingSuggestion_ShouldReturnNormalizedSuggestions()
    {
        using var client = factory.CreateClient();
        var payload = RecipeApiContracts.CreateRecipePayload("Tagliatelle z serem");
        payload.Ingredients =
        [
            new RecipeApiContracts.RecipeIngredientPayload("Zolty Ser", "200", "g"),
            new RecipeApiContracts.RecipeIngredientPayload("Makaron", "250", "g"),
        ];

        await CreateRecipe(client, payload);

        var response = await client.GetAsync(
            "/api/recipes/suggestions/ingredients?q=%C5%BC%C3%B3%C5%82"
        );
        response.EnsureSuccessStatusCode();
        var suggestions = await response.Content.ReadFromJsonAsync<List<string>>(JsonOptions);

        Assert.NotNull(suggestions);
        Assert.Contains("Zolty Ser", suggestions!);
    }

    [Fact]
    public async Task SuggestExistingTagDuringEditing_ShouldReturnNormalizedSuggestions()
    {
        using var client = factory.CreateClient();
        var payload = RecipeApiContracts.CreateRecipePayload("Kanapki weekendowe");
        payload.Tags = ["Szybkie", "Kolacja"];

        await CreateRecipe(client, payload);

        var response = await client.GetAsync("/api/recipes/suggestions/tags?q=szY");
        response.EnsureSuccessStatusCode();
        var suggestions = await response.Content.ReadFromJsonAsync<List<string>>(JsonOptions);

        Assert.NotNull(suggestions);
        Assert.Contains(
            suggestions!,
            x => string.Equals(x, "Szybkie", StringComparison.OrdinalIgnoreCase)
        );
    }

    [Theory]
    [InlineData("ingredients", "null")]
    [InlineData("ingredients", "[null]")]
    [InlineData("steps", "null")]
    [InlineData("steps", "[null]")]
    [InlineData("tags", "null")]
    [InlineData("tags", "[null]")]
    public async Task SaveRecipe_ShouldRejectExplicitNullCollectionsAndElements(
        string field,
        string invalidValue
    )
    {
        using var client = factory.CreateClient();
        var initial = await CreateRecipe(client, $"Before null validation {Guid.NewGuid():N}");
        var ingredients =
            field == "ingredients"
                ? invalidValue
                : """[{"name":"Pomidor","quantityText":"2","unit":"szt"}]""";
        var steps = field == "steps" ? invalidValue : """["Wymieszaj składniki"]""";
        var tags = field == "tags" ? invalidValue : """["obiad"]""";
        var json = $$"""
            {
                "title": "Updated recipe",
                "servings": 4,
                "ingredients": {{ingredients}},
                "steps": {{steps}},
                "tags": {{tags}}
            }
            """;

        using var create = await client.PostAsync(
            "/api/recipes",
            new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        );
        Assert.Equal(HttpStatusCode.BadRequest, create.StatusCode);

        using var update = await client.PutAsync(
            $"/api/recipes/{initial.Id}",
            new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        );
        Assert.Equal(HttpStatusCode.BadRequest, update.StatusCode);

        var persisted = await client.GetFromJsonAsync<RecipeApiContracts.RecipeDetailsDto>(
            $"/api/recipes/{initial.Id}",
            JsonOptions
        );
        Assert.NotNull(persisted);
        Assert.Equal(initial.Title, persisted.Title);
        Assert.Equal(initial.Ingredients.Count, persisted.Ingredients.Count);
    }

    [Fact]
    public async Task CreateRecipeWithOmittedOptionalTags_ShouldUseEmptyTags()
    {
        using var client = factory.CreateClient();
        var title = $"Omitted tags {Guid.NewGuid():N}";
        var json = $$"""
            {
                "title": "{{title}}",
                "servings": 4,
                "ingredients": [{"name":"Pomidor"}],
                "steps": ["Gotuj"]
            }
            """;
        using var response = await client.PostAsync(
            "/api/recipes",
            new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task SaveRecipeWithMissingRequiredFields_ShouldReturnValidationProblem()
    {
        using var client = factory.CreateClient();
        var payload = RecipeApiContracts.CreateRecipePayload(string.Empty);
        payload.Servings = 0;
        payload.Ingredients = [];
        payload.Steps = [];

        var response = await client.PostAsJsonAsync("/api/recipes", payload);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static async Task<RecipeApiContracts.RecipeDetailsDto> CreateRecipe(
        HttpClient client,
        string title
    )
    {
        return await CreateRecipe(client, RecipeApiContracts.CreateRecipePayload(title));
    }

    private static async Task<RecipeApiContracts.RecipeDetailsDto> CreateRecipe(
        HttpClient client,
        RecipeApiContracts.RecipeUpsertPayload payload
    )
    {
        var response = await client.PostAsJsonAsync("/api/recipes", payload);
        response.EnsureSuccessStatusCode();
        var recipe = await response.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeDetailsDto>(
            JsonOptions
        );
        return recipe!;
    }
}

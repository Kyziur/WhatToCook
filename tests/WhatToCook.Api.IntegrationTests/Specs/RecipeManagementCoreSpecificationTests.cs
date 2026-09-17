using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace WhatToCook.Api.IntegrationTests.Specs;

public sealed class RecipeManagementCoreSpecificationTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

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

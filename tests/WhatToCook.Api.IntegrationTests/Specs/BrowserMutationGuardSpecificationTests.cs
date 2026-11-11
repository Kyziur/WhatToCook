using System.Net;
using System.Net.Http.Json;

namespace WhatToCook.Api.IntegrationTests.Specs;

public sealed class BrowserMutationGuardSpecificationTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task CrossOriginBrowserRequest_ShouldNotCreateRecipe()
    {
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/recipes")
        {
            Content = JsonContent.Create(
                RecipeApiContracts.CreateRecipePayload($"Blocked {Guid.NewGuid():N}")
            ),
        };
        request.Headers.TryAddWithoutValidation("Origin", "https://untrusted.example");

        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CrossSiteFetchMetadata_ShouldBlockPhotoUpload()
    {
        using var client = factory.CreateClient();
        using var created = await client.PostAsJsonAsync(
            "/api/recipes",
            RecipeApiContracts.CreateRecipePayload($"CSRF upload {Guid.NewGuid():N}")
        );
        created.EnsureSuccessStatusCode();
        var recipe = await created.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeDetailsDto>();
        Assert.NotNull(recipe);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/recipes/{recipe!.Id}/photo"
        );
        request.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "cross-site");
        var form = new MultipartFormDataContent();
        form.Add(
            new ByteArrayContent(
                Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///ywAAAAAAQABAAACAUwAOw==")
            ),
            "file",
            "photo.gif"
        );
        request.Content = form;

        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var persisted = await client.GetFromJsonAsync<RecipeApiContracts.RecipeDetailsDto>(
            $"/api/recipes/{recipe.Id}"
        );
        Assert.NotNull(persisted);
        Assert.Null(persisted.MainPhotoPath);
    }

    [Fact]
    public async Task ServerToServerRecipeMutationWithoutBrowserHeaders_RemainsAllowed()
    {
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync(
            "/api/recipes",
            RecipeApiContracts.CreateRecipePayload($"Server request {Guid.NewGuid():N}")
        );
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}

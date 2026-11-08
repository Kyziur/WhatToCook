using System.Net;
using System.Text;
using System.Text.Json;
using WhatToCook.Web.Components.Features.Recipes.Shared;

namespace WhatToCook.Web.ComponentTests;

public sealed class RecipeCatalogHttpContractTests
{
    private static readonly CatalogQuery FirstPage = new(
        1,
        24,
        string.Empty,
        [],
        [],
        true,
        "TitleAscending"
    );

    [Fact]
    public async Task PagedCatalog_MissingTotalCount_ShouldFailInsteadOfShowingAnEmptyLibrary()
    {
        const string json = """
            {
              "items": [],
              "page": 1,
              "pageSize": 24,
              "tags": [],
              "ingredients": [],
              "tagUsage": {}
            }
            """;
        using var handler = new FixedJsonHandler(json);
        using var client = CreateClient(handler);
        var service = new ApiRecipeCatalogService(client);

        await Assert.ThrowsAsync<JsonException>(() => service.GetPageAsync(FirstPage));
    }

    [Fact]
    public async Task PagedCatalog_MissingRecipeTitle_ShouldFailInsteadOfDisplayingABlankCard()
    {
        const string json = """
            {
              "items": [
                {
                  "id": "11111111-1111-1111-1111-111111111111",
                  "servings": 4,
                  "isActive": true,
                  "tags": [],
                  "ingredients": []
                }
              ],
              "totalCount": 1,
              "page": 1,
              "pageSize": 24,
              "tags": [],
              "ingredients": [],
              "tagUsage": {}
            }
            """;
        using var handler = new FixedJsonHandler(json);
        using var client = CreateClient(handler);
        var service = new ApiRecipeCatalogService(client);

        await Assert.ThrowsAsync<JsonException>(() => service.GetPageAsync(FirstPage));
    }

    [Fact]
    public async Task PagedCatalog_WrongPageMetadata_ShouldFailInsteadOfRenderingWrongPage()
    {
        const string json = """
            {
              "items": [],
              "totalCount": 0,
              "page": 2,
              "pageSize": 24,
              "tags": [],
              "ingredients": [],
              "tagUsage": {}
            }
            """;
        using var handler = new FixedJsonHandler(json);
        using var client = CreateClient(handler);
        var service = new ApiRecipeCatalogService(client);

        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetPageAsync(FirstPage));
    }

    [Fact]
    public async Task RecipeDetails_MissingSteps_ShouldFailInsteadOfSilentlyHidingInstructions()
    {
        const string json = """
            {
              "id": "11111111-1111-1111-1111-111111111111",
              "title": "Test recipe",
              "servings": 4,
              "isActive": true,
              "tags": [],
              "ingredients": []
            }
            """;
        using var handler = new FixedJsonHandler(json);
        using var client = CreateClient(handler);
        var service = new ApiRecipeCatalogService(client);

        await Assert.ThrowsAsync<JsonException>(() => service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task PagedCatalog_PresentButNullItems_ShouldFailInsteadOfRenderingAnEmptyPage()
    {
        const string json = """
            {
              "items": null,
              "totalCount": 0,
              "page": 1,
              "pageSize": 24,
              "tags": [],
              "ingredients": [],
              "tagUsage": {}
            }
            """;
        using var handler = new FixedJsonHandler(json);
        using var client = CreateClient(handler);
        var service = new ApiRecipeCatalogService(client);

        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetPageAsync(FirstPage));
    }

    [Fact]
    public async Task RecipeDetails_PresentButNullSteps_ShouldFailInsteadOfHidingInstructions()
    {
        const string json = """
            {
              "id": "11111111-1111-1111-1111-111111111111",
              "title": "Test recipe",
              "servings": 4,
              "isActive": true,
              "tags": [],
              "ingredients": [],
              "steps": null
            }
            """;
        using var handler = new FixedJsonHandler(json);
        using var client = CreateClient(handler);
        var service = new ApiRecipeCatalogService(client);

        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task RecipeDetails_MissingNestedIngredientName_ShouldFailDeserialization()
    {
        const string json = """
            {
              "id": "11111111-1111-1111-1111-111111111111",
              "title": "Test recipe",
              "servings": 4,
              "isActive": true,
              "tags": [],
              "ingredients": [{ "quantityText": "2", "unit": "szt" }],
              "steps": ["Test step"]
            }
            """;
        using var handler = new FixedJsonHandler(json);
        using var client = CreateClient(handler);
        var service = new ApiRecipeCatalogService(client);

        await Assert.ThrowsAsync<JsonException>(() => service.GetByIdAsync(Guid.NewGuid()));
    }

    private static HttpClient CreateClient(HttpMessageHandler handler) =>
        new(handler, disposeHandler: false) { BaseAddress = new Uri("http://localhost") };

    private sealed class FixedJsonHandler(string json) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                }
            );
    }
}

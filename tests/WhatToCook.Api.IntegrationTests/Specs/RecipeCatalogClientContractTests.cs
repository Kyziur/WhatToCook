using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace WhatToCook.Api.IntegrationTests.Specs;

public sealed class RecipeCatalogClientContractTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task RecipeCatalogHttpResponses_ShouldPreserveRequiredWebConsumerFields()
    {
        using var client = factory.CreateClient();
        var title = $"Contract {Guid.NewGuid():N}";
        var payload = RecipeApiContracts.CreateRecipePayload(title);

        using var created = await client.PostAsJsonAsync("/api/recipes", payload);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var createdDocument = JsonDocument.Parse(
            await created.Content.ReadAsStringAsync()
        );
        var id = createdDocument.RootElement.GetProperty("id").GetGuid();

        using var pageResponse = await client.GetAsync(
            $"/api/recipes/page?q={Uri.EscapeDataString(title)}&page=1&pageSize=1"
        );
        Assert.Equal(HttpStatusCode.OK, pageResponse.StatusCode);
        using var pageDocument = JsonDocument.Parse(
            await pageResponse.Content.ReadAsStringAsync()
        );
        var page = pageDocument.RootElement;
        AssertProperty(page, "items", JsonValueKind.Array);
        AssertProperty(page, "totalCount", JsonValueKind.Number);
        AssertProperty(page, "page", JsonValueKind.Number);
        AssertProperty(page, "pageSize", JsonValueKind.Number);
        AssertProperty(page, "tags", JsonValueKind.Array);
        AssertProperty(page, "ingredients", JsonValueKind.Array);
        AssertProperty(page, "tagUsage", JsonValueKind.Object);
        Assert.Equal(1, page.GetProperty("page").GetInt32());
        Assert.Equal(1, page.GetProperty("pageSize").GetInt32());
        Assert.Equal(1, page.GetProperty("totalCount").GetInt32());

        var item = Assert.Single(page.GetProperty("items").EnumerateArray());
        AssertProperty(item, "id", JsonValueKind.String);
        AssertProperty(item, "title", JsonValueKind.String);
        AssertProperty(item, "servings", JsonValueKind.Number);
        AssertProperty(item, "isActive", JsonValueKind.True);
        AssertProperty(item, "tags", JsonValueKind.Array);
        AssertProperty(item, "ingredients", JsonValueKind.Array);
        Assert.Equal(id, item.GetProperty("id").GetGuid());
        Assert.Equal(title, item.GetProperty("title").GetString());

        using var detailsResponse = await client.GetAsync($"/api/recipes/{id}");
        Assert.Equal(HttpStatusCode.OK, detailsResponse.StatusCode);
        using var detailsDocument = JsonDocument.Parse(
            await detailsResponse.Content.ReadAsStringAsync()
        );
        var details = detailsDocument.RootElement;
        AssertProperty(details, "id", JsonValueKind.String);
        AssertProperty(details, "title", JsonValueKind.String);
        AssertProperty(details, "servings", JsonValueKind.Number);
        AssertProperty(details, "isActive", JsonValueKind.True);
        AssertProperty(details, "tags", JsonValueKind.Array);
        AssertProperty(details, "ingredients", JsonValueKind.Array);
        AssertProperty(details, "steps", JsonValueKind.Array);
        Assert.Equal(title, details.GetProperty("title").GetString());
        Assert.Equal(2, details.GetProperty("steps").GetArrayLength());
        var ingredient = Assert.Single(details.GetProperty("ingredients").EnumerateArray());
        AssertProperty(ingredient, "ingredient", JsonValueKind.String);
        Assert.Equal("Pomidor", ingredient.GetProperty("ingredient").GetString());
    }

    private static void AssertProperty(JsonElement source, string name, JsonValueKind kind)
    {
        Assert.True(source.TryGetProperty(name, out var value), $"Missing HTTP contract field: {name}");
        Assert.Equal(kind, value.ValueKind);
    }
}

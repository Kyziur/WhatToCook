using System.Net;
using System.Net.Http.Json;
using WhatToCook.Web.Components.Features.Recipes.Shared;

namespace WhatToCook.Web.ComponentTests;

public sealed class ApiRecipeCatalogServiceTests
{
    [Fact]
    public async Task PlannerCatalog_ShouldTraverseEveryPageBeyondFiveHundredWithoutFacets()
    {
        using var handler = new PagedCatalogHandler(551);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost"),
        };
        var service = new ApiRecipeCatalogService(httpClient);

        var recipes = await service.GetActiveRecipesAsync();

        Assert.Equal(551, recipes.Count);
        Assert.Equal("Przepis 0001", recipes[0].Title);
        Assert.Equal("Przepis 0551", recipes[^1].Title);
        Assert.Equal(12, handler.Requests.Count);
        Assert.Equal(Enumerable.Range(1, 12), handler.Requests.Select(x => int.Parse(x["page"])));
        Assert.All(
            handler.Requests,
            x =>
            {
                Assert.Equal("50", x["pageSize"]);
                Assert.Equal("false", x["includeFacets"]);
                Assert.Equal("TitleAscending", x["sort"]);
            }
        );
    }

    [Fact]
    public async Task PlannerCatalog_ShouldFailRatherThanSilentlyTruncateOnMissingPage()
    {
        using var handler = new PagedCatalogHandler(80, emptyPage: 2);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost"),
        };
        var service = new ApiRecipeCatalogService(httpClient);

        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetActiveRecipesAsync());
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task PlannerCatalog_ShouldFailOnShortNonemptyPageRatherThanHideRecipes()
    {
        using var handler = new PagedCatalogHandler(80, shortPage: 1);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost"),
        };
        var service = new ApiRecipeCatalogService(httpClient);

        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetActiveRecipesAsync());
        Assert.Equal(2, handler.Requests.Count);
    }

    private sealed class PagedCatalogHandler(
        int totalCount,
        int? emptyPage = null,
        int? shortPage = null
    ) : HttpMessageHandler
    {
        public List<Dictionary<string, string>> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Assert.Equal("/api/recipes/page", request.RequestUri?.AbsolutePath);
            var fields = request
                .RequestUri!.Query.TrimStart('?')
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Split('=', 2))
                .ToDictionary(
                    pair => pair[0],
                    pair => Uri.UnescapeDataString(pair[1]),
                    StringComparer.OrdinalIgnoreCase
                );
            Requests.Add(fields);
            var page = int.Parse(fields["page"]);
            var pageSize = int.Parse(fields["pageSize"]);
            var start = (page - 1) * pageSize + 1;
            var count =
                page == emptyPage ? 0 : Math.Max(0, Math.Min(pageSize, totalCount - start + 1));
            if (page == shortPage)
            {
                count = Math.Min(count, 20);
            }
            var items = Enumerable
                .Range(start, count)
                .Select(index => new
                {
                    id = Guid.NewGuid(),
                    title = $"Przepis {index:0000}",
                    servings = 4,
                    source = (string?)null,
                    mainPhotoPath = (string?)null,
                    isActive = true,
                    tags = Array.Empty<string>(),
                    ingredients = Array.Empty<string>(),
                })
                .ToList();

            return Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(
                        new
                        {
                            items,
                            totalCount,
                            page,
                            pageSize,
                            tags = Array.Empty<string>(),
                            ingredients = Array.Empty<string>(),
                            tagUsage = new Dictionary<string, int>(),
                        }
                    ),
                }
            );
        }
    }
}

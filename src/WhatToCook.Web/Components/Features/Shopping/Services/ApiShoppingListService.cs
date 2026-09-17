using System.Net.Http.Json;
using System.Text.Json;

namespace WhatToCook.Web.Components.Features.Shopping.Services;

public sealed class ApiShoppingListService(HttpClient httpClient) : IShoppingListService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<ShoppingListDetails?> GetForPlanAsync(
        Guid planId,
        CancellationToken cancellationToken = default
    )
    {
        var response = await httpClient.GetAsync(
            $"/api/shopping-lists/plan/{planId}",
            cancellationToken
        );
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<ShoppingListDetailsDto>(
            JsonOptions,
            cancellationToken
        );
        return payload is null ? null : Map(payload);
    }

    public async Task<IReadOnlyList<RecipeRecommendation>?> GetRecipeRecommendationsAsync(
        Guid planId,
        CancellationToken cancellationToken = default
    )
    {
        var response = await httpClient.GetAsync(
            $"/api/shopping-lists/plan/{planId}/recommendations",
            cancellationToken
        );
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<RecipeRecommendationsDto>(
            JsonOptions,
            cancellationToken
        );
        return payload
            ?.Items.Select(x => new RecipeRecommendation(
                x.RecipeId,
                x.Title,
                x.MainPhotoPath,
                x.MatchedIngredientCount,
                x.TotalIngredientCount,
                x.MatchedIngredients
            ))
            .ToList();
    }

    public async Task<ShoppingListDetails?> SetItemStateAsync(
        Guid planId,
        Guid itemId,
        ShoppingChecklistState state,
        CancellationToken cancellationToken = default
    )
    {
        var stateText = state == ShoppingChecklistState.Mam ? "MAM" : "NIE_MAM";
        var response = await httpClient.PutAsJsonAsync(
            $"/api/shopping-lists/plan/{planId}/items/{itemId}/state",
            new SetStateRequestDto(stateText),
            cancellationToken
        );
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<ShoppingListDetailsDto>(
            JsonOptions,
            cancellationToken
        );
        return payload is null ? null : Map(payload);
    }

    public async Task<ShoppingListDetails?> AddManualItemAsync(
        Guid planId,
        string text,
        CancellationToken cancellationToken = default
    )
    {
        var response = await httpClient.PostAsJsonAsync(
            $"/api/shopping-lists/plan/{planId}/manual-items",
            new AddManualItemRequestDto(text),
            cancellationToken
        );
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<ShoppingListDetailsDto>(
            JsonOptions,
            cancellationToken
        );
        return payload is null ? null : Map(payload);
    }

    public async Task<string?> GetCopyTextAsync(
        Guid planId,
        CancellationToken cancellationToken = default
    )
    {
        var response = await httpClient.GetAsync(
            $"/api/shopping-lists/plan/{planId}/copy-text",
            cancellationToken
        );
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<CopyTextResponseDto>(
            JsonOptions,
            cancellationToken
        );
        return payload?.Text;
    }

    private static ShoppingListDetails Map(ShoppingListDetailsDto source) =>
        new(
            source.Id,
            source.MealPlanId,
            source.MealPlanName,
            source.UpdatedAt,
            source
                .Items.OrderBy(x => x.SortOrder)
                .Select(x => new ShoppingListItem(
                    x.Id,
                    x.Name,
                    x.QuantityText,
                    x.Unit,
                    ParseState(x.State),
                    x.IsManual,
                    x.SortOrder,
                    x.DisplayText
                ))
                .ToList()
        );

    private static ShoppingChecklistState ParseState(string value) =>
        string.Equals(value, "MAM", StringComparison.OrdinalIgnoreCase)
            ? ShoppingChecklistState.Mam
            : ShoppingChecklistState.NieMam;

    private sealed class ShoppingListDetailsDto
    {
        public Guid Id { get; init; }
        public Guid MealPlanId { get; init; }
        public string MealPlanName { get; init; } = string.Empty;
        public DateTimeOffset UpdatedAt { get; init; }
        public List<ShoppingListItemDto> Items { get; init; } = [];
    }

    private sealed class ShoppingListItemDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? QuantityText { get; init; }
        public string? Unit { get; init; }
        public string State { get; init; } = "NIE_MAM";
        public bool IsManual { get; init; }
        public int SortOrder { get; init; }
        public string DisplayText { get; init; } = string.Empty;
    }

    private sealed class RecipeRecommendationsDto
    {
        public List<RecipeRecommendationDto> Items { get; init; } = [];
    }

    private sealed class RecipeRecommendationDto
    {
        public Guid RecipeId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? MainPhotoPath { get; init; }
        public int MatchedIngredientCount { get; init; }
        public int TotalIngredientCount { get; init; }
        public List<string> MatchedIngredients { get; init; } = [];
    }

    private sealed record SetStateRequestDto(string State);

    private sealed record AddManualItemRequestDto(string Text);

    private sealed class CopyTextResponseDto
    {
        public string Text { get; init; } = string.Empty;
    }
}

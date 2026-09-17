using WhatToCook.Api.Domain.Shopping;

namespace WhatToCook.Api.Features.Shopping.Lists;

public static class ShoppingListEndpoints
{
    public static IEndpointRouteBuilder MapShoppingListEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/shopping-lists").WithTags("Shopping");

        group
            .MapGet("/plan/{planId:guid}", GetShoppingListForPlan)
            .WithName("GetShoppingListForPlan");
        group
            .MapGet("/plan/{planId:guid}/recommendations", GetRecipeRecommendations)
            .WithName("GetShoppingListRecipeRecommendations");
        group
            .MapPut("/plan/{planId:guid}/items/{itemId:guid}/state", SetItemState)
            .WithName("SetShoppingListItemState");
        group
            .MapPost("/plan/{planId:guid}/manual-items", AddManualItem)
            .WithName("AddManualShoppingListItem");
        group
            .MapGet("/plan/{planId:guid}/copy-text", CopyShoppingListText)
            .WithName("CopyShoppingListText");

        return app;
    }

    private static async Task<IResult> GetShoppingListForPlan(
        Guid planId,
        ShoppingListService shoppingListService,
        CancellationToken cancellationToken
    )
    {
        var list = await shoppingListService.GetForPlanAsync(planId, cancellationToken);
        return list is null ? Results.NotFound() : Results.Ok(ToResponse(list));
    }

    private static async Task<IResult> GetRecipeRecommendations(
        Guid planId,
        int? limit,
        ShoppingListService shoppingListService,
        CancellationToken cancellationToken
    )
    {
        var recommendations = await shoppingListService.GetRecipeRecommendationsAsync(
            planId,
            limit ?? 6,
            cancellationToken
        );
        return recommendations is null
            ? Results.NotFound()
            : Results.Ok(
                new RecipeRecommendationsResponse(
                    recommendations
                        .Select(x => new RecipeRecommendationResponse(
                            x.RecipeId,
                            x.Title,
                            x.MainPhotoPath,
                            x.MatchedIngredientCount,
                            x.TotalIngredientCount,
                            x.MatchedIngredients
                        ))
                        .ToList()
                )
            );
    }

    private static async Task<IResult> SetItemState(
        Guid planId,
        Guid itemId,
        SetShoppingListItemStateRequest request,
        ShoppingListService shoppingListService,
        CancellationToken cancellationToken
    )
    {
        if (!TryParseState(request.State, out var state))
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    [nameof(SetShoppingListItemStateRequest.State)] =
                    [
                        "State must be one of: MAM, NIE_MAM.",
                    ],
                }
            );
        }

        var list = await shoppingListService.SetItemStateAsync(
            planId,
            itemId,
            state,
            cancellationToken
        );
        return list is null ? Results.NotFound() : Results.Ok(ToResponse(list));
    }

    private static async Task<IResult> AddManualItem(
        Guid planId,
        AddManualItemRequest request,
        ShoppingListService shoppingListService,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    [nameof(AddManualItemRequest.Text)] = ["Text is required."],
                }
            );
        }

        var list = await shoppingListService.AddManualItemAsync(
            planId,
            request.Text,
            cancellationToken
        );
        return list is null ? Results.NotFound() : Results.Ok(ToResponse(list));
    }

    private static async Task<IResult> CopyShoppingListText(
        Guid planId,
        ShoppingListService shoppingListService,
        CancellationToken cancellationToken
    )
    {
        var text = await shoppingListService.BuildCopyTextAsync(planId, cancellationToken);
        return text is null ? Results.NotFound() : Results.Ok(new CopyShoppingListResponse(text));
    }

    private static ShoppingListResponse ToResponse(ShoppingListView source) =>
        new(
            source.Id,
            source.MealPlanId,
            source.MealPlanName,
            source.UpdatedAt,
            source
                .Items.Select(x => new ShoppingListItemResponse(
                    x.Id,
                    x.Name,
                    x.QuantityText,
                    x.Unit,
                    ToStateText(x.State),
                    x.IsManual,
                    x.SortOrder,
                    x.DisplayText
                ))
                .ToList()
        );

    private static bool TryParseState(string? value, out ChecklistState state)
    {
        state = ChecklistState.NieMam;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim().ToUpperInvariant();
        if (normalized == "MAM")
        {
            state = ChecklistState.Mam;
            return true;
        }

        if (normalized is "NIE_MAM" or "NIEMAM")
        {
            state = ChecklistState.NieMam;
            return true;
        }

        return false;
    }

    private static string ToStateText(ChecklistState state) =>
        state == ChecklistState.Mam ? "MAM" : "NIE_MAM";

    private sealed record ShoppingListResponse(
        Guid Id,
        Guid MealPlanId,
        string MealPlanName,
        DateTimeOffset UpdatedAt,
        IReadOnlyList<ShoppingListItemResponse> Items
    );

    private sealed record ShoppingListItemResponse(
        Guid Id,
        string Name,
        string? QuantityText,
        string? Unit,
        string State,
        bool IsManual,
        int SortOrder,
        string DisplayText
    );

    private sealed record RecipeRecommendationsResponse(
        IReadOnlyList<RecipeRecommendationResponse> Items
    );

    private sealed record RecipeRecommendationResponse(
        Guid RecipeId,
        string Title,
        string? MainPhotoPath,
        int MatchedIngredientCount,
        int TotalIngredientCount,
        IReadOnlyList<string> MatchedIngredients
    );

    private sealed class SetShoppingListItemStateRequest
    {
        public string State { get; init; } = string.Empty;
    }

    private sealed class AddManualItemRequest
    {
        public string Text { get; init; } = string.Empty;
    }

    private sealed record CopyShoppingListResponse(string Text);
}

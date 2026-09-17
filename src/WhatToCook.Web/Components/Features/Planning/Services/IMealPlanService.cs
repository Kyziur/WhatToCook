namespace WhatToCook.Web.Components.Features.Planning.Services;

public interface IMealPlanService
{
    Task<IReadOnlyList<MealPlanSummary>> GetPlansAsync(
        CancellationToken cancellationToken = default
    );

    Task<MealPlanDetails?> GetPlanAsync(Guid planId, CancellationToken cancellationToken = default);

    Task<MealPlanDetails?> CreatePlanAsync(
        MealPlanCreateRequest request,
        CancellationToken cancellationToken = default
    );

    Task<MealPlanSaveResult> SavePlanAsync(
        Guid planId,
        MealPlanSaveRequest request,
        CancellationToken cancellationToken = default
    );

    Task<bool> MarkShoppingGeneratedAsync(
        Guid planId,
        CancellationToken cancellationToken = default
    );
}

public sealed record MealPlanSummary(
    Guid Id,
    string Name,
    DateOnly DateFrom,
    DateOnly DateTo,
    bool HasGeneratedShoppingList,
    int PlannedRecipeCount
);

public sealed record MealPlanEntry(
    Guid? Id,
    Guid RecipeId,
    string RecipeTitle,
    bool RecipeIsActive,
    DateOnly PlannedDate,
    decimal Multiplier,
    int SortOrder
);

public sealed record MealPlanDetails(
    Guid Id,
    string Name,
    DateOnly DateFrom,
    DateOnly DateTo,
    bool HasGeneratedShoppingList,
    IReadOnlyList<MealPlanEntry> Entries
);

public sealed record MealPlanCreateRequest(string? Name, DateOnly DateFrom, DateOnly DateTo);

public sealed record MealPlanSaveRequest(
    string Name,
    DateOnly DateFrom,
    DateOnly DateTo,
    bool RegenerateShoppingList,
    IReadOnlyList<MealPlanEntry> Entries
);

public sealed record MealPlanSaveResult(
    bool IsSuccess,
    bool RegenerationRequired,
    MealPlanDetails? Plan,
    string? ErrorMessage
)
{
    public static MealPlanSaveResult Success(MealPlanDetails plan) => new(true, false, plan, null);

    public static MealPlanSaveResult RegenerationRequiredResult(string? message = null) =>
        new(false, true, null, message);

    public static MealPlanSaveResult Failure(string? message) => new(false, false, null, message);
}

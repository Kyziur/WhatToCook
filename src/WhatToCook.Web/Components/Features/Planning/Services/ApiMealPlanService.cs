using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace WhatToCook.Web.Components.Features.Planning.Services;

public sealed class ApiMealPlanService(HttpClient httpClient) : IMealPlanService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<IReadOnlyList<MealPlanSummary>> GetPlansAsync(
        CancellationToken cancellationToken = default
    )
    {
        using var response = await httpClient.GetAsync("/api/plans", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return [];
        }

        var plans = await response.Content.ReadFromJsonAsync<List<MealPlanSummaryDto>>(
            JsonOptions,
            cancellationToken
        );

        return plans?.Select(MapSummary).ToList() ?? [];
    }

    public async Task<MealPlanDetails?> GetPlanAsync(
        Guid planId,
        CancellationToken cancellationToken = default
    )
    {
        using var response = await httpClient.GetAsync($"/api/plans/{planId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var details = await response.Content.ReadFromJsonAsync<MealPlanDetailsDto>(
            JsonOptions,
            cancellationToken
        );

        return details is null ? null : MapDetails(details);
    }

    public async Task<MealPlanDetails?> CreatePlanAsync(
        MealPlanCreateRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var response = await httpClient.PostAsJsonAsync("/api/plans", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<MealPlanDetailsDto>(
            JsonOptions,
            cancellationToken
        );
        return payload is null ? null : MapDetails(payload);
    }

    public async Task<MealPlanSaveResult> SavePlanAsync(
        Guid planId,
        MealPlanSaveRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var response = await httpClient.PutAsJsonAsync(
            $"/api/plans/{planId}",
            new SavePlanRequestDto(
                request.Name,
                request.DateFrom,
                request.DateTo,
                request.RegenerateShoppingList,
                request
                    .Entries.Select(x => new SavePlanEntryRequestDto(
                        x.Id,
                        x.RecipeId,
                        x.PlannedDate,
                        x.Multiplier,
                        x.SortOrder
                    ))
                    .ToList()
            ),
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var payload = await response.Content.ReadFromJsonAsync<MealPlanDetailsDto>(
                JsonOptions,
                cancellationToken
            );
            return payload is null
                ? MealPlanSaveResult.Failure("Nie udalo sie odczytac zapisanego planu.")
                : MealPlanSaveResult.Success(MapDetails(payload));
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var conflict = await response.Content.ReadFromJsonAsync<ConflictDto>(
                JsonOptions,
                cancellationToken
            );
            if (
                string.Equals(
                    conflict?.Code,
                    "shopping_list_regeneration_required",
                    StringComparison.Ordinal
                )
            )
            {
                return MealPlanSaveResult.RegenerationRequiredResult(conflict?.Message);
            }

            if (!string.IsNullOrWhiteSpace(conflict?.Message))
            {
                return MealPlanSaveResult.Failure(conflict.Message);
            }
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return MealPlanSaveResult.Failure(
            string.IsNullOrWhiteSpace(content) ? "Nie udalo sie zapisac planu." : content
        );
    }

    public async Task<bool> MarkShoppingGeneratedAsync(
        Guid planId,
        CancellationToken cancellationToken = default
    )
    {
        var response = await httpClient.PostAsync(
            $"/api/plans/{planId}/shopping-generated",
            content: null,
            cancellationToken
        );
        return response.IsSuccessStatusCode;
    }

    private static MealPlanSummary MapSummary(MealPlanSummaryDto source) =>
        new(
            source.Id,
            source.Name,
            source.DateFrom,
            source.DateTo,
            source.HasGeneratedShoppingList,
            source.PlannedRecipeCount
        );

    private static MealPlanDetails MapDetails(MealPlanDetailsDto source) =>
        new(
            source.Id,
            source.Name,
            source.DateFrom,
            source.DateTo,
            source.HasGeneratedShoppingList,
            source
                .Entries.Select(x => new MealPlanEntry(
                    x.Id,
                    x.RecipeId,
                    x.RecipeTitle,
                    x.RecipeIsActive,
                    x.PlannedDate,
                    x.Multiplier,
                    x.SortOrder
                ))
                .ToList()
        );

    private sealed class MealPlanSummaryDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public DateOnly DateFrom { get; init; }
        public DateOnly DateTo { get; init; }
        public bool HasGeneratedShoppingList { get; init; }
        public int PlannedRecipeCount { get; init; }
    }

    private sealed class MealPlanDetailsDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public DateOnly DateFrom { get; init; }
        public DateOnly DateTo { get; init; }
        public bool HasGeneratedShoppingList { get; init; }
        public List<MealPlanEntryDto> Entries { get; init; } = [];
    }

    private sealed class MealPlanEntryDto
    {
        public Guid Id { get; init; }
        public Guid RecipeId { get; init; }
        public string RecipeTitle { get; init; } = string.Empty;
        public bool RecipeIsActive { get; init; }
        public DateOnly PlannedDate { get; init; }
        public decimal Multiplier { get; init; }
        public int SortOrder { get; init; }
    }

    private sealed record SavePlanRequestDto(
        string Name,
        DateOnly DateFrom,
        DateOnly DateTo,
        bool RegenerateShoppingList,
        IReadOnlyList<SavePlanEntryRequestDto> Entries
    );

    private sealed record SavePlanEntryRequestDto(
        Guid? Id,
        Guid RecipeId,
        DateOnly PlannedDate,
        decimal Multiplier,
        int SortOrder
    );

    private sealed class ConflictDto
    {
        public string? Code { get; init; }
        public string? Message { get; init; }
    }
}

using WhatToCook.Api.Domain.Planning;

namespace WhatToCook.Api.Features.Planning.Plans;

/// <summary>
/// Domain rules for editing existing plan assignments. An archived recipe can
/// remain in its historical slot, but must not be added under an existing row ID.
/// </summary>
internal static class MealPlanMutationRules
{
    internal static bool IsRetainedInactiveAssignment(
        Guid? entryId,
        Guid recipeId,
        DateOnly plannedDate,
        IReadOnlyDictionary<Guid, PlannedRecipe> existingEntriesById
    ) =>
        entryId is { } id
        && existingEntriesById.TryGetValue(id, out var existingEntry)
        && existingEntry.RecipeId == recipeId
        && existingEntry.PlannedDate == plannedDate;
}

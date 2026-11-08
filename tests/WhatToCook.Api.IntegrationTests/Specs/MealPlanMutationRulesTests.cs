using WhatToCook.Api.Domain.Planning;
using WhatToCook.Api.Features.Planning.Plans;

namespace WhatToCook.Api.IntegrationTests.Specs;

public sealed class MealPlanMutationRulesTests
{
    [Fact]
    public void ArchivedRecipeCanOnlyKeepSameEntryRecipeAndDay()
    {
        var entry = new PlannedRecipe
        {
            Id = Guid.NewGuid(),
            RecipeId = Guid.NewGuid(),
            PlannedDate = new DateOnly(2026, 5, 1),
        };
        var previous = new Dictionary<Guid, PlannedRecipe> { [entry.Id] = entry };

        Assert.True(
            MealPlanMutationRules.IsRetainedInactiveAssignment(
                entry.Id,
                entry.RecipeId,
                entry.PlannedDate,
                previous
            )
        );
        Assert.False(
            MealPlanMutationRules.IsRetainedInactiveAssignment(
                null,
                entry.RecipeId,
                entry.PlannedDate,
                previous
            )
        );
        Assert.False(
            MealPlanMutationRules.IsRetainedInactiveAssignment(
                Guid.NewGuid(),
                entry.RecipeId,
                entry.PlannedDate,
                previous
            )
        );
        Assert.False(
            MealPlanMutationRules.IsRetainedInactiveAssignment(
                entry.Id,
                Guid.NewGuid(),
                entry.PlannedDate,
                previous
            )
        );
        Assert.False(
            MealPlanMutationRules.IsRetainedInactiveAssignment(
                entry.Id,
                entry.RecipeId,
                entry.PlannedDate.AddDays(1),
                previous
            )
        );
    }
}

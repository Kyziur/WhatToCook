namespace WhatToCook.Web.Components.Features.Recipes.Shared;

// One catalog of unit choices for recipe editing and import review.
internal static class RecipeUnitOptions
{
    public static IReadOnlyList<RecipeUnitOption> All { get; } =
    [
        new("", "brak jednostki"),
        new("szt", "szt."),
        new("g", "g"),
        new("kg", "kg"),
        new("ml", "ml"),
        new("l", "l"),
        new("lyzeczka", "łyżeczka"),
        new("lyzka", "łyżka"),
        new("szklanka", "szklanka"),
        new("opakowanie", "opakowanie"),
    ];
}

internal sealed record RecipeUnitOption(string Value, string Label);

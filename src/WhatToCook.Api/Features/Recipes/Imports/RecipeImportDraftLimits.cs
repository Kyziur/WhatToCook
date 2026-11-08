using WhatToCook.Api.Infrastructure.Validation;

namespace WhatToCook.Api.Features.Recipes.Imports;

// Validate untrusted draft data before creating entities or replacing persisted collections.
internal static class RecipeImportDraftLimits
{
    public const int MaxSourceUrlLength = 2000;

    public static string? Validate(
        string? title,
        string? source,
        IReadOnlyList<RecipeImportIngredientRequest>? ingredients,
        IReadOnlyList<string>? steps,
        IReadOnlyList<string>? tags,
        string? rawContent = null
    )
    {
        if (UnicodeTextLength.Exceeds(title, 200))
        {
            return "Draft title must be at most 200 characters.";
        }

        if (UnicodeTextLength.Exceeds(source, 2000))
        {
            return "Draft source must be at most 2000 characters.";
        }

        if (rawContent?.Length > 1_000_000)
        {
            return "Imported page content exceeds the supported size.";
        }

        if (ingredients is null || ingredients.Count > 100)
        {
            return "Draft ingredients must contain at most 100 entries.";
        }

        if (
            ingredients.Any(x =>
                x is null
                || UnicodeTextLength.Exceeds(x.Name, 200)
                || UnicodeTextLength.Exceeds(x.QuantityText, 64)
                || UnicodeTextLength.Exceeds(x.Unit, 32)
            )
        )
        {
            return "Draft ingredient fields exceed the supported limits.";
        }

        if (steps is null || steps.Count > 100)
        {
            return "Draft steps must contain at most 100 entries.";
        }

        if (steps.Any(x => x is null || UnicodeTextLength.Exceeds(x, 4000)))
        {
            return "Draft steps must be at most 4000 characters each.";
        }

        if (tags is null || tags.Count > 50)
        {
            return "Draft tags must contain at most 50 entries.";
        }

        if (tags.Any(x => x is null || UnicodeTextLength.Exceeds(x, 100)))
        {
            return "Draft tags must be at most 100 characters each.";
        }

        return null;
    }

    public static bool ExceedsMaxSourceUrlLength(string sourceUrl) =>
        UnicodeTextLength.Exceeds(sourceUrl, MaxSourceUrlLength, trim: false);
}

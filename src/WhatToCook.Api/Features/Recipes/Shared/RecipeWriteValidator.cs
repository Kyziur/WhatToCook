using WhatToCook.Api.Infrastructure.Validation;

namespace WhatToCook.Api.Features.Recipes.Shared;

public static class RecipeWriteValidator
{
    private static readonly HashSet<string> AllowedUnitsInternal = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "szt",
        "g",
        "kg",
        "ml",
        "l",
        "lyzeczka",
        "lyzka",
        "szklanka",
        "opakowanie",
    };

    public static IReadOnlySet<string> AllowedUnits => AllowedUnitsInternal;

    public static Dictionary<string, string[]> Validate(RecipeUpsertRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        // Explicit JSON nulls bypass non-nullable DTO annotations. Separate null guards
        // let nullable flow analysis prove collections are safe to enumerate below.
        if (request.Ingredients is null || request.Steps is null || request.Tags is null)
        {
            if (request.Ingredients is null)
            {
                errors[nameof(request.Ingredients)] = ["Ingredients must contain non-null items."];
            }

            if (request.Steps is null)
            {
                errors[nameof(request.Steps)] = ["Steps must contain non-null items."];
            }

            if (request.Tags is null)
            {
                errors[nameof(request.Tags)] = ["Tags must contain non-null items."];
            }

            return errors;
        }

        if (request.Ingredients.Any(x => x is null))
        {
            errors[nameof(request.Ingredients)] = ["Ingredients must contain non-null items."];
        }

        if (request.Steps.Any(x => x is null))
        {
            errors[nameof(request.Steps)] = ["Steps must contain non-null items."];
        }

        if (request.Tags.Any(x => x is null))
        {
            errors[nameof(request.Tags)] = ["Tags must contain non-null items."];
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            errors[nameof(request.Title)] = ["Title is required."];
        }
        else if (UnicodeTextLength.Exceeds(request.Title, 200))
        {
            errors[nameof(request.Title)] = ["Title must be at most 200 characters."];
        }

        if (request.Servings <= 0)
        {
            errors[nameof(request.Servings)] = ["Servings must be greater than zero."];
        }

        var validIngredients = request
            .Ingredients.Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .ToArray();

        if (validIngredients.Length == 0)
        {
            errors[nameof(request.Ingredients)] = ["At least one ingredient is required."];
        }

        if (request.Steps.All(string.IsNullOrWhiteSpace))
        {
            errors[nameof(request.Steps)] = ["At least one step is required."];
        }

        if (!string.IsNullOrWhiteSpace(request.Source))
        {
            if (UnicodeTextLength.Exceeds(request.Source, 500))
            {
                errors[nameof(request.Source)] = ["Source must be at most 500 characters."];
            }

            if (
                Uri.TryCreate(request.Source.Trim(), UriKind.Absolute, out var sourceUri)
                && sourceUri.Scheme is not ("http" or "https")
            )
            {
                errors[nameof(request.Source)] = ["Source URLs must use HTTP or HTTPS."];
            }
        }

        if (validIngredients.Length > 100 || request.Steps.Count > 100 || request.Tags.Count > 50)
        {
            errors[nameof(request.Ingredients)] =
            [
                "Recipe collections exceed the supported limits.",
            ];
        }

        if (
            validIngredients.Any(x =>
                UnicodeTextLength.Exceeds(x.Name, 200)
                || UnicodeTextLength.Exceeds(x.QuantityText, 64)
                || UnicodeTextLength.Exceeds(x.Unit, 32)
            )
        )
        {
            errors[nameof(request.Ingredients)] =
            [
                "Ingredient fields exceed the supported limits.",
            ];
        }

        if (request.Steps.Any(x => UnicodeTextLength.Exceeds(x, 4000)))
        {
            errors[nameof(request.Steps)] = ["Steps must be at most 4000 characters each."];
        }

        if (request.Tags.Any(x => UnicodeTextLength.Exceeds(x, 100)))
        {
            errors[nameof(request.Tags)] = ["Tags must be at most 100 characters each."];
        }

        var invalidUnit = validIngredients
            .Select(x => x.Unit?.Trim())
            .FirstOrDefault(x =>
                !string.IsNullOrWhiteSpace(x) && !AllowedUnitsInternal.Contains(x)
            );

        if (invalidUnit is not null)
        {
            errors[nameof(request.Ingredients)] = [$"Unit '{invalidUnit}' is not supported."];
        }

        return errors;
    }
}

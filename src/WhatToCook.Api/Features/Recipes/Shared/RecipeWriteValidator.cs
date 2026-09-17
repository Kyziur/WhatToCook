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

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            errors[nameof(request.Title)] = ["Title is required."];
        }
        else if (request.Title.Trim().Length > 200)
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
            if (request.Source.Trim().Length > 500)
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
                x.Name.Trim().Length > 200
                || x.QuantityText?.Trim().Length > 64
                || x.Unit?.Trim().Length > 32
            )
        )
        {
            errors[nameof(request.Ingredients)] =
            [
                "Ingredient fields exceed the supported limits.",
            ];
        }

        if (request.Steps.Any(x => x.Trim().Length > 4000))
        {
            errors[nameof(request.Steps)] = ["Steps must be at most 4000 characters each."];
        }

        if (request.Tags.Any(x => x.Trim().Length > 100))
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

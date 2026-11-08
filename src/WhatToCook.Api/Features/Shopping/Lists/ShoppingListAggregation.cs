using System.Globalization;
using WhatToCook.Api.Domain.Shopping;
using WhatToCook.Api.Features.Recipes.Shared;
using static WhatToCook.Api.Features.Shopping.Lists.ShoppingQuantity;

namespace WhatToCook.Api.Features.Shopping.Lists;

/// <summary>
/// Pure recipe-ingredient aggregation. Persistence, regeneration identity and
/// checklist reconciliation remain owned by ShoppingListService.
/// </summary>
internal static class ShoppingListAggregation
{
    internal static List<ShoppingListItem> BuildGeneratedItems(
        Guid shoppingListId,
        IEnumerable<Domain.Planning.PlannedRecipe> plannedRecipes
    )
    {
        var quantified = new Dictionary<(string Name, string? Unit), AggregatedIngredient>(
            StringTupleComparer.Instance
        );
        var descriptive = new Dictionary<(string Name, string? Unit), AggregatedIngredient>(
            StringTupleComparer.Instance
        );
        var nameOnly = new Dictionary<string, AggregatedIngredient>(StringComparer.Ordinal);

        foreach (var entry in plannedRecipes.OrderBy(x => x.PlannedDate).ThenBy(x => x.SortOrder))
        {
            foreach (var ingredient in entry.Recipe.Ingredients.OrderBy(x => x.SortOrder))
            {
                var normalizedName = ingredient.Ingredient.NormalizedName;
                var displayName = ingredient.Ingredient.DisplayName;
                var normalizedUnit = NormalizeUnit(ingredient.Unit);
                var unit = string.IsNullOrWhiteSpace(ingredient.Unit)
                    ? null
                    : ingredient.Unit.Trim();
                var quantityText = string.IsNullOrWhiteSpace(ingredient.QuantityText)
                    ? null
                    : ingredient.QuantityText.Trim();

                if (TryParseDecimal(quantityText, out var quantity))
                {
                    var key = (normalizedName, normalizedUnit);
                    var scaled = quantity * entry.Multiplier;

                    if (quantified.TryGetValue(key, out var existingQuantified))
                    {
                        existingQuantified.AggregatedQuantity += scaled;
                        continue;
                    }

                    quantified[key] = new AggregatedIngredient
                    {
                        Name = displayName,
                        NormalizedName = normalizedName,
                        Unit = unit,
                        AggregatedQuantity = scaled,
                    };
                    continue;
                }

                if (quantityText is null && unit is null)
                {
                    if (nameOnly.TryGetValue(normalizedName, out var existingNameOnly))
                    {
                        existingNameOnly.Repetitions++;
                        existingNameOnly.TotalMultiplier += entry.Multiplier;
                    }
                    else
                    {
                        nameOnly[normalizedName] = new AggregatedIngredient
                        {
                            Name = displayName,
                            NormalizedName = normalizedName,
                            Repetitions = 1,
                            TotalMultiplier = entry.Multiplier,
                        };
                    }

                    continue;
                }

                var descriptiveKey = (normalizedName, $"{quantityText}|{normalizedUnit}");
                if (descriptive.TryGetValue(descriptiveKey, out var descriptiveExisting))
                {
                    descriptiveExisting.Repetitions++;
                    descriptiveExisting.TotalMultiplier += entry.Multiplier;
                    continue;
                }

                descriptive[descriptiveKey] = new AggregatedIngredient
                {
                    Name = displayName,
                    NormalizedName = normalizedName,
                    QuantityText = quantityText,
                    Unit = unit,
                    Repetitions = 1,
                    TotalMultiplier = entry.Multiplier,
                };
            }
        }

        var results = new List<ShoppingListItem>();
        foreach (var aggregate in quantified.Values.OrderBy(x => x.Name).ThenBy(x => x.Unit))
        {
            results.Add(
                new ShoppingListItem
                {
                    ShoppingListId = shoppingListId,
                    Name = aggregate.Name,
                    NormalizedName = aggregate.NormalizedName,
                    QuantityText = FormatQuantity(aggregate.AggregatedQuantity),
                    Unit = aggregate.Unit,
                    IsManual = false,
                    State = ChecklistState.NieMam,
                }
            );
        }

        foreach (var aggregate in descriptive.Values.OrderBy(x => x.Name).ThenBy(x => x.Unit))
        {
            results.Add(
                new ShoppingListItem
                {
                    ShoppingListId = shoppingListId,
                    Name = aggregate.Name,
                    NormalizedName = aggregate.NormalizedName,
                    QuantityText = aggregate.QuantityText,
                    QuantityContext = BuildQuantityContext(aggregate),
                    Unit = aggregate.Unit,
                    IsManual = false,
                    State = ChecklistState.NieMam,
                }
            );
        }

        foreach (var aggregate in nameOnly.Values.OrderBy(x => x.Name))
        {
            results.Add(
                new ShoppingListItem
                {
                    ShoppingListId = shoppingListId,
                    Name = aggregate.Name,
                    NormalizedName = aggregate.NormalizedName,
                    QuantityContext = BuildQuantityContext(aggregate),
                    IsManual = false,
                    State = ChecklistState.NieMam,
                }
            );
        }

        for (var index = 0; index < results.Count; index++)
        {
            results[index].SortOrder = index;
        }

        return results;
    }

    private static string? BuildQuantityContext(AggregatedIngredient aggregate)
    {
        if (aggregate.Repetitions == 1 && aggregate.TotalMultiplier == 1m)
        {
            return null;
        }

        // Descriptive quantities cannot be multiplied numerically. Keep the source phrase
        // intact and expose how often it occurs and the sum of plan multipliers.
        return $"wystąpienia: {aggregate.Repetitions}; suma mnożników: ×{FormatQuantity(aggregate.TotalMultiplier)}";
    }

    private sealed class AggregatedIngredient
    {
        public string Name { get; init; } = string.Empty;
        public string NormalizedName { get; init; } = string.Empty;
        public string? QuantityText { get; set; }
        public string? Unit { get; init; }
        public decimal AggregatedQuantity { get; set; }
        public int Repetitions { get; set; }
        public decimal TotalMultiplier { get; set; }
    }

    private sealed class StringTupleComparer : IEqualityComparer<(string Name, string? Unit)>
    {
        public static readonly StringTupleComparer Instance = new();

        public bool Equals((string Name, string? Unit) x, (string Name, string? Unit) y) =>
            string.Equals(x.Name, y.Name, StringComparison.Ordinal)
            && string.Equals(x.Unit, y.Unit, StringComparison.Ordinal);

        public int GetHashCode((string Name, string? Unit) obj) =>
            HashCode.Combine(obj.Name, obj.Unit);
    }
}

/// <summary>Common amount/unit semantics used by aggregation and regeneration.</summary>
internal static class ShoppingQuantity
{
    internal static bool TryParseDecimal(string? value, out decimal parsed)
    {
        parsed = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim().Replace(',', '.');
        return decimal.TryParse(
            normalized,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out parsed
        );
    }

    internal static string? NormalizeUnit(string? unit) =>
        string.IsNullOrWhiteSpace(unit) ? null : TextNormalizer.Normalize(unit);

    internal static string FormatQuantity(decimal value) =>
        value % 1 == 0
            ? value.ToString("0", CultureInfo.InvariantCulture)
            : value.ToString("0.##", CultureInfo.InvariantCulture);

}

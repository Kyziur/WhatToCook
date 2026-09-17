using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace WhatToCook.Api.Features.Recipes.Imports;

internal static partial class RecipeImportIngredientParser
{
    public static RecipeImportIngredientRequest ParseIngredientText(string cleaned)
    {
        cleaned = NormalizeFractions(RecipeImportHtml.NormalizeWhitespace(cleaned));
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return new RecipeImportIngredientRequest();
        }

        if (TryParseQuantityWithUnit(cleaned, out var quantityText, out var unit, out var name))
        {
            return new RecipeImportIngredientRequest
            {
                Name = name,
                QuantityText = quantityText,
                Unit = unit,
            };
        }

        return new RecipeImportIngredientRequest { Name = cleaned };
    }

    public static (string? QuantityText, string? Unit) ParseQuantityParts(string? rawValue)
    {
        var cleaned = NormalizeFractions(
            RecipeImportHtml.NormalizeWhitespace(rawValue ?? string.Empty)
        );
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return (null, null);
        }

        var candidate = TrimDecorators(cleaned);
        if (TryParseBareQuantityWithUnit(candidate, out var quantityText, out var unit))
        {
            return (quantityText, unit);
        }

        if (TryParseQuantityOnly(candidate, out quantityText))
        {
            return (quantityText, null);
        }

        return (candidate, null);
    }

    public static int? ParseServings(string? rawValue)
    {
        var cleaned = NormalizeFractions(
            RecipeImportHtml.NormalizeWhitespace(rawValue ?? string.Empty)
        );
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return null;
        }

        var folded = FoldToAscii(cleaned);
        if (
            folded.Contains("cm", StringComparison.OrdinalIgnoreCase)
            || folded.Contains(" x ", StringComparison.OrdinalIgnoreCase)
            || folded.Contains("tortownic", StringComparison.OrdinalIgnoreCase)
            || folded.Contains("keksowk", StringComparison.OrdinalIgnoreCase)
            || folded.Contains("blasz", StringComparison.OrdinalIgnoreCase)
            || folded.Contains("foremk", StringComparison.OrdinalIgnoreCase)
            || folded.Contains("forma", StringComparison.OrdinalIgnoreCase)
        )
        {
            return null;
        }

        var match = LeadingIntegerRegex().Match(cleaned);
        if (!match.Success)
        {
            return null;
        }

        return int.TryParse(
            match.Groups["value"].Value,
            CultureInfo.InvariantCulture,
            out var value
        )
            ? value
            : null;
    }

    private static bool TryParseQuantityWithUnit(
        string cleaned,
        out string quantityText,
        out string? unit,
        out string name
    )
    {
        quantityText = string.Empty;
        unit = null;
        name = cleaned;

        var match = IngredientRegex().Match(cleaned);
        if (!match.Success)
        {
            return false;
        }

        quantityText = RecipeImportHtml.NormalizeWhitespace(match.Groups["quantity"].Value);
        var rawUnit =
            RecipeImportHtml.NormalizeWhitespace(match.Groups["unit"].Value) ?? string.Empty;
        unit = NormalizeUnit(rawUnit);
        var parsedName = RecipeImportHtml.NormalizeWhitespace(
            match.Groups["name"].Value.Trim(' ', ',', '-', '(', ')')
        );
        name = unit is null
            ? RecipeImportHtml.NormalizeWhitespace($"{rawUnit} {parsedName}")
            : parsedName;

        return !string.IsNullOrWhiteSpace(name);
    }

    private static bool TryParseBareQuantityWithUnit(
        string cleaned,
        out string quantityText,
        out string? unit
    )
    {
        quantityText = string.Empty;
        unit = null;

        var match = QuantityWithUnitRegex().Match(cleaned);
        if (!match.Success)
        {
            return false;
        }

        quantityText = RecipeImportHtml.NormalizeWhitespace(match.Groups["quantity"].Value);
        unit = NormalizeUnit(match.Groups["unit"].Value);
        return true;
    }

    private static bool TryParseQuantityOnly(string cleaned, out string quantityText)
    {
        quantityText = string.Empty;
        var match = QuantityOnlyRegex().Match(cleaned);
        if (!match.Success)
        {
            return false;
        }

        quantityText = RecipeImportHtml.NormalizeWhitespace(match.Groups["quantity"].Value);
        return true;
    }

    private static string TrimDecorators(string cleaned)
    {
        var dashIndex = cleaned.IndexOf(" - ", StringComparison.Ordinal);
        if (dashIndex > 0)
        {
            return cleaned[..dashIndex];
        }

        var parenthesisIndex = cleaned.IndexOf(" (", StringComparison.Ordinal);
        if (parenthesisIndex > 0)
        {
            return cleaned[..parenthesisIndex];
        }

        return cleaned;
    }

    private static string NormalizeFractions(string value)
    {
        return value
            .Replace("\u00BD", "1/2", StringComparison.Ordinal)
            .Replace("\u00BC", "1/4", StringComparison.Ordinal)
            .Replace("\u00BE", "3/4", StringComparison.Ordinal)
            .Replace("\u2153", "1/3", StringComparison.Ordinal)
            .Replace("\u2154", "2/3", StringComparison.Ordinal);
    }

    private static string? NormalizeUnit(string rawUnit)
    {
        if (string.IsNullOrWhiteSpace(rawUnit))
        {
            return null;
        }

        var folded = FoldToAscii(rawUnit).Trim('.', ' ', ',', ';', ':');
        return folded switch
        {
            "g" or "gram" or "gramy" or "gramow" => "g",
            "kg" or "kilogram" or "kilogramy" => "kg",
            "ml" or "mililitr" or "mililitry" => "ml",
            "l" or "litr" or "litry" => "l",
            "szklanka" or "szklanki" or "szklanek" => "szklanka",
            "lyzka" or "lyzki" or "lyzek" => "lyzka",
            "lyzeczka" or "lyzeczki" or "lyzeczek" => "lyzeczka",
            "szt" or "sztuka" or "sztuki" or "sztuk" => "szt",
            "opakowanie" or "opakowania" or "opakowan" => "opakowanie",
            _ => null,
        };
    }

    private static string FoldToAscii(string value)
    {
        var normalized = value
            .Replace('\u0142', 'l')
            .Replace('\u0141', 'L')
            .Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder(normalized.Length);
        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex(
        "^(?<quantity>(?:(?:ok\\.?|okolo|ponad|niecale?|niepelne?|pelna|pelne|duza|duze|mala|male)\\s+)*(?:\\d+\\s+\\d+/\\d+|\\d+/\\d+|\\d+(?:[.,]\\d+)?))\\s*(?<unit>[\\p{L}.]+)\\b(?:\\s+(?<name>.+))$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
    )]
    private static partial Regex IngredientRegex();

    [GeneratedRegex(
        "^(?<quantity>(?:(?:ok\\.?|okolo|ponad|niecale?|niepelne?|pelna|pelne|duza|duze|mala|male)\\s+)*(?:\\d+\\s+\\d+/\\d+|\\d+/\\d+|\\d+(?:[.,]\\d+)?))\\s*(?<unit>[\\p{L}.]+)\\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
    )]
    private static partial Regex QuantityWithUnitRegex();

    [GeneratedRegex(
        "^(?<quantity>(?:(?:ok\\.?|okolo|ponad|niecale?|niepelne?|pelna|pelne|duza|duze|mala|male)\\s+)*(?:\\d+\\s+\\d+/\\d+|\\d+/\\d+|\\d+(?:[.,]\\d+)?))$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
    )]
    private static partial Regex QuantityOnlyRegex();

    [GeneratedRegex("(?<value>\\d+)")]
    private static partial Regex LeadingIntegerRegex();
}

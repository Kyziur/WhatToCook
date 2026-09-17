using System.Globalization;
using System.Text;

namespace WhatToCook.Api.Features.Recipes.Shared;

public static class TextNormalizer
{
    public static string Normalize(string value)
    {
        var trimmed = ReplacePolishCharacters(value.Trim());
        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        var normalized = trimmed.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }

    private static string ReplacePolishCharacters(string value)
    {
        return value
            .Replace("ą", "a", StringComparison.Ordinal)
            .Replace("ć", "c", StringComparison.Ordinal)
            .Replace("ę", "e", StringComparison.Ordinal)
            .Replace("ł", "l", StringComparison.Ordinal)
            .Replace("ń", "n", StringComparison.Ordinal)
            .Replace("ó", "o", StringComparison.Ordinal)
            .Replace("ś", "s", StringComparison.Ordinal)
            .Replace("ź", "z", StringComparison.Ordinal)
            .Replace("ż", "z", StringComparison.Ordinal)
            .Replace("Ą", "A", StringComparison.Ordinal)
            .Replace("Ć", "C", StringComparison.Ordinal)
            .Replace("Ę", "E", StringComparison.Ordinal)
            .Replace("Ł", "L", StringComparison.Ordinal)
            .Replace("Ń", "N", StringComparison.Ordinal)
            .Replace("Ó", "O", StringComparison.Ordinal)
            .Replace("Ś", "S", StringComparison.Ordinal)
            .Replace("Ź", "Z", StringComparison.Ordinal)
            .Replace("Ż", "Z", StringComparison.Ordinal);
    }
}

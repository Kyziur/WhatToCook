using System.Text.RegularExpressions;

namespace WhatToCook.Web.Components.Shared;

public static partial class IngredientFilterNameNormalizer
{
    public static string Normalize(string? value)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        var match = LeadingQuantityAndUnitRegex().Match(trimmed);
        return match.Success && match.Groups["name"].Value.Trim().Length > 0
            ? match.Groups["name"].Value.Trim(' ', ',', '-', '(', ')')
            : trimmed;
    }

    [GeneratedRegex(
        "^\\s*(?:(?:ok\\.?|około|ponad|niecałe?|niepełne?|pełna|pełne|duża|duże|mała|małe)\\s+)*(?:\\d+\\s+\\d+/\\d+|\\d+/\\d+|\\d+(?:[.,]\\d+)?)\\s*(?:kg|g|ml|l|szt\\.?|sztuka|sztuki|sztuk|opakowanie|opakowania|opakowań|łyżka|łyżki|łyżek|łyżeczka|łyżeczki|łyżeczek|szklanka|szklanki|szklanek|puszka|puszki|puszek|pęczek|pęczki|pęczków|ząbek|ząbki|ząbków|cm|plaster|plastry|plastrów|garść|garście)\\.?[)\\]]*\\s+(?<name>.+)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
    )]
    private static partial Regex LeadingQuantityAndUnitRegex();
}

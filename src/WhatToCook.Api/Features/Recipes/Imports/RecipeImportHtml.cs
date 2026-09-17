using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Dom;

namespace WhatToCook.Api.Features.Recipes.Imports;

internal static partial class RecipeImportHtml
{
    public static string Decode(string value)
    {
        return NormalizeWhitespace(WebUtility.HtmlDecode(value).Replace('\u00A0', ' '));
    }

    public static string DecodePreservingLineBreaks(string value)
    {
        return WebUtility.HtmlDecode(value).Replace('\u00A0', ' ');
    }

    public static IReadOnlyList<string> ExtractLogicalLines(IElement element)
    {
        var builder = new StringBuilder();
        AppendNodeText(element, builder);

        return builder
            .ToString()
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(NormalizeWhitespace)
            .Where(x => x.Length > 0)
            .ToList();
    }

    public static string NormalizeWhitespace(string value)
    {
        return WhitespaceRegex().Replace(value, " ").Trim();
    }

    public static string FoldToAscii(string value)
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

    private static void AppendNodeText(INode node, StringBuilder builder)
    {
        if (node is IText textNode)
        {
            builder.Append(textNode.Data);
            return;
        }

        if (node is not IElement element)
        {
            return;
        }

        if (element.TagName.Equals("BR", StringComparison.OrdinalIgnoreCase))
        {
            builder.AppendLine();
            return;
        }

        foreach (var child in element.ChildNodes)
        {
            AppendNodeText(child, builder);
        }
    }

    [GeneratedRegex("\\s+", RegexOptions.Singleline)]
    private static partial Regex WhitespaceRegex();
}

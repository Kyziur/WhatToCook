using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace WhatToCook.Api.Features.Recipes.Imports;

public sealed partial class MojeWypiekiRecipeImportSourceAdapter()
    : RecipeImportSourceAdapterBase("mojewypieki", priority: 70)
{
    private static readonly HtmlParser HtmlParser = new();

    public override bool CanHandle(Uri sourceUri)
    {
        return MatchesHost(sourceUri, "mojewypieki.com");
    }

    public override RecipeImportExtractionResult Extract(Uri sourceUri, string html)
    {
        var document = HtmlParser.ParseDocument(html);
        var title = ExtractTitle(document);
        var contentRoot =
            document.QuerySelector(".article__content") ?? document.QuerySelector("article");

        if (string.IsNullOrWhiteSpace(title) || contentRoot is null)
        {
            return RecipeImportExtractionResult.Failure(
                "The recipe page could not be extracted into a usable draft."
            );
        }

        var servingsText = ExtractServingsText(contentRoot);
        var ingredients = ExtractIngredients(contentRoot);
        var steps = ExtractSteps(contentRoot);

        if (ingredients.Count == 0 || steps.Count == 0)
        {
            return RecipeImportExtractionResult.Failure(
                "The recipe page could not be extracted into a usable draft."
            );
        }

        return RecipeImportExtractionResult.Success(
            new RecipeImportExtractedDraft(
                title,
                RecipeImportIngredientParser.ParseServings(servingsText),
                sourceUri.ToString(),
                html,
                ingredients,
                steps,
                []
            )
        );
    }

    private static string? ExtractTitle(IHtmlDocument document)
    {
        var title = NormalizeText(document.QuerySelector("h1")?.TextContent);
        if (!string.IsNullOrWhiteSpace(title))
        {
            return title;
        }

        return NormalizeText(
            document.QuerySelector("meta[property='og:title']")?.GetAttribute("content")
        )
            ?.Replace(" - Moje Wypieki", string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    private static string? ExtractServingsText(IElement contentRoot)
    {
        foreach (var line in EnumerateLines(contentRoot))
        {
            var folded = Folded(line);
            if (
                folded.StartsWith("skladniki na", StringComparison.OrdinalIgnoreCase)
                && folded.Contains("porcj", StringComparison.OrdinalIgnoreCase)
            )
            {
                return line;
            }
        }

        return null;
    }

    private static List<RecipeImportIngredientRequest> ExtractIngredients(IElement contentRoot)
    {
        var ingredients = new List<RecipeImportIngredientRequest>();
        var collecting = false;

        foreach (var child in contentRoot.Children)
        {
            var lines = RecipeImportHtml.ExtractLogicalLines(child);
            if (lines.Count == 0)
            {
                continue;
            }

            if (child.TagName.Equals("UL", StringComparison.OrdinalIgnoreCase))
            {
                collecting = true;
                foreach (var line in lines)
                {
                    var ingredient = RecipeImportIngredientParser.ParseIngredientText(line);
                    if (!string.IsNullOrWhiteSpace(ingredient.Name))
                    {
                        ingredients.Add(ingredient);
                    }
                }

                continue;
            }

            if (collecting && lines.Any(IsTerminalLine))
            {
                break;
            }

            if (lines.Any(IsIngredientHeading) || lines.Any(LooksLikeServingsLine))
            {
                continue;
            }
        }

        return ingredients;
    }

    private static List<string> ExtractSteps(IElement contentRoot)
    {
        var steps = new List<string>();
        var sawIngredients = false;

        foreach (var child in contentRoot.Children)
        {
            var lines = RecipeImportHtml.ExtractLogicalLines(child);
            if (lines.Count == 0)
            {
                continue;
            }

            if (child.TagName.Equals("UL", StringComparison.OrdinalIgnoreCase))
            {
                sawIngredients = true;
                continue;
            }

            if (!sawIngredients)
            {
                continue;
            }

            if (lines.Any(IsTerminalLine))
            {
                break;
            }

            if (lines.Any(IsIngredientHeading) || lines.Any(LooksLikeServingsLine))
            {
                continue;
            }

            if (!LooksLikeStepParagraph(child, lines))
            {
                continue;
            }

            foreach (var line in lines)
            {
                var cleaned = StepPrefixRegex().Replace(line, string.Empty).Trim();
                if (cleaned.Length > 0 && !IsTerminalLine(cleaned))
                {
                    steps.Add(cleaned);
                }
            }
        }

        return steps;
    }

    private static IEnumerable<string> EnumerateLines(IElement contentRoot)
    {
        foreach (var child in contentRoot.Children)
        {
            foreach (var line in RecipeImportHtml.ExtractLogicalLines(child))
            {
                yield return line;
            }
        }
    }

    private static bool LooksLikeStepParagraph(IElement element, IReadOnlyList<string> lines)
    {
        if (!element.TagName.Equals("P", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return lines.Any(line =>
            !IsIngredientHeading(line)
            && !LooksLikeServingsLine(line)
            && !line.Equals("Smacznego!", StringComparison.OrdinalIgnoreCase)
        );
    }

    private static bool LooksLikeServingsLine(string line)
    {
        var folded = Folded(line);
        return folded.StartsWith("skladniki na", StringComparison.OrdinalIgnoreCase)
            && folded.Contains("porcj", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsIngredientHeading(string line)
    {
        var folded = Folded(line).TrimEnd(':', '.');
        return folded.StartsWith("skladniki na", StringComparison.OrdinalIgnoreCase)
            || folded.Equals("ponadto", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTerminalLine(string line)
    {
        var folded = Folded(line);
        return folded.StartsWith("smacznego", StringComparison.OrdinalIgnoreCase)
            || folded.StartsWith("zrodlo", StringComparison.OrdinalIgnoreCase)
            || folded.StartsWith("uwagi", StringComparison.OrdinalIgnoreCase);
    }

    private static string Folded(string value)
    {
        return value
            .Replace('\u0105', 'a')
            .Replace('\u0107', 'c')
            .Replace('\u0119', 'e')
            .Replace('\u0142', 'l')
            .Replace('\u0144', 'n')
            .Replace('\u00F3', 'o')
            .Replace('\u015B', 's')
            .Replace('\u017A', 'z')
            .Replace('\u017C', 'z')
            .ToLowerInvariant();
    }

    private static string? NormalizeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return RecipeImportHtml.NormalizeWhitespace(
            value.Replace('\u00A0', ' ').Replace('\r', ' ').Replace('\n', ' ')
        );
    }

    [GeneratedRegex("^\\d+[.)]?\\s*")]
    private static partial Regex StepPrefixRegex();
}

using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace WhatToCook.Api.Features.Recipes.Imports;

public sealed partial class RozkosznyRecipeImportSourceAdapter()
    : RecipeImportSourceAdapterBase("rozkoszny", priority: 90)
{
    private static readonly HtmlParser HtmlParser = new();

    public override bool CanHandle(Uri sourceUri)
    {
        return MatchesHost(sourceUri, "rozkoszny.pl");
    }

    public override RecipeImportExtractionResult Extract(Uri sourceUri, string html)
    {
        var document = HtmlParser.ParseDocument(html);
        var title = ExtractTitle(document);
        var contentRoot = FindContentRoot(document);

        if (string.IsNullOrWhiteSpace(title) || contentRoot is null)
        {
            return RecipeImportExtractionResult.Failure(
                "The recipe page could not be extracted into a usable draft."
            );
        }

        var ingredientBlocks = ExtractIngredientBlocks(contentRoot, title);
        var servingsText = ExtractServingsText(ingredientBlocks, title);
        var ingredients = ingredientBlocks
            .SelectMany(x => ParseIngredientBlock(x, title, servingsText))
            .ToList();
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
            ?.Replace(" - Rozkoszny", string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    private static IElement? FindContentRoot(IHtmlDocument document)
    {
        return document.QuerySelector(
                ".elementor-widget-theme-post-content .elementor-widget-container"
            )
            ?? document.QuerySelector(".entry-content")
            ?? document.QuerySelector("article");
    }

    private static List<IElement> ExtractIngredientBlocks(IElement contentRoot, string title)
    {
        var blocks = new List<IElement>();
        foreach (var paragraph in contentRoot.Children.OfType<IHtmlParagraphElement>())
        {
            var lines = RecipeImportHtml.ExtractLogicalLines(paragraph);
            if (lines.Count == 0)
            {
                continue;
            }

            if (LooksLikeStep(lines[0]) || IsAdviceBlock(lines))
            {
                break;
            }

            if (LooksLikeIngredientBlock(paragraph, lines, title))
            {
                blocks.Add(paragraph);
            }
        }

        return blocks;
    }

    private static string? ExtractServingsText(IEnumerable<IElement> ingredientBlocks, string title)
    {
        var firstBlock = ingredientBlocks.FirstOrDefault();
        if (firstBlock is null)
        {
            return null;
        }

        var lines = RecipeImportHtml.ExtractLogicalLines(firstBlock);
        return lines.FirstOrDefault(x =>
            !x.Equals(title, StringComparison.OrdinalIgnoreCase)
            && (LooksLikeYieldLine(x) || LooksLikePanSize(x))
        );
    }

    private static bool LooksLikeYieldLine(string line)
    {
        var folded = line.Replace('ł', 'l').Replace('Ł', 'L').Replace('ó', 'o').Replace('Ó', 'O');
        return folded.Contains("porcj", StringComparison.OrdinalIgnoreCase)
            || folded.Contains("sloik", StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<RecipeImportIngredientRequest> ParseIngredientBlock(
        IElement block,
        string title,
        string? servingsText
    )
    {
        var lines = RecipeImportHtml.ExtractLogicalLines(block);
        foreach (var line in lines)
        {
            if (
                line.Equals(title, StringComparison.OrdinalIgnoreCase)
                || string.Equals(line, servingsText, StringComparison.OrdinalIgnoreCase)
                || LooksLikePanSize(line)
                || IsIngredientHeading(line)
            )
            {
                continue;
            }

            var ingredient = RecipeImportIngredientParser.ParseIngredientText(line);
            if (!string.IsNullOrWhiteSpace(ingredient.Name))
            {
                yield return ingredient;
            }
        }
    }

    private static List<string> ExtractSteps(IElement contentRoot)
    {
        var steps = new List<string>();
        foreach (var paragraph in contentRoot.Children.OfType<IHtmlParagraphElement>())
        {
            var text = NormalizeText(paragraph.TextContent);
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            if (IsAdviceBlock([text]))
            {
                break;
            }

            if (!LooksLikeStep(text))
            {
                continue;
            }

            var cleaned = StepPrefixRegex().Replace(text, string.Empty).Trim();
            if (cleaned.Length > 0)
            {
                steps.Add(cleaned);
            }
        }

        return steps;
    }

    private static bool LooksLikeIngredientBlock(
        IElement paragraph,
        IReadOnlyList<string> lines,
        string title
    )
    {
        if (lines.Count < 2)
        {
            return false;
        }

        if (lines[0].Equals(title, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (paragraph.ClassList.Contains("has-background"))
        {
            return true;
        }

        var style = paragraph.GetAttribute("style") ?? string.Empty;
        return style.Contains("text-align: center", StringComparison.OrdinalIgnoreCase)
            || style.Contains("background-color", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAdviceBlock(IReadOnlyList<string> lines)
    {
        return lines.Any(x =>
            x.StartsWith("Rady/porady", StringComparison.OrdinalIgnoreCase)
            || x.StartsWith("Nie zapomnij", StringComparison.OrdinalIgnoreCase)
            || x.StartsWith("Wasz,", StringComparison.OrdinalIgnoreCase)
        );
    }

    private static bool IsIngredientHeading(string line)
    {
        if (line.Length > 40 || Regex.IsMatch(line, @"\d"))
        {
            return false;
        }

        if (LooksLikePanSize(line))
        {
            return true;
        }

        return line.Equals(line.ToUpperInvariant(), StringComparison.Ordinal)
            || char.IsUpper(line[0]);
    }

    private static bool LooksLikePanSize(string line)
    {
        return line.Contains("cm", StringComparison.OrdinalIgnoreCase)
            || line.Contains(" x ", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeStep(string line)
    {
        return StepPrefixRegex().IsMatch(line);
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

    [GeneratedRegex(@"^\d+\.\s*")]
    private static partial Regex StepPrefixRegex();
}

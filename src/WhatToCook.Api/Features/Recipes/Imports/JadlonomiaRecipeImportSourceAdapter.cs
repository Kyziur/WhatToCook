using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace WhatToCook.Api.Features.Recipes.Imports;

public sealed partial class JadlonomiaRecipeImportSourceAdapter()
    : RecipeImportSourceAdapterBase("jadlonomia", priority: 80)
{
    private static readonly HtmlParser HtmlParser = new();

    public override bool CanHandle(Uri sourceUri)
    {
        return MatchesHost(sourceUri, "jadlonomia.com");
    }

    public override RecipeImportExtractionResult Extract(Uri sourceUri, string html)
    {
        var document = HtmlParser.ParseDocument(html);
        var recipeScope = FindRecipeScope(document);
        var title = ExtractTitle(document, recipeScope);
        var ingredientsSection = FindIngredientsSection(recipeScope);
        var preparationSection = FindPreparationSection(recipeScope, ingredientsSection);

        if (
            string.IsNullOrWhiteSpace(title)
            || ingredientsSection is null
            || preparationSection is null
        )
        {
            return RecipeImportExtractionResult.Failure(
                "The recipe page could not be extracted into a usable draft."
            );
        }

        var servingsText = NormalizeText(ingredientsSection.TextContent);
        var ingredients = ExtractIngredients(ingredientsSection, preparationSection);
        var steps = ExtractSteps(preparationSection);

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

    private static string? ExtractTitle(IHtmlDocument document, IElement recipeScope)
    {
        var printableTitle = NormalizeText(
            recipeScope.QuerySelector("h2[itemprop='name']")?.TextContent
        );
        if (!string.IsNullOrWhiteSpace(printableTitle))
        {
            return printableTitle;
        }

        var title = NormalizeText(document.QuerySelector("h1")?.TextContent);
        if (!string.IsNullOrWhiteSpace(title))
        {
            return title;
        }

        return NormalizeText(
            document.QuerySelector("meta[property='og:title']")?.GetAttribute("content")
        );
    }

    private static IElement FindRecipeScope(IHtmlDocument document)
    {
        return document.QuerySelector("#PrintIcon")?.ParentElement
            ?? document.Body
            ?? document.DocumentElement;
    }

    private static IElement? FindIngredientsSection(IElement recipeScope)
    {
        return recipeScope
            .QuerySelectorAll(".font-bold.subtitle, .font-bold.subtitle.big-margin-top")
            .FirstOrDefault(x => HasHeading(x, "skladniki na"));
    }

    private static IElement? FindPreparationSection(
        IElement recipeScope,
        IElement? ingredientsSection
    )
    {
        if (ingredientsSection is not null)
        {
            for (
                var sibling = ingredientsSection.NextElementSibling;
                sibling is not null;
                sibling = sibling.NextElementSibling
            )
            {
                if (HasHeading(sibling, "przygotowanie"))
                {
                    return sibling;
                }
            }
        }

        return recipeScope
            .QuerySelectorAll(".font-bold.subtitle, .font-bold.subtitle.big-margin-top")
            .FirstOrDefault(x => HasHeading(x, "przygotowanie"));
    }

    private static List<RecipeImportIngredientRequest> ExtractIngredients(
        IElement ingredientsSection,
        IElement preparationSection
    )
    {
        var ingredients = new List<RecipeImportIngredientRequest>();
        for (
            var sibling = ingredientsSection.NextElementSibling;
            sibling is not null && !ReferenceEquals(sibling, preparationSection);
            sibling = sibling.NextElementSibling
        )
        {
            foreach (var line in RecipeImportHtml.ExtractLogicalLines(sibling))
            {
                if (ShouldSkipIngredientLine(line))
                {
                    continue;
                }

                var ingredient = RecipeImportIngredientParser.ParseIngredientText(line);
                if (!string.IsNullOrWhiteSpace(ingredient.Name))
                {
                    ingredients.Add(ingredient);
                }
            }
        }

        return ingredients;
    }

    private static List<string> ExtractSteps(IElement preparationSection)
    {
        var orderedSteps = preparationSection
            .NextElementSibling?.QuerySelectorAll("li")
            .Select(x => NormalizeText(x.TextContent))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .ToList();
        if (orderedSteps?.Count > 0)
        {
            return orderedSteps;
        }

        var steps = new List<string>();
        for (
            var sibling = preparationSection.NextElementSibling;
            sibling is not null;
            sibling = sibling.NextElementSibling
        )
        {
            foreach (var line in RecipeImportHtml.ExtractLogicalLines(sibling))
            {
                if (line.StartsWith("Porady", StringComparison.OrdinalIgnoreCase))
                {
                    return steps;
                }

                var cleaned = StepPrefixRegex().Replace(line, string.Empty).Trim();
                if (cleaned.Length > 0)
                {
                    steps.Add(cleaned);
                }
            }
        }

        return steps;
    }

    private static bool ShouldSkipIngredientLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return true;
        }

        if (line.StartsWith("Przygotowanie", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (line.EndsWith(':') && !Regex.IsMatch(line, "\\d", RegexOptions.CultureInvariant))
        {
            return true;
        }

        return false;
    }

    private static bool HasHeading(IElement element, string normalizedPrefix)
    {
        var text = NormalizeText(element.TextContent);
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return RecipeImportHtml
            .FoldToAscii(text)
            .StartsWith(normalizedPrefix, StringComparison.OrdinalIgnoreCase);
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

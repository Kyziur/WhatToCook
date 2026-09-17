using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace WhatToCook.Api.Features.Recipes.Imports;

public sealed class AniaGotujeRecipeImportSourceAdapter()
    : RecipeImportSourceAdapterBase("aniagotuje", priority: 100)
{
    private static readonly HtmlParser HtmlParser = new();

    private static readonly HashSet<string> IgnoredStepValues = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "Skladniki:",
        "Składniki:",
        "Kopiuj",
        "Ukryj zdjecia",
        "Ukryj zdjęcia",
        "Najczęściej zadawane pytania",
    };

    public override bool CanHandle(Uri sourceUri)
    {
        return MatchesHost(sourceUri, "aniagotuje.pl")
            || MatchesLoopbackFixture(sourceUri, "/import-fixtures/recipe-import/");
    }

    public override RecipeImportExtractionResult Extract(Uri sourceUri, string html)
    {
        var document = HtmlParser.ParseDocument(html);

        var title = ExtractTitle(document);
        var servingsText = ExtractServingsText(document);
        var ingredients = ExtractIngredients(document);
        var steps = ExtractSteps(document, title);

        if (string.IsNullOrWhiteSpace(title) || ingredients.Count == 0 || steps.Count == 0)
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
        );
    }

    private static string? ExtractServingsText(IHtmlDocument document)
    {
        var recipeYield = NormalizeText(
            document.QuerySelector("meta[itemprop='recipeYield']")?.GetAttribute("content")
        );
        if (!string.IsNullOrWhiteSpace(recipeYield))
        {
            return recipeYield;
        }

        var recipeInfoText = NormalizeText(document.QuerySelector("p.recipe-info")?.TextContent);
        if (
            !string.IsNullOrWhiteSpace(recipeInfoText)
            && recipeInfoText.Contains("Liczba porcji", StringComparison.OrdinalIgnoreCase)
        )
        {
            return recipeInfoText;
        }

        var servingsLabelText = document
            .Body?.QuerySelectorAll("p, div, span, li, strong")
            .Select(x => NormalizeText(x.TextContent))
            .FirstOrDefault(x =>
                !string.IsNullOrWhiteSpace(x)
                && x.Contains("Liczba porcji", StringComparison.OrdinalIgnoreCase)
            );
        if (!string.IsNullOrWhiteSpace(servingsLabelText))
        {
            return servingsLabelText;
        }

        var bodyText = NormalizeText(document.Body?.TextContent);
        if (
            !string.IsNullOrWhiteSpace(bodyText)
            && bodyText.Contains("Liczba porcji", StringComparison.OrdinalIgnoreCase)
        )
        {
            return bodyText;
        }

        return null;
    }

    private static List<RecipeImportIngredientRequest> ExtractIngredients(IHtmlDocument document)
    {
        var container = document.QuerySelector("#recipeIngredients, #ingredients");
        if (container is null)
        {
            return [];
        }

        return container
            .QuerySelectorAll("li")
            .Select(ParseIngredient)
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .ToList();
    }

    private static List<string> ExtractSteps(IHtmlDocument document, string? title)
    {
        var listSteps = document
            .QuerySelectorAll("#steps ol li")
            .Select(x => NormalizeText(x.TextContent))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .ToList();
        if (listSteps.Count > 0)
        {
            return FilterSteps(listSteps, title, allowShortSteps: true);
        }

        var articleContent = document.QuerySelector(".article-content") ?? document.Body;
        if (articleContent is null)
        {
            return [];
        }

        var textBlocks = new List<string>();
        foreach (var scope in EnumerateStepScopes(articleContent))
        {
            var clone = scope.Clone(deep: true) as IElement;
            if (clone is null)
            {
                continue;
            }

            RemoveStepNoise(clone);

            foreach (var block in clone.QuerySelectorAll("p, h2, h3, h4, li"))
            {
                var text = NormalizeText(block.TextContent);
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                if (text.Equals("Najczęściej zadawane pytania", StringComparison.OrdinalIgnoreCase))
                {
                    return FilterSteps(textBlocks, title);
                }

                textBlocks.Add(text);
            }
        }

        return FilterSteps(textBlocks, title);
    }

    private static IEnumerable<IElement> EnumerateStepScopes(IElement articleContent)
    {
        var contentBody = articleContent.QuerySelector(
            "[itemprop='recipeInstructions'], .article-content-body"
        );
        if (contentBody is null)
        {
            yield return articleContent;
            yield break;
        }

        yield return contentBody;

        for (
            var sibling = contentBody.NextElementSibling;
            sibling is not null;
            sibling = sibling.NextElementSibling
        )
        {
            if (string.Equals(sibling.Id, "vote", StringComparison.OrdinalIgnoreCase))
            {
                yield break;
            }

            yield return sibling;
        }
    }

    private static void RemoveStepNoise(IElement scope)
    {
        foreach (
            var selector in new[]
            {
                "#recipeIngredients",
                "#ingredients",
                ".article-intro",
                "p.recipe-info",
                "[data-nosnippet]",
                "script",
                "style",
                "img",
                ".img-placeholder",
                ".copy-share-lock-con",
                ".share-ingredients",
                ".wake-photo-con",
                ".newsletter-box",
                "#faq",
            }
        )
        {
            foreach (var node in scope.QuerySelectorAll(selector).ToArray())
            {
                node.Remove();
            }
        }
    }

    private static List<string> FilterSteps(
        IEnumerable<string> values,
        string? title,
        bool allowShortSteps = false
    )
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        return values
            .Select(RecipeImportHtml.NormalizeWhitespace)
            .Where(x => IsUsefulStep(x, title, allowShortSteps))
            .Where(seen.Add)
            .ToList();
    }

    private static RecipeImportIngredientRequest ParseIngredient(IElement item)
    {
        var ingredientNode = item.QuerySelector("[itemprop='recipeIngredient']") ?? item;
        var name = NormalizeText(ingredientNode.QuerySelector(".ingredient")?.TextContent);
        var quantityValue = NormalizeText(ingredientNode.QuerySelector(".qty")?.TextContent);

        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(quantityValue))
        {
            return RecipeImportIngredientParser.ParseIngredientText(
                NormalizeText(ingredientNode.TextContent) ?? string.Empty
            );
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return new RecipeImportIngredientRequest { Name = quantityValue ?? string.Empty };
        }

        if (string.IsNullOrWhiteSpace(quantityValue))
        {
            return RecipeImportIngredientParser.ParseIngredientText(name);
        }

        var quantityParts = RecipeImportIngredientParser.ParseQuantityParts(quantityValue);
        return new RecipeImportIngredientRequest
        {
            Name = name,
            QuantityText = quantityParts.QuantityText,
            Unit = quantityParts.Unit,
        };
    }

    private static bool IsUsefulStep(string value, string? title, bool allowShortSteps)
    {
        if (!allowShortSteps && value.Length < 20)
        {
            return false;
        }

        if (allowShortSteps && value.Length < 5)
        {
            return false;
        }

        if (
            !string.IsNullOrWhiteSpace(title)
            && value.Equals(title, StringComparison.OrdinalIgnoreCase)
        )
        {
            return false;
        }

        if (value.StartsWith("Tryb gotowania", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return !IgnoredStepValues.Contains(value);
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
}

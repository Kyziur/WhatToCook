using WhatToCook.Api.Domain.Recipes;

namespace WhatToCook.Api.Features.Recipes.Shared;

public static class RecipeMappings
{
    public static RecipeSummaryResponse ToSummaryResponse(this Recipe recipe)
    {
        return new RecipeSummaryResponse(
            recipe.Id,
            recipe.Title,
            recipe.Servings,
            recipe.Source,
            recipe.MainPhotoPath,
            recipe.IsActive,
            recipe.RecipeTags.Select(x => x.Tag.DisplayName).Order().ToArray(),
            recipe
                .Ingredients.OrderBy(x => x.SortOrder)
                .Select(x => x.Ingredient.DisplayName)
                .ToArray()
        );
    }

    public static RecipeDetailsResponse ToDetailsResponse(this Recipe recipe)
    {
        return new RecipeDetailsResponse(
            recipe.Id,
            recipe.Title,
            recipe.Servings,
            recipe.Source,
            recipe.MainPhotoPath,
            recipe.IsActive,
            recipe.RecipeTags.Select(x => x.Tag.DisplayName).Order().ToArray(),
            recipe
                .Ingredients.OrderBy(x => x.SortOrder)
                .Select(x => new RecipeIngredientResponse(
                    x.Ingredient.DisplayName,
                    x.QuantityText,
                    x.Unit
                ))
                .ToArray(),
            recipe.Steps.OrderBy(x => x.SortOrder).Select(x => x.Text).ToArray()
        );
    }
}

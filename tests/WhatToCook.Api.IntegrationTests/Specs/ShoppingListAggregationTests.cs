using WhatToCook.Api.Domain.Planning;
using WhatToCook.Api.Domain.Recipes;
using WhatToCook.Api.Features.Shopping.Lists;

namespace WhatToCook.Api.IntegrationTests.Specs;

public sealed class ShoppingListAggregationTests
{
    [Fact]
    public void Aggregation_IsPureAndKeepsNumericDescriptiveAndNameOnlyAmounts()
    {
        var entries = new[]
        {
            Entry(
                0,
                2m,
                ("Pomidor", "2", "szt"),
                ("Sól", "szczypta", null),
                ("Pieprz", null, null)
            ),
            Entry(
                1,
                1m,
                ("Pomidor", "3", "szt"),
                ("Sól", "szczypta", null),
                ("Pieprz", null, null)
            ),
        };

        var shoppingListId = Guid.NewGuid();
        var items = ShoppingListAggregation.BuildGeneratedItems(shoppingListId, entries);

        Assert.Equal(3, items.Count);
        Assert.All(items, item =>
        {
            Assert.Equal(shoppingListId, item.ShoppingListId);
            Assert.False(item.IsManual);
            Assert.Equal(WhatToCook.Api.Domain.Shopping.ChecklistState.NieMam, item.State);
        });

        var tomato = Assert.Single(items.Where(item => item.Name == "Pomidor"));
        Assert.Equal("7", tomato.QuantityText);
        Assert.Equal("szt", tomato.Unit);
        Assert.Null(tomato.QuantityContext);

        var salt = Assert.Single(items.Where(item => item.Name == "Sól"));
        Assert.Equal("szczypta", salt.QuantityText);
        Assert.Equal("wystąpienia: 2; suma mnożników: ×3", salt.QuantityContext);

        var pepper = Assert.Single(items.Where(item => item.Name == "Pieprz"));
        Assert.Null(pepper.QuantityText);
        Assert.Equal("wystąpienia: 2; suma mnożników: ×3", pepper.QuantityContext);
        Assert.Equal(new[] { 0, 1, 2 }, items.Select(item => item.SortOrder));
    }

    [Fact]
    public void Aggregation_DoesNotMergeDifferentUnitsAndParsesCommaDecimal()
    {
        var entries = new[]
        {
            Entry(0, 1m, ("Mąka", "1,5", "kg"), ("Mąka", "250", "g")),
            Entry(1, 1m, ("Mąka", "0.5", "kg")),
        };

        var items = ShoppingListAggregation.BuildGeneratedItems(Guid.NewGuid(), entries);
        Assert.Equal(2, items.Count);
        Assert.Equal("2", Assert.Single(items.Where(item => item.Unit == "kg")).QuantityText);
        Assert.Equal("250", Assert.Single(items.Where(item => item.Unit == "g")).QuantityText);
    }

    private static PlannedRecipe Entry(
        int sortOrder,
        decimal multiplier,
        params (string Name, string? Quantity, string? Unit)[] ingredients
    ) =>
        new()
        {
            PlannedDate = new DateOnly(2026, 6, 1),
            SortOrder = sortOrder,
            Multiplier = multiplier,
            Recipe = new Recipe
            {
                Ingredients = ingredients
                    .Select((ingredient, index) => new RecipeIngredient
                    {
                        SortOrder = index,
                        Ingredient = new Ingredient
                        {
                            DisplayName = ingredient.Name,
                            NormalizedName = ingredient.Name.ToLowerInvariant(),
                        },
                        QuantityText = ingredient.Quantity,
                        Unit = ingredient.Unit,
                    })
                    .ToList(),
            },
        };
}

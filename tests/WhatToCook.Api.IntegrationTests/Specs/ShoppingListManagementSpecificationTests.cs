using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace WhatToCook.Api.IntegrationTests.Specs;

public sealed class ShoppingListManagementSpecificationTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public async Task GenerateShoppingListOnDemand_ShouldNotExistBeforeGeneration()
    {
        using var client = factory.CreateClient();
        var recipe = await CreateRecipe(
            client,
            $"OnDemand {Guid.NewGuid():N}"[..24],
            [new RecipeApiContracts.RecipeIngredientPayload("Pomidor", "2", "szt")]
        );
        var plan = await CreatePlan(
            client,
            "Plan OnDemand",
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 7)
        );
        await SavePlanWithSingleEntry(client, plan, recipe.Id, 1m, new DateOnly(2026, 6, 1));

        var beforeGenerate = await client.GetAsync($"/api/shopping-lists/plan/{plan.Id}");
        Assert.Equal(HttpStatusCode.NotFound, beforeGenerate.StatusCode);

        var generateResponse = await client.PostAsync(
            $"/api/plans/{plan.Id}/shopping-generated",
            null
        );
        Assert.Equal(HttpStatusCode.NoContent, generateResponse.StatusCode);

        var generated = await client.GetFromJsonAsync<ShoppingListDto>(
            $"/api/shopping-lists/plan/{plan.Id}",
            JsonOptions
        );
        Assert.NotNull(generated);
        Assert.NotEmpty(generated!.Items);
    }

    [Fact]
    public async Task AggregateMatchingIngredientAndUnit_ShouldMergeToSingleItem()
    {
        using var client = factory.CreateClient();
        var recipeA = await CreateRecipe(
            client,
            $"AggrA {Guid.NewGuid():N}"[..24],
            [new RecipeApiContracts.RecipeIngredientPayload("Pomidor", "2", "szt")]
        );
        var recipeB = await CreateRecipe(
            client,
            $"AggrB {Guid.NewGuid():N}"[..24],
            [new RecipeApiContracts.RecipeIngredientPayload("Pomidor", "3", "szt")]
        );
        var plan = await CreatePlan(
            client,
            "Plan agregacja",
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 7)
        );

        await SavePlan(
            client,
            plan.Id,
            new SavePlanPayload
            {
                Name = plan.Name,
                DateFrom = plan.DateFrom,
                DateTo = plan.DateTo,
                Entries =
                [
                    new SavePlanEntryPayload
                    {
                        RecipeId = recipeA.Id,
                        PlannedDate = new DateOnly(2026, 6, 1),
                        Multiplier = 1m,
                        SortOrder = 0,
                    },
                    new SavePlanEntryPayload
                    {
                        RecipeId = recipeB.Id,
                        PlannedDate = new DateOnly(2026, 6, 1),
                        Multiplier = 1m,
                        SortOrder = 1,
                    },
                ],
                RegenerateShoppingList = false,
            }
        );

        await client.PostAsync($"/api/plans/{plan.Id}/shopping-generated", null);
        var generated = await client.GetFromJsonAsync<ShoppingListDto>(
            $"/api/shopping-lists/plan/{plan.Id}",
            JsonOptions
        );

        Assert.NotNull(generated);
        var tomatoItems = generated!
            .Items.Where(x => x.Name == "Pomidor" && x.Unit == "szt")
            .ToList();
        Assert.Single(tomatoItems);
        Assert.Equal("5", tomatoItems[0].QuantityText);
    }

    [Fact]
    public async Task KeepSeparateItemsForDifferentUnits_ShouldNotMerge()
    {
        using var client = factory.CreateClient();
        var recipeKg = await CreateRecipe(
            client,
            $"UnitKg {Guid.NewGuid():N}"[..24],
            [new RecipeApiContracts.RecipeIngredientPayload("Pomidor", "1", "kg")]
        );
        var recipePiece = await CreateRecipe(
            client,
            $"UnitSzt {Guid.NewGuid():N}"[..24],
            [new RecipeApiContracts.RecipeIngredientPayload("Pomidor", "2", "szt")]
        );
        var plan = await CreatePlan(
            client,
            "Plan units",
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 7)
        );

        await SavePlan(
            client,
            plan.Id,
            new SavePlanPayload
            {
                Name = plan.Name,
                DateFrom = plan.DateFrom,
                DateTo = plan.DateTo,
                Entries =
                [
                    new SavePlanEntryPayload
                    {
                        RecipeId = recipeKg.Id,
                        PlannedDate = new DateOnly(2026, 6, 1),
                        Multiplier = 1m,
                        SortOrder = 0,
                    },
                    new SavePlanEntryPayload
                    {
                        RecipeId = recipePiece.Id,
                        PlannedDate = new DateOnly(2026, 6, 2),
                        Multiplier = 1m,
                        SortOrder = 0,
                    },
                ],
                RegenerateShoppingList = false,
            }
        );

        await client.PostAsync($"/api/plans/{plan.Id}/shopping-generated", null);
        var generated = await client.GetFromJsonAsync<ShoppingListDto>(
            $"/api/shopping-lists/plan/{plan.Id}",
            JsonOptions
        );

        Assert.NotNull(generated);
        Assert.Contains(generated!.Items, x => x.Name == "Pomidor" && x.Unit == "kg");
        Assert.Contains(generated.Items, x => x.Name == "Pomidor" && x.Unit == "szt");
    }

    [Fact]
    public async Task IngredientWithoutQuantity_ShouldBeListedAsNameOnly()
    {
        using var client = factory.CreateClient();
        var recipe = await CreateRecipe(
            client,
            $"NoQty {Guid.NewGuid():N}"[..24],
            [new RecipeApiContracts.RecipeIngredientPayload("Sol do smaku", null, null)]
        );
        var plan = await CreatePlan(
            client,
            "Plan no qty",
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 7)
        );
        await SavePlanWithSingleEntry(client, plan, recipe.Id, 1m, new DateOnly(2026, 6, 1));

        await client.PostAsync($"/api/plans/{plan.Id}/shopping-generated", null);
        var generated = await client.GetFromJsonAsync<ShoppingListDto>(
            $"/api/shopping-lists/plan/{plan.Id}",
            JsonOptions
        );

        Assert.NotNull(generated);
        Assert.Contains(
            generated!.Items,
            x => x.Name == "Sol do smaku" && x.QuantityText is null && x.Unit is null
        );
    }

    [Fact]
    public async Task ChecklistAndManualItems_ShouldBeStored()
    {
        using var client = factory.CreateClient();
        var recipe = await CreateRecipe(
            client,
            $"Checklist {Guid.NewGuid():N}"[..24],
            [new RecipeApiContracts.RecipeIngredientPayload("Feta", "1", "opakowanie")]
        );
        var plan = await CreatePlan(
            client,
            "Plan checklist",
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 7)
        );
        await SavePlanWithSingleEntry(client, plan, recipe.Id, 1m, new DateOnly(2026, 6, 1));

        await client.PostAsync($"/api/plans/{plan.Id}/shopping-generated", null);
        var list = await client.GetFromJsonAsync<ShoppingListDto>(
            $"/api/shopping-lists/plan/{plan.Id}",
            JsonOptions
        );
        Assert.NotNull(list);

        var firstItem = list!.Items.First();
        var stateResponse = await client.PutAsJsonAsync(
            $"/api/shopping-lists/plan/{plan.Id}/items/{firstItem.Id}/state",
            new SetStatePayload { State = "MAM" }
        );
        Assert.Equal(HttpStatusCode.OK, stateResponse.StatusCode);

        var manualResponse = await client.PostAsJsonAsync(
            $"/api/shopping-lists/plan/{plan.Id}/manual-items",
            new ManualItemPayload { Text = "Papier do pieczenia" }
        );
        Assert.Equal(HttpStatusCode.OK, manualResponse.StatusCode);

        var afterUpdate = await manualResponse.Content.ReadFromJsonAsync<ShoppingListDto>(
            JsonOptions
        );
        Assert.NotNull(afterUpdate);
        Assert.Contains(afterUpdate!.Items, x => x.State == "MAM");
        Assert.Contains(afterUpdate.Items, x => x.IsManual && x.Name == "Papier do pieczenia");
    }

    [Fact]
    public async Task Recommendations_ShouldRankMatchedIngredientsAndExcludePlannedRecipes()
    {
        using var client = factory.CreateClient();
        var ingredientSuffix = Guid.NewGuid().ToString("N")[..8];
        var ownedTomato = $"Pomidor {ingredientSuffix}";
        var ownedOnion = $"Cebula {ingredientSuffix}";
        var plannedTomato = await CreateRecipe(
            client,
            $"Planned tomato {Guid.NewGuid():N}"[..24],
            [new RecipeApiContracts.RecipeIngredientPayload(ownedTomato, "1", "szt")]
        );
        var plannedOnion = await CreateRecipe(
            client,
            $"Planned onion {Guid.NewGuid():N}"[..24],
            [new RecipeApiContracts.RecipeIngredientPayload(ownedOnion, "1", "szt")]
        );
        var twoMatches = await CreateRecipe(
            client,
            $"Two matches {Guid.NewGuid():N}"[..24],
            [
                new RecipeApiContracts.RecipeIngredientPayload(ownedTomato, "1", "szt"),
                new RecipeApiContracts.RecipeIngredientPayload(ownedOnion, "1", "szt"),
            ]
        );
        var oneMatch = await CreateRecipe(
            client,
            $"One match {Guid.NewGuid():N}"[..24],
            [new RecipeApiContracts.RecipeIngredientPayload(ownedTomato, "1", "szt")]
        );
        var plan = await CreatePlan(
            client,
            "Plan rekomendacji",
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 7)
        );

        await SavePlan(
            client,
            plan.Id,
            new SavePlanPayload
            {
                Name = plan.Name,
                DateFrom = plan.DateFrom,
                DateTo = plan.DateTo,
                Entries =
                [
                    new SavePlanEntryPayload
                    {
                        RecipeId = plannedTomato.Id,
                        PlannedDate = new DateOnly(2026, 6, 1),
                        Multiplier = 1m,
                        SortOrder = 0,
                    },
                    new SavePlanEntryPayload
                    {
                        RecipeId = plannedOnion.Id,
                        PlannedDate = new DateOnly(2026, 6, 2),
                        Multiplier = 1m,
                        SortOrder = 0,
                    },
                ],
                RegenerateShoppingList = false,
            }
        );
        await client.PostAsync($"/api/plans/{plan.Id}/shopping-generated", null);
        var list = await client.GetFromJsonAsync<ShoppingListDto>(
            $"/api/shopping-lists/plan/{plan.Id}",
            JsonOptions
        );
        Assert.NotNull(list);

        foreach (var item in list!.Items)
        {
            var response = await client.PutAsJsonAsync(
                $"/api/shopping-lists/plan/{plan.Id}/items/{item.Id}/state",
                new SetStatePayload { State = "MAM" }
            );
            response.EnsureSuccessStatusCode();
        }

        var recommendations = await client.GetFromJsonAsync<RecipeRecommendationsDto>(
            $"/api/shopping-lists/plan/{plan.Id}/recommendations",
            JsonOptions
        );

        Assert.NotNull(recommendations);
        Assert.Equal(twoMatches.Id, recommendations!.Items[0].RecipeId);
        Assert.Equal(2, recommendations.Items[0].MatchedIngredientCount);
        Assert.Contains(recommendations.Items, x => x.RecipeId == oneMatch.Id);
        Assert.DoesNotContain(
            recommendations.Items,
            x => x.RecipeId == plannedTomato.Id || x.RecipeId == plannedOnion.Id
        );
    }

    [Fact]
    public async Task Recommendations_ShouldApplyThirtyPercentCoverageBoundary()
    {
        using var client = factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var owned = Enumerable
            .Range(1, 10)
            .Select(index => $"Boundary owned {suffix} {index}")
            .ToArray();
        static RecipeApiContracts.RecipeIngredientPayload Ingredient(string name) =>
            new(name, "1", "szt");

        var planned = await CreateRecipe(
            client,
            $"Boundary planned {Guid.NewGuid():N}"[..24],
            owned.Select(Ingredient).ToList()
        );
        var exact = await CreateRecipe(
            client,
            $"Boundary exact {Guid.NewGuid():N}"[..24],
            [
                Ingredient(owned[0]),
                Ingredient(owned[1]),
                Ingredient(owned[2]),
                Ingredient($"Boundary exact missing {suffix} 1"),
                Ingredient($"Boundary exact missing {suffix} 2"),
                Ingredient($"Boundary exact missing {suffix} 3"),
                Ingredient($"Boundary exact missing {suffix} 4"),
                Ingredient($"Boundary exact missing {suffix} 5"),
                Ingredient($"Boundary exact missing {suffix} 6"),
                Ingredient($"Boundary exact missing {suffix} 7"),
            ]
        );
        var below = await CreateRecipe(
            client,
            $"Boundary below {Guid.NewGuid():N}"[..24],
            [
                Ingredient(owned[0]),
                Ingredient(owned[1]),
                Ingredient($"Boundary below missing {suffix} 1"),
                Ingredient($"Boundary below missing {suffix} 2"),
                Ingredient($"Boundary below missing {suffix} 3"),
                Ingredient($"Boundary below missing {suffix} 4"),
                Ingredient($"Boundary below missing {suffix} 5"),
            ]
        );
        var above = await CreateRecipe(
            client,
            $"Boundary above {Guid.NewGuid():N}"[..24],
            [
                Ingredient(owned[0]),
                Ingredient(owned[1]),
                Ingredient(owned[2]),
                Ingredient(owned[3]),
                Ingredient($"Boundary above missing {suffix} 1"),
                Ingredient($"Boundary above missing {suffix} 2"),
                Ingredient($"Boundary above missing {suffix} 3"),
            ]
        );
        var plan = await CreatePlan(
            client,
            $"Boundary plan {suffix}",
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 7)
        );

        await SavePlanWithSingleEntry(client, plan, planned.Id, 1m, plan.DateFrom);
        await client.PostAsync($"/api/plans/{plan.Id}/shopping-generated", null);
        var list = await client.GetFromJsonAsync<ShoppingListDto>(
            $"/api/shopping-lists/plan/{plan.Id}",
            JsonOptions
        );
        Assert.NotNull(list);
        Assert.Equal(10, list!.Items.Count);

        foreach (var item in list.Items)
        {
            var response = await client.PutAsJsonAsync(
                $"/api/shopping-lists/plan/{plan.Id}/items/{item.Id}/state",
                new SetStatePayload { State = "MAM" }
            );
            response.EnsureSuccessStatusCode();
        }

        var recommendations = await client.GetFromJsonAsync<RecipeRecommendationsDto>(
            $"/api/shopping-lists/plan/{plan.Id}/recommendations",
            JsonOptions
        );

        Assert.NotNull(recommendations);
        var exactRecommendation = Assert.Single(
            recommendations!.Items,
            item => item.RecipeId == exact.Id
        );
        Assert.Equal(3, exactRecommendation.MatchedIngredientCount);
        Assert.Equal(10, exactRecommendation.TotalIngredientCount);

        var aboveRecommendation = Assert.Single(
            recommendations.Items,
            item => item.RecipeId == above.Id
        );
        Assert.Equal(4, aboveRecommendation.MatchedIngredientCount);
        Assert.Equal(7, aboveRecommendation.TotalIngredientCount);
        Assert.DoesNotContain(recommendations.Items, item => item.RecipeId == below.Id);
        Assert.DoesNotContain(recommendations.Items, item => item.RecipeId == planned.Id);
    }

    [Fact]
    public async Task RegenerationAndShoppingStateChange_ShouldBothComplete()
    {
        using var client = factory.CreateClient();
        var recipe = await CreateRecipe(
            client,
            $"Lock {Guid.NewGuid():N}"[..24],
            [new RecipeApiContracts.RecipeIngredientPayload("Pomidor", "1", "szt")]
        );
        var plan = await CreatePlan(
            client,
            "Plan blokady",
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 7)
        );
        var saved = await SavePlanWithSingleEntry(
            client,
            plan,
            recipe.Id,
            1m,
            new DateOnly(2026, 6, 1)
        );
        await client.PostAsync($"/api/plans/{plan.Id}/shopping-generated", null);
        var list = await client.GetFromJsonAsync<ShoppingListDto>(
            $"/api/shopping-lists/plan/{plan.Id}",
            JsonOptions
        );
        Assert.NotNull(list);

        var responses = await Task.WhenAll(
            client.PutAsJsonAsync(
                $"/api/shopping-lists/plan/{plan.Id}/items/{list!.Items.Single().Id}/state",
                new SetStatePayload { State = "MAM" }
            ),
            client.PutAsJsonAsync(
                $"/api/plans/{plan.Id}",
                new SavePlanPayload
                {
                    Name = saved.Name,
                    DateFrom = saved.DateFrom,
                    DateTo = saved.DateTo,
                    RegenerateShoppingList = true,
                    Entries =
                    [
                        new SavePlanEntryPayload
                        {
                            Id = saved.Entries.Single().Id,
                            RecipeId = recipe.Id,
                            PlannedDate = new DateOnly(2026, 6, 1),
                            Multiplier = 1m,
                            SortOrder = 0,
                        },
                    ],
                }
            )
        );

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));
        var finalList = await client.GetFromJsonAsync<ShoppingListDto>(
            $"/api/shopping-lists/plan/{plan.Id}",
            JsonOptions
        );
        Assert.NotNull(finalList);
        Assert.Equal("MAM", finalList!.Items.Single().State);
    }

    [Fact]
    public async Task SavingNewPlanEntry_ShouldRegenerateShoppingList()
    {
        using var client = factory.CreateClient();
        var recipe = await CreateRecipe(
            client,
            $"Save {Guid.NewGuid():N}"[..24],
            [new RecipeApiContracts.RecipeIngredientPayload("Pomidor", "1", "szt")]
        );
        var plan = await CreatePlan(
            client,
            "Plan zapisu",
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 7)
        );

        var response = await client.PutAsJsonAsync(
            $"/api/plans/{plan.Id}",
            new SavePlanPayload
            {
                Name = plan.Name,
                DateFrom = plan.DateFrom,
                DateTo = plan.DateTo,
                RegenerateShoppingList = true,
                Entries =
                [
                    new SavePlanEntryPayload
                    {
                        RecipeId = recipe.Id,
                        PlannedDate = new DateOnly(2026, 6, 1),
                        Multiplier = 1m,
                        SortOrder = 0,
                    },
                ],
            }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var shoppingList = await client.GetFromJsonAsync<ShoppingListDto>(
            $"/api/shopping-lists/plan/{plan.Id}",
            JsonOptions
        );
        Assert.NotNull(shoppingList);
        Assert.Contains(
            shoppingList!.Items,
            item => item is { Name: "Pomidor", QuantityText: "1", Unit: "szt" }
        );
    }

    [Fact]
    public async Task Regeneration_ShouldPreserveMatchingStateAndManualDuplicates()
    {
        using var client = factory.CreateClient();
        var recipe = await CreateRecipe(
            client,
            $"Regen {Guid.NewGuid():N}"[..24],
            [
                new RecipeApiContracts.RecipeIngredientPayload("Pomidor", "1", "szt"),
                new RecipeApiContracts.RecipeIngredientPayload("Ogórek", "1", "szt"),
            ]
        );
        var replacementRecipe = await CreateRecipe(
            client,
            $"Regen replacement {Guid.NewGuid():N}"[..24],
            [new RecipeApiContracts.RecipeIngredientPayload("Pomidor", "1", "szt")]
        );
        var plan = await CreatePlan(
            client,
            "Plan regeneracja",
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 7)
        );
        var saved = await SavePlanWithSingleEntry(
            client,
            plan,
            recipe.Id,
            1m,
            new DateOnly(2026, 6, 1)
        );

        await client.PostAsync($"/api/plans/{plan.Id}/shopping-generated", null);
        var list = await client.GetFromJsonAsync<ShoppingListDto>(
            $"/api/shopping-lists/plan/{plan.Id}",
            JsonOptions
        );
        Assert.NotNull(list);

        var firstItem = list!.Items.Single(x => x.Name == "Pomidor");
        await client.PutAsJsonAsync(
            $"/api/shopping-lists/plan/{plan.Id}/items/{firstItem.Id}/state",
            new SetStatePayload { State = "MAM" }
        );
        await client.PostAsJsonAsync(
            $"/api/shopping-lists/plan/{plan.Id}/manual-items",
            new ManualItemPayload { Text = "Mleko" }
        );
        await client.PostAsJsonAsync(
            $"/api/shopping-lists/plan/{plan.Id}/manual-items",
            new ManualItemPayload { Text = "Mleko" }
        );

        var existingEntry = saved.Entries.Single();
        var updatedPlan = await SavePlan(
            client,
            plan.Id,
            new SavePlanPayload
            {
                Name = saved.Name,
                DateFrom = saved.DateFrom,
                DateTo = saved.DateTo,
                RegenerateShoppingList = true,
                Entries =
                [
                    new SavePlanEntryPayload
                    {
                        Id = existingEntry.Id,
                        RecipeId = replacementRecipe.Id,
                        PlannedDate = existingEntry.PlannedDate,
                        Multiplier = 2m,
                        SortOrder = 0,
                    },
                ],
            }
        );
        Assert.NotNull(updatedPlan);

        var regenerated = await client.GetFromJsonAsync<ShoppingListDto>(
            $"/api/shopping-lists/plan/{plan.Id}",
            JsonOptions
        );
        Assert.NotNull(regenerated);
        Assert.Equal(2, regenerated!.Items.Count(x => x.IsManual && x.Name == "Mleko"));
        Assert.Contains(regenerated.Items, x => x.Name == "Pomidor" && x.State == "MAM");
        Assert.Contains(regenerated.Items, x => x.Name == "Pomidor" && x.QuantityText == "2");
        Assert.DoesNotContain(regenerated.Items, x => x.Name == "Ogórek");
    }

    [Fact]
    public async Task RegenerationWithNewIngredients_ShouldPreserveExistingItemIdentityAndState()
    {
        using var client = factory.CreateClient();
        var initialRecipe = await CreateRecipe(
            client,
            $"Expand initial {Guid.NewGuid():N}"[..24],
            [new RecipeApiContracts.RecipeIngredientPayload("Risotto", "1", "opakowanie")]
        );
        var addedRecipe = await CreateRecipe(
            client,
            $"Expand added {Guid.NewGuid():N}"[..24],
            [new RecipeApiContracts.RecipeIngredientPayload("Curry", "2", "g")]
        );
        var plan = await CreatePlan(
            client,
            "Plan expanding ingredients",
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 7)
        );

        var saved = await SavePlanWithSingleEntry(
            client,
            plan,
            initialRecipe.Id,
            1m,
            new DateOnly(2026, 6, 1)
        );
        await client.PostAsync($"/api/plans/{plan.Id}/shopping-generated", null);

        var initialList = await client.GetFromJsonAsync<ShoppingListDto>(
            $"/api/shopping-lists/plan/{plan.Id}",
            JsonOptions
        );
        Assert.NotNull(initialList);
        var initialItem = initialList!.Items.Single(x => x.Name == "Risotto");
        var stateResponse = await client.PutAsJsonAsync(
            $"/api/shopping-lists/plan/{plan.Id}/items/{initialItem.Id}/state",
            new SetStatePayload { State = "MAM" }
        );
        Assert.Equal(HttpStatusCode.OK, stateResponse.StatusCode);

        var manualResponse = await client.PostAsJsonAsync(
            $"/api/shopping-lists/plan/{plan.Id}/manual-items",
            new ManualItemPayload { Text = "Papier do pieczenia" }
        );
        Assert.Equal(HttpStatusCode.OK, manualResponse.StatusCode);
        var listWithManual = await manualResponse.Content.ReadFromJsonAsync<ShoppingListDto>(
            JsonOptions
        );
        Assert.NotNull(listWithManual);
        var manualItem = listWithManual!.Items.Single(x => x.IsManual);

        var existingEntry = saved.Entries.Single();
        var updatedPlan = await SavePlan(
            client,
            plan.Id,
            new SavePlanPayload
            {
                Name = saved.Name,
                DateFrom = saved.DateFrom,
                DateTo = saved.DateTo,
                RegenerateShoppingList = true,
                Entries =
                [
                    new SavePlanEntryPayload
                    {
                        Id = existingEntry.Id,
                        RecipeId = initialRecipe.Id,
                        PlannedDate = existingEntry.PlannedDate,
                        Multiplier = 1m,
                        SortOrder = 0,
                    },
                    new SavePlanEntryPayload
                    {
                        RecipeId = addedRecipe.Id,
                        PlannedDate = new DateOnly(2026, 6, 2),
                        Multiplier = 1m,
                        SortOrder = 1,
                    },
                ],
            }
        );

        Assert.Equal(2, updatedPlan.Entries.Count);
        Assert.Contains(updatedPlan.Entries, entry => entry.RecipeId == initialRecipe.Id);
        Assert.Contains(updatedPlan.Entries, entry => entry.RecipeId == addedRecipe.Id);
        var regenerated = await client.GetFromJsonAsync<ShoppingListDto>(
            $"/api/shopping-lists/plan/{plan.Id}",
            JsonOptions
        );
        Assert.NotNull(regenerated);
        var retainedItem = regenerated!.Items.Single(x => x.Name == "Risotto");
        Assert.Equal(initialItem.Id, retainedItem.Id);
        Assert.Equal("MAM", retainedItem.State);
        var newItem = regenerated.Items.Single(x => x.Name == "Curry");
        Assert.NotEqual(initialItem.Id, newItem.Id);
        var retainedManual = regenerated.Items.Single(x => x.IsManual);
        Assert.Equal(manualItem.Id, retainedManual.Id);
    }

    [Fact]
    public async Task CopyText_ShouldStartWithMealPlanNameAndContainItems()
    {
        using var client = factory.CreateClient();
        var recipe = await CreateRecipe(
            client,
            $"Copy {Guid.NewGuid():N}"[..24],
            [new RecipeApiContracts.RecipeIngredientPayload("Pomidor", "2", "szt")]
        );
        var plan = await CreatePlan(
            client,
            "Plan kopiowania",
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 7)
        );
        await SavePlanWithSingleEntry(client, plan, recipe.Id, 1m, new DateOnly(2026, 6, 1));
        await client.PostAsync($"/api/plans/{plan.Id}/shopping-generated", null);

        var response = await client.GetFromJsonAsync<CopyTextDto>(
            $"/api/shopping-lists/plan/{plan.Id}/copy-text",
            JsonOptions
        );
        Assert.NotNull(response);
        Assert.StartsWith("Lista zakupów - Plan kopiowania", response!.Text);
        Assert.Contains("2 szt Pomidor", response.Text);
    }

    private static async Task<RecipeApiContracts.RecipeDetailsDto> CreateRecipe(
        HttpClient client,
        string title,
        List<RecipeApiContracts.RecipeIngredientPayload> ingredients
    )
    {
        var payload = RecipeApiContracts.CreateRecipePayload(title);
        payload.Ingredients = ingredients;
        var response = await client.PostAsJsonAsync("/api/recipes", payload);
        response.EnsureSuccessStatusCode();
        var recipe = await response.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeDetailsDto>(
            JsonOptions
        );
        return recipe!;
    }

    private static async Task<PlanDetailsDto> CreatePlan(
        HttpClient client,
        string name,
        DateOnly from,
        DateOnly to
    )
    {
        var response = await client.PostAsJsonAsync(
            "/api/plans",
            new CreatePlanPayload
            {
                Name = name,
                DateFrom = from,
                DateTo = to,
            }
        );
        response.EnsureSuccessStatusCode();
        var plan = await response.Content.ReadFromJsonAsync<PlanDetailsDto>(JsonOptions);
        return plan!;
    }

    private static Task<PlanDetailsDto> SavePlanWithSingleEntry(
        HttpClient client,
        PlanDetailsDto plan,
        Guid recipeId,
        decimal multiplier,
        DateOnly date
    ) =>
        SavePlan(
            client,
            plan.Id,
            new SavePlanPayload
            {
                Name = plan.Name,
                DateFrom = plan.DateFrom,
                DateTo = plan.DateTo,
                Entries =
                [
                    new SavePlanEntryPayload
                    {
                        RecipeId = recipeId,
                        PlannedDate = date,
                        Multiplier = multiplier,
                        SortOrder = 0,
                    },
                ],
                RegenerateShoppingList = false,
            }
        );

    private static async Task<PlanDetailsDto> SavePlan(
        HttpClient client,
        Guid planId,
        SavePlanPayload payload
    )
    {
        var response = await client.PutAsJsonAsync($"/api/plans/{planId}", payload);
        response.EnsureSuccessStatusCode();
        var plan = await response.Content.ReadFromJsonAsync<PlanDetailsDto>(JsonOptions);
        return plan!;
    }

    private sealed class CreatePlanPayload
    {
        public string Name { get; set; } = string.Empty;
        public DateOnly DateFrom { get; set; }
        public DateOnly DateTo { get; set; }
    }

    private sealed class SavePlanPayload
    {
        public string Name { get; set; } = string.Empty;
        public DateOnly DateFrom { get; set; }
        public DateOnly DateTo { get; set; }
        public bool RegenerateShoppingList { get; set; }
        public List<SavePlanEntryPayload> Entries { get; set; } = [];
    }

    private sealed class SavePlanEntryPayload
    {
        public Guid? Id { get; set; }
        public Guid RecipeId { get; set; }
        public DateOnly PlannedDate { get; set; }
        public decimal Multiplier { get; set; }
        public int SortOrder { get; set; }
    }

    private sealed class PlanDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly DateFrom { get; set; }
        public DateOnly DateTo { get; set; }
        public List<PlanEntryDto> Entries { get; set; } = [];
    }

    private sealed class PlanEntryDto
    {
        public Guid Id { get; set; }
        public Guid RecipeId { get; set; }
        public DateOnly PlannedDate { get; set; }
    }

    private sealed class ShoppingListDto
    {
        public Guid Id { get; set; }
        public Guid MealPlanId { get; set; }
        public string MealPlanName { get; set; } = string.Empty;
        public List<ShoppingItemDto> Items { get; set; } = [];
    }

    private sealed class ShoppingItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? QuantityText { get; set; }
        public string? Unit { get; set; }
        public string State { get; set; } = "NIE_MAM";
        public bool IsManual { get; set; }
    }

    private sealed class RecipeRecommendationsDto
    {
        public List<RecipeRecommendationDto> Items { get; set; } = [];
    }

    private sealed class RecipeRecommendationDto
    {
        public Guid RecipeId { get; set; }
        public int MatchedIngredientCount { get; set; }
        public int TotalIngredientCount { get; set; }
    }

    private sealed class SetStatePayload
    {
        public string State { get; set; } = "NIE_MAM";
    }

    private sealed class ManualItemPayload
    {
        public string Text { get; set; } = string.Empty;
    }

    private sealed class CopyTextDto
    {
        public string Text { get; set; } = string.Empty;
    }
}

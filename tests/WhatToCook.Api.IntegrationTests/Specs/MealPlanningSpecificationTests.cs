using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace WhatToCook.Api.IntegrationTests.Specs;

public sealed class MealPlanningSpecificationTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public async Task CreatePlanWithDefaultName_ShouldAllowEditingName()
    {
        using var client = factory.CreateClient();

        var created = await CreatePlan(
            client,
            null,
            new DateOnly(2026, 5, 1),
            new DateOnly(2026, 5, 7)
        );
        Assert.Contains("01.05 - 07.05", created.Name);

        var updated = await SavePlan(
            client,
            created.Id,
            new SavePlanPayload
            {
                Name = "Plan rodzinny",
                DateFrom = created.DateFrom,
                DateTo = created.DateTo,
                Entries = [],
                RegenerateShoppingList = false,
            }
        );

        Assert.Equal("Plan rodzinny", updated.Name);
    }

    [Fact]
    public async Task CreateOverlappingPlans_ShouldStoreAsSeparatePlans()
    {
        using var client = factory.CreateClient();

        await CreatePlan(client, "Plan A", new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 7));
        await CreatePlan(client, "Plan B", new DateOnly(2026, 5, 5), new DateOnly(2026, 5, 9));

        var plans = await client.GetFromJsonAsync<List<PlanSummaryDto>>("/api/plans", JsonOptions);
        Assert.NotNull(plans);
        Assert.Contains(plans!, x => x.Name == "Plan A");
        Assert.Contains(plans!, x => x.Name == "Plan B");
    }

    [Fact]
    public async Task AddRecipeToPlanDay_AndReuseAnotherDay_ShouldWork()
    {
        using var client = factory.CreateClient();
        var recipe = await CreateRecipe(client, "Planowany przepis");
        var plan = await CreatePlan(
            client,
            "Plan tygodnia",
            new DateOnly(2026, 5, 1),
            new DateOnly(2026, 5, 7)
        );

        var saved = await SavePlan(
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
                        RecipeId = recipe.Id,
                        PlannedDate = new DateOnly(2026, 5, 1),
                        Multiplier = 1,
                        SortOrder = 0,
                    },
                    new SavePlanEntryPayload
                    {
                        RecipeId = recipe.Id,
                        PlannedDate = new DateOnly(2026, 5, 2),
                        Multiplier = 1,
                        SortOrder = 0,
                    },
                ],
                RegenerateShoppingList = false,
            }
        );

        Assert.Equal(2, saved.Entries.Count);
        Assert.Contains(saved.Entries, x => x.PlannedDate == new DateOnly(2026, 5, 1));
        Assert.Contains(saved.Entries, x => x.PlannedDate == new DateOnly(2026, 5, 2));
    }

    [Fact]
    public async Task PreventDuplicateRecipeOnSameDay_ShouldReject()
    {
        using var client = factory.CreateClient();
        var recipe = await CreateRecipe(client, "Duplikat dnia");
        var plan = await CreatePlan(
            client,
            "Plan duplikat",
            new DateOnly(2026, 5, 1),
            new DateOnly(2026, 5, 7)
        );

        var response = await client.PutAsJsonAsync(
            $"/api/plans/{plan.Id}",
            new SavePlanPayload
            {
                Name = plan.Name,
                DateFrom = plan.DateFrom,
                DateTo = plan.DateTo,
                Entries =
                [
                    new SavePlanEntryPayload
                    {
                        RecipeId = recipe.Id,
                        PlannedDate = new DateOnly(2026, 5, 3),
                        Multiplier = 1,
                        SortOrder = 0,
                    },
                    new SavePlanEntryPayload
                    {
                        RecipeId = recipe.Id,
                        PlannedDate = new DateOnly(2026, 5, 3),
                        Multiplier = 1,
                        SortOrder = 1,
                    },
                ],
                RegenerateShoppingList = false,
            }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ReorderAndMultiplier_ShouldPersist()
    {
        using var client = factory.CreateClient();
        var recipeA = await CreateRecipe(client, "A");
        var recipeB = await CreateRecipe(client, "B");
        var plan = await CreatePlan(
            client,
            "Plan kolejnosci",
            new DateOnly(2026, 5, 1),
            new DateOnly(2026, 5, 7)
        );

        var initial = await SavePlan(
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
                        PlannedDate = new DateOnly(2026, 5, 1),
                        Multiplier = 1,
                        SortOrder = 0,
                    },
                    new SavePlanEntryPayload
                    {
                        RecipeId = recipeB.Id,
                        PlannedDate = new DateOnly(2026, 5, 1),
                        Multiplier = 1.5m,
                        SortOrder = 1,
                    },
                ],
                RegenerateShoppingList = false,
            }
        );

        var firstEntry = initial.Entries.Single(x => x.SortOrder == 0);
        var secondEntry = initial.Entries.Single(x => x.SortOrder == 1);

        var reordered = await SavePlan(
            client,
            plan.Id,
            new SavePlanPayload
            {
                Name = initial.Name,
                DateFrom = initial.DateFrom,
                DateTo = initial.DateTo,
                Entries =
                [
                    new SavePlanEntryPayload
                    {
                        Id = firstEntry.Id,
                        RecipeId = firstEntry.RecipeId,
                        PlannedDate = firstEntry.PlannedDate,
                        Multiplier = firstEntry.Multiplier,
                        SortOrder = 1,
                    },
                    new SavePlanEntryPayload
                    {
                        Id = secondEntry.Id,
                        RecipeId = secondEntry.RecipeId,
                        PlannedDate = secondEntry.PlannedDate,
                        Multiplier = secondEntry.Multiplier,
                        SortOrder = 0,
                    },
                ],
                RegenerateShoppingList = false,
            }
        );

        Assert.Equal(secondEntry.Id, reordered.Entries.Single(x => x.SortOrder == 0).Id);
        Assert.Equal(1.5m, reordered.Entries.Single(x => x.Id == secondEntry.Id).Multiplier);

        var invalidMultiplierResponse = await client.PutAsJsonAsync(
            $"/api/plans/{plan.Id}",
            new SavePlanPayload
            {
                Name = reordered.Name,
                DateFrom = reordered.DateFrom,
                DateTo = reordered.DateTo,
                Entries =
                [
                    new SavePlanEntryPayload
                    {
                        Id = reordered.Entries[0].Id,
                        RecipeId = reordered.Entries[0].RecipeId,
                        PlannedDate = reordered.Entries[0].PlannedDate,
                        Multiplier = -1m,
                        SortOrder = reordered.Entries[0].SortOrder,
                    },
                ],
                RegenerateShoppingList = false,
            }
        );

        Assert.Equal(HttpStatusCode.BadRequest, invalidMultiplierResponse.StatusCode);
    }

    [Fact]
    public async Task InactiveRecipes_ShouldRemainInExistingPlans_AndBeExcludedFromNewAssignments()
    {
        using var client = factory.CreateClient();
        var recipe = await CreateRecipe(client, "Przepis do archiwizacji");
        var plan = await CreatePlan(
            client,
            "Plan historii",
            new DateOnly(2026, 5, 1),
            new DateOnly(2026, 5, 7)
        );

        var saved = await SavePlan(
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
                        RecipeId = recipe.Id,
                        PlannedDate = new DateOnly(2026, 5, 1),
                        Multiplier = 1m,
                        SortOrder = 0,
                    },
                ],
                RegenerateShoppingList = false,
            }
        );

        var archiveResponse = await client.PostAsync($"/api/recipes/{recipe.Id}/archive", null);
        Assert.Equal(HttpStatusCode.NoContent, archiveResponse.StatusCode);

        var detailsAfterArchive = await client.GetFromJsonAsync<PlanDetailsDto>(
            $"/api/plans/{plan.Id}",
            JsonOptions
        );
        Assert.NotNull(detailsAfterArchive);
        Assert.False(detailsAfterArchive!.Entries.Single().RecipeIsActive);

        var response = await client.PutAsJsonAsync(
            $"/api/plans/{plan.Id}",
            new SavePlanPayload
            {
                Name = saved.Name,
                DateFrom = saved.DateFrom,
                DateTo = saved.DateTo,
                Entries =
                [
                    new SavePlanEntryPayload
                    {
                        Id = saved.Entries.Single().Id,
                        RecipeId = recipe.Id,
                        PlannedDate = new DateOnly(2026, 5, 1),
                        Multiplier = 1m,
                        SortOrder = 0,
                    },
                    new SavePlanEntryPayload
                    {
                        RecipeId = recipe.Id,
                        PlannedDate = new DateOnly(2026, 5, 2),
                        Multiplier = 1m,
                        SortOrder = 0,
                    },
                ],
                RegenerateShoppingList = false,
            }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SaveChangedPlanWithExistingShoppingList_ShouldRequireRegenerationDecision()
    {
        using var client = factory.CreateClient();
        var plan = await CreatePlan(
            client,
            "Plan zakupowy",
            new DateOnly(2026, 5, 1),
            new DateOnly(2026, 5, 7)
        );

        var markResponse = await client.PostAsync($"/api/plans/{plan.Id}/shopping-generated", null);
        Assert.Equal(HttpStatusCode.NoContent, markResponse.StatusCode);

        var conflictResponse = await client.PutAsJsonAsync(
            $"/api/plans/{plan.Id}",
            new SavePlanPayload
            {
                Name = "Plan zakupowy po zmianie",
                DateFrom = plan.DateFrom,
                DateTo = plan.DateTo,
                Entries = [],
                RegenerateShoppingList = false,
            }
        );

        Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);

        var okResponse = await client.PutAsJsonAsync(
            $"/api/plans/{plan.Id}",
            new SavePlanPayload
            {
                Name = "Plan zakupowy po zmianie",
                DateFrom = plan.DateFrom,
                DateTo = plan.DateTo,
                Entries = [],
                RegenerateShoppingList = true,
            }
        );
        Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
    }

    [Fact]
    public async Task ConcurrentPlanSavesWithShoppingRegeneration_ShouldBothComplete()
    {
        using var client = factory.CreateClient();
        var plan = await CreatePlan(
            client,
            "Plan współbieżny",
            new DateOnly(2026, 5, 1),
            new DateOnly(2026, 5, 7)
        );
        var payload = new SavePlanPayload
        {
            Name = plan.Name,
            DateFrom = plan.DateFrom,
            DateTo = plan.DateTo,
            Entries = [],
            RegenerateShoppingList = true,
        };

        var responses = await Task.WhenAll(
            client.PutAsJsonAsync($"/api/plans/{plan.Id}", payload),
            client.PutAsJsonAsync($"/api/plans/{plan.Id}", payload)
        );

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));
    }

    private static async Task<PlanDetailsDto> CreatePlan(
        HttpClient client,
        string? name,
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

    private static async Task<RecipeApiContracts.RecipeDetailsDto> CreateRecipe(
        HttpClient client,
        string title
    )
    {
        var payload = RecipeApiContracts.CreateRecipePayload(title);
        var response = await client.PostAsJsonAsync("/api/recipes", payload);
        response.EnsureSuccessStatusCode();
        var recipe = await response.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeDetailsDto>(
            JsonOptions
        );
        return recipe!;
    }

    private sealed class PlanSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly DateFrom { get; set; }
        public DateOnly DateTo { get; set; }
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
        public decimal Multiplier { get; set; }
        public int SortOrder { get; set; }
        public bool RecipeIsActive { get; set; }
    }

    private sealed class CreatePlanPayload
    {
        public string? Name { get; set; }
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
}

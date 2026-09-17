using System.Globalization;
using Microsoft.EntityFrameworkCore;
using WhatToCook.Api.Domain.Shopping;
using WhatToCook.Api.Features.Recipes.Shared;
using WhatToCook.Api.Infrastructure.Data;

namespace WhatToCook.Api.Features.Shopping.Lists;

public sealed class ShoppingListService(AppDbContext dbContext, ILogger<ShoppingListService> logger)
{
    public async Task<ShoppingListView?> GetForPlanAsync(
        Guid mealPlanId,
        CancellationToken cancellationToken
    )
    {
        var shoppingList = await dbContext
            .ShoppingLists.AsNoTracking()
            .Include(x => x.MealPlan)
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.MealPlanId == mealPlanId, cancellationToken);

        return shoppingList is null ? null : ToView(shoppingList);
    }

    public async Task<IReadOnlyList<RecipeRecommendationView>?> GetRecipeRecommendationsAsync(
        Guid mealPlanId,
        int limit,
        CancellationToken cancellationToken
    )
    {
        var plan = await dbContext
            .MealPlans.AsNoTracking()
            .Include(x => x.PlannedRecipes)
            .SingleOrDefaultAsync(x => x.Id == mealPlanId, cancellationToken);
        if (plan is null)
        {
            return null;
        }

        var possessedIngredientNames = (
            await dbContext
                .ShoppingListItems.AsNoTracking()
                .Where(x =>
                    x.ShoppingList.MealPlanId == mealPlanId
                    && x.State == ChecklistState.Mam
                    && !x.IsManual
                )
                .Select(x => new { x.Name, x.NormalizedName })
                .ToListAsync(cancellationToken)
        )
            .Select(x => x.NormalizedName ?? TextNormalizer.Normalize(x.Name))
            .Where(x => x.Length > 0)
            .ToHashSet(StringComparer.Ordinal);
        if (possessedIngredientNames.Count == 0)
        {
            return [];
        }

        var plannedRecipeIds = plan.PlannedRecipes.Select(x => x.RecipeId).ToHashSet();
        var recipes = await dbContext
            .Recipes.AsNoTracking()
            .Where(x => x.IsActive && !plannedRecipeIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.MainPhotoPath,
                Ingredients = x.Ingredients.Select(ingredient => new
                {
                    ingredient.Ingredient.DisplayName,
                    ingredient.Ingredient.NormalizedName,
                }),
            })
            .ToListAsync(cancellationToken);

        return recipes
            .Select(recipe =>
            {
                var ingredients = recipe
                    .Ingredients.DistinctBy(x => x.NormalizedName, StringComparer.Ordinal)
                    .ToList();
                var matchedIngredients = ingredients
                    .Where(x => possessedIngredientNames.Contains(x.NormalizedName))
                    .Select(x => x.DisplayName)
                    .ToList();
                return new RecipeRecommendationView(
                    recipe.Id,
                    recipe.Title,
                    recipe.MainPhotoPath,
                    matchedIngredients.Count,
                    ingredients.Count,
                    matchedIngredients
                );
            })
            .Where(x => x.MatchedIngredientCount > 0 && x.Coverage >= 0.3m)
            .OrderByDescending(x => x.MatchedIngredientCount)
            .ThenByDescending(x => x.Coverage)
            .ThenBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Clamp(limit, 1, 20))
            .ToList();
    }

    public async Task<ShoppingListView?> GenerateForPlanAsync(
        Guid mealPlanId,
        CancellationToken cancellationToken
    )
    {
        var ownsTransaction = dbContext.Database.CurrentTransaction is null;
        await using var transaction = ownsTransaction
            ? await dbContext.Database.BeginTransactionAsync(cancellationToken)
            : null;
        await AcquirePlanMutationLockAsync(mealPlanId, cancellationToken);
        var mealPlan = await dbContext
            .MealPlans.Include(x => x.PlannedRecipes)
                .ThenInclude(x => x.Recipe)
                    .ThenInclude(x => x.Ingredients)
                        .ThenInclude(x => x.Ingredient)
            .SingleOrDefaultAsync(x => x.Id == mealPlanId, cancellationToken);

        if (mealPlan is null)
        {
            logger.LogWarning(
                "Cannot generate shopping list because meal plan {MealPlanId} was not found.",
                mealPlanId
            );
            return null;
        }

        var existingShoppingList = await dbContext
            .ShoppingLists.Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.MealPlanId == mealPlanId, cancellationToken);

        if (existingShoppingList is null)
        {
            existingShoppingList = new Domain.Shopping.ShoppingList
            {
                MealPlanId = mealPlan.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };
            dbContext.ShoppingLists.Add(existingShoppingList);
        }
        var generatedItems = BuildGeneratedItems(existingShoppingList.Id, mealPlan.PlannedRecipes);
        MergeGeneratedItems(existingShoppingList, generatedItems);
        existingShoppingList.UpdatedAt = DateTimeOffset.UtcNow;

        mealPlan.HasGeneratedShoppingList = true;
        mealPlan.UpdatedAt = DateTimeOffset.UtcNow;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            logger.LogError(
                exception,
                "Concurrency conflict while generating shopping list for meal plan {MealPlanId}. Entities: {Entities}.",
                mealPlanId,
                string.Join(
                    ", ",
                    exception.Entries.Select(entry =>
                        $"{entry.Metadata.ClrType.Name}:{string.Join("|", entry.Properties.Where(property => property.Metadata.IsPrimaryKey()).Select(property => property.CurrentValue))}"
                    )
                )
            );
            throw;
        }
        if (ownsTransaction)
        {
            await transaction!.CommitAsync(cancellationToken);
        }
        logger.LogInformation(
            "Generated shopping list for meal plan {MealPlanId} with {ItemCount} items.",
            mealPlanId,
            generatedItems.Count
        );

        var refreshed = await dbContext
            .ShoppingLists.AsNoTracking()
            .Include(x => x.MealPlan)
            .Include(x => x.Items)
            .SingleAsync(x => x.MealPlanId == mealPlanId, cancellationToken);

        return ToView(refreshed);
    }

    public async Task<ShoppingListView?> AddManualItemAsync(
        Guid mealPlanId,
        string text,
        CancellationToken cancellationToken
    )
    {
        var trimmed = text.Trim();
        if (trimmed.Length == 0)
        {
            return await GetForPlanAsync(mealPlanId, cancellationToken);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );
        await AcquirePlanMutationLockAsync(mealPlanId, cancellationToken);

        var shoppingList = await dbContext.ShoppingLists.SingleOrDefaultAsync(
            x => x.MealPlanId == mealPlanId,
            cancellationToken
        );
        if (shoppingList is null)
        {
            logger.LogWarning(
                "Cannot add manual shopping list item because plan {MealPlanId} has no generated list.",
                mealPlanId
            );
            return null;
        }

        var maxSortOrder = await dbContext
            .ShoppingListItems.Where(x => x.ShoppingListId == shoppingList.Id)
            .Select(x => (int?)x.SortOrder)
            .MaxAsync(cancellationToken);
        dbContext.ShoppingListItems.Add(
            new ShoppingListItem
            {
                ShoppingListId = shoppingList.Id,
                Name = trimmed,
                IsManual = true,
                State = ChecklistState.NieMam,
                SortOrder = (maxSortOrder ?? -1) + 1,
            }
        );
        shoppingList.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation(
            "Added manual shopping list item to meal plan {MealPlanId}.",
            mealPlanId
        );
        return await GetForPlanAsync(mealPlanId, cancellationToken);
    }

    public async Task<ShoppingListView?> SetItemStateAsync(
        Guid mealPlanId,
        Guid itemId,
        ChecklistState state,
        CancellationToken cancellationToken
    )
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );
        await AcquirePlanMutationLockAsync(mealPlanId, cancellationToken);

        var shoppingList = await dbContext.ShoppingLists.SingleOrDefaultAsync(
            x => x.MealPlanId == mealPlanId,
            cancellationToken
        );
        if (shoppingList is null)
        {
            logger.LogWarning(
                "Cannot set shopping list item state because plan {MealPlanId} has no generated list.",
                mealPlanId
            );
            return null;
        }

        var item = await dbContext.ShoppingListItems.SingleOrDefaultAsync(
            x => x.ShoppingListId == shoppingList.Id && x.Id == itemId,
            cancellationToken
        );
        if (item is null)
        {
            logger.LogWarning(
                "Cannot set shopping list item state because item {ItemId} was not found in plan {MealPlanId}.",
                itemId,
                mealPlanId
            );
            return null;
        }

        item.State = state;
        shoppingList.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation(
            "Updated shopping list item {ItemId} in plan {MealPlanId} to state {State}.",
            itemId,
            mealPlanId,
            state
        );

        return await GetForPlanAsync(mealPlanId, cancellationToken);
    }

    public async Task<string?> BuildCopyTextAsync(
        Guid mealPlanId,
        CancellationToken cancellationToken
    )
    {
        var shoppingList = await dbContext
            .ShoppingLists.AsNoTracking()
            .Include(x => x.MealPlan)
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.MealPlanId == mealPlanId, cancellationToken);

        if (shoppingList is null)
        {
            return null;
        }

        var lines = new List<string>
        {
            $"Lista zakupów - {shoppingList.MealPlan.Name}",
            string.Empty,
        };

        foreach (var item in shoppingList.Items.OrderBy(x => x.SortOrder))
        {
            lines.Add(ToDisplayText(item));
        }

        return string.Join(Environment.NewLine, lines);
    }

    public Task AcquirePlanMutationLockAsync(
        Guid mealPlanId,
        CancellationToken cancellationToken
    ) =>
        dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtext({mealPlanId.ToString()}))",
            cancellationToken
        );

    private static List<ShoppingListItem> BuildGeneratedItems(
        Guid shoppingListId,
        IEnumerable<Domain.Planning.PlannedRecipe> plannedRecipes
    )
    {
        var quantified = new Dictionary<(string Name, string? Unit), AggregatedIngredient>(
            StringTupleComparer.Instance
        );
        var descriptive = new Dictionary<(string Name, string? Unit), AggregatedIngredient>(
            StringTupleComparer.Instance
        );
        var nameOnly = new Dictionary<string, AggregatedIngredient>(StringComparer.Ordinal);

        foreach (var entry in plannedRecipes.OrderBy(x => x.PlannedDate).ThenBy(x => x.SortOrder))
        {
            foreach (var ingredient in entry.Recipe.Ingredients.OrderBy(x => x.SortOrder))
            {
                var normalizedName = ingredient.Ingredient.NormalizedName;
                var displayName = ingredient.Ingredient.DisplayName;
                var normalizedUnit = NormalizeUnit(ingredient.Unit);
                var unit = string.IsNullOrWhiteSpace(ingredient.Unit)
                    ? null
                    : ingredient.Unit.Trim();
                var quantityText = string.IsNullOrWhiteSpace(ingredient.QuantityText)
                    ? null
                    : ingredient.QuantityText.Trim();

                if (TryParseDecimal(quantityText, out var quantity))
                {
                    var key = (normalizedName, normalizedUnit);
                    var scaled = quantity * entry.Multiplier;

                    if (quantified.TryGetValue(key, out var existingQuantified))
                    {
                        existingQuantified.AggregatedQuantity += scaled;
                        continue;
                    }

                    quantified[key] = new AggregatedIngredient
                    {
                        Name = displayName,
                        NormalizedName = normalizedName,
                        Unit = unit,
                        AggregatedQuantity = scaled,
                    };
                    continue;
                }

                if (quantityText is null && unit is null)
                {
                    if (!nameOnly.ContainsKey(normalizedName))
                    {
                        nameOnly[normalizedName] = new AggregatedIngredient
                        {
                            Name = displayName,
                            NormalizedName = normalizedName,
                        };
                    }

                    continue;
                }

                var descriptiveKey = (normalizedName, $"{quantityText}|{normalizedUnit}");
                if (descriptive.TryGetValue(descriptiveKey, out var descriptiveExisting))
                {
                    descriptiveExisting.Repetitions++;
                    continue;
                }

                descriptive[descriptiveKey] = new AggregatedIngredient
                {
                    Name = displayName,
                    NormalizedName = normalizedName,
                    QuantityText = quantityText,
                    Unit = unit,
                    Repetitions = 1,
                };
            }
        }

        var results = new List<ShoppingListItem>();
        foreach (var aggregate in quantified.Values.OrderBy(x => x.Name).ThenBy(x => x.Unit))
        {
            results.Add(
                new ShoppingListItem
                {
                    ShoppingListId = shoppingListId,
                    Name = aggregate.Name,
                    NormalizedName = aggregate.NormalizedName,
                    QuantityText = FormatQuantity(aggregate.AggregatedQuantity),
                    Unit = aggregate.Unit,
                    IsManual = false,
                    State = ChecklistState.NieMam,
                }
            );
        }

        foreach (var aggregate in descriptive.Values.OrderBy(x => x.Name).ThenBy(x => x.Unit))
        {
            results.Add(
                new ShoppingListItem
                {
                    ShoppingListId = shoppingListId,
                    Name = aggregate.Name,
                    NormalizedName = aggregate.NormalizedName,
                    QuantityText = aggregate.QuantityText,
                    Unit = aggregate.Unit,
                    IsManual = false,
                    State = ChecklistState.NieMam,
                }
            );
        }

        foreach (var aggregate in nameOnly.Values.OrderBy(x => x.Name))
        {
            results.Add(
                new ShoppingListItem
                {
                    ShoppingListId = shoppingListId,
                    Name = aggregate.Name,
                    NormalizedName = aggregate.NormalizedName,
                    IsManual = false,
                    State = ChecklistState.NieMam,
                }
            );
        }

        for (var index = 0; index < results.Count; index++)
        {
            results[index].SortOrder = index;
        }

        return results;
    }

    private void MergeGeneratedItems(
        Domain.Shopping.ShoppingList shoppingList,
        IReadOnlyList<ShoppingListItem> generatedItems
    )
    {
        var existingGeneratedItems = shoppingList.Items.Where(x => !x.IsManual).ToList();
        var existingGeneratedIds = existingGeneratedItems.Select(x => x.Id).ToHashSet();
        var existingGenerated = existingGeneratedItems
            .Where(x => !x.IsManual)
            .GroupBy(GetIngredientUnitKey)
            .ToDictionary(x => x.Key, x => new Queue<ShoppingListItem>(x));
        var retainedGenerated = new HashSet<Guid>();

        for (var index = 0; index < generatedItems.Count; index++)
        {
            var generated = generatedItems[index];
            var key = GetIngredientUnitKey(generated);
            if (existingGenerated.TryGetValue(key, out var candidates) && candidates.Count > 0)
            {
                var existing = candidates.Dequeue();
                existing.Name = generated.Name;
                existing.NormalizedName = generated.NormalizedName;
                existing.QuantityText = generated.QuantityText;
                existing.Unit = generated.Unit;
                existing.SortOrder = index;
                retainedGenerated.Add(existing.Id);
                continue;
            }

            generated.SortOrder = index;
            shoppingList.Items.Add(generated);
            dbContext.ShoppingListItems.Add(generated);
        }

        var staleItems = shoppingList
            .Items.Where(x =>
                existingGeneratedIds.Contains(x.Id) && !retainedGenerated.Contains(x.Id)
            )
            .ToList();
        dbContext.ShoppingListItems.RemoveRange(staleItems);

        var manualIndex = generatedItems.Count;
        foreach (
            var manualItem in shoppingList.Items.Where(x => x.IsManual).OrderBy(x => x.SortOrder)
        )
        {
            manualItem.SortOrder = manualIndex++;
        }
    }

    private static string GetIngredientUnitKey(ShoppingListItem item) =>
        $"{item.NormalizedName ?? TextNormalizer.Normalize(item.Name)}|{NormalizeUnit(item.Unit)}";

    private static ShoppingListView ToView(Domain.Shopping.ShoppingList source)
    {
        return new ShoppingListView(
            source.Id,
            source.MealPlanId,
            source.MealPlan.Name,
            source.CreatedAt,
            source.UpdatedAt,
            source
                .Items.OrderBy(x => x.SortOrder)
                .Select(x => new ShoppingListItemView(
                    x.Id,
                    x.Name,
                    x.QuantityText,
                    x.Unit,
                    x.State,
                    x.IsManual,
                    x.SortOrder,
                    ToDisplayText(x)
                ))
                .ToList()
        );
    }

    private static bool TryParseDecimal(string? value, out decimal parsed)
    {
        parsed = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim().Replace(',', '.');
        return decimal.TryParse(
            normalized,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out parsed
        );
    }

    private static string? NormalizeUnit(string? unit) =>
        string.IsNullOrWhiteSpace(unit) ? null : TextNormalizer.Normalize(unit);

    private static string FormatQuantity(decimal value) =>
        value % 1 == 0
            ? value.ToString("0", CultureInfo.InvariantCulture)
            : value.ToString("0.##", CultureInfo.InvariantCulture);

    private static string ToDisplayText(ShoppingListItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.QuantityText) && !string.IsNullOrWhiteSpace(item.Unit))
        {
            return $"{item.QuantityText} {item.Unit} {item.Name}";
        }

        if (!string.IsNullOrWhiteSpace(item.QuantityText))
        {
            return $"{item.QuantityText} {item.Name}";
        }

        return item.Name;
    }

    private sealed class AggregatedIngredient
    {
        public string Name { get; init; } = string.Empty;
        public string NormalizedName { get; init; } = string.Empty;
        public string? QuantityText { get; set; }
        public string? Unit { get; init; }
        public decimal AggregatedQuantity { get; set; }
        public int Repetitions { get; set; }
    }

    private sealed class StringTupleComparer : IEqualityComparer<(string Name, string? Unit)>
    {
        public static readonly StringTupleComparer Instance = new();

        public bool Equals((string Name, string? Unit) x, (string Name, string? Unit) y) =>
            string.Equals(x.Name, y.Name, StringComparison.Ordinal)
            && string.Equals(x.Unit, y.Unit, StringComparison.Ordinal);

        public int GetHashCode((string Name, string? Unit) obj) =>
            HashCode.Combine(obj.Name, obj.Unit);
    }
}

public sealed record ShoppingListView(
    Guid Id,
    Guid MealPlanId,
    string MealPlanName,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<ShoppingListItemView> Items
);

public sealed record RecipeRecommendationView(
    Guid RecipeId,
    string Title,
    string? MainPhotoPath,
    int MatchedIngredientCount,
    int TotalIngredientCount,
    IReadOnlyList<string> MatchedIngredients
)
{
    public decimal Coverage =>
        TotalIngredientCount == 0 ? 0 : (decimal)MatchedIngredientCount / TotalIngredientCount;
}

public sealed record ShoppingListItemView(
    Guid Id,
    string Name,
    string? QuantityText,
    string? Unit,
    ChecklistState State,
    bool IsManual,
    int SortOrder,
    string DisplayText
);

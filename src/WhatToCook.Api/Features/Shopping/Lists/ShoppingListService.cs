using Microsoft.EntityFrameworkCore;
using WhatToCook.Api.Domain.Shopping;
using WhatToCook.Api.Features.Recipes.Shared;
using WhatToCook.Api.Infrastructure.Data;
using static WhatToCook.Api.Features.Shopping.Lists.ShoppingQuantity;

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
        var possessedIngredientKeys = possessedIngredientNames.ToArray();
        var recipes = await dbContext
            .Recipes.AsNoTracking()
            .Where(x =>
                x.IsActive
                && !plannedRecipeIds.Contains(x.Id)
                && x.Ingredients.Any(ingredient =>
                    possessedIngredientKeys.Contains(ingredient.Ingredient.NormalizedName)
                )
            )
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
        var generatedItems = ShoppingListAggregation.BuildGeneratedItems(
            existingShoppingList.Id,
            mealPlan.PlannedRecipes
        );
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
        item.RegenerationNote = null;
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

    private void MergeGeneratedItems(
        Domain.Shopping.ShoppingList shoppingList,
        IReadOnlyList<ShoppingListItem> generatedItems
    )
    {
        var existingItems = shoppingList.Items.Where(x => !x.IsManual).ToList();
        var existingIds = existingItems.Select(x => x.Id).ToHashSet();
        var existingByIngredient = existingItems
            .GroupBy(GetIngredientUnitKey)
            .ToDictionary(group => group.Key, group => group.OrderBy(x => x.SortOrder).ToList());

        // Reserve unique exact-amount matches before reusing any obsolete row.
        // Otherwise an earlier changed amount could steal the identity of a later exact match.
        var assignments =
            new Dictionary<ShoppingListItem, (ShoppingListItem Existing, bool KeepState)>();
        foreach (var incomingGroup in generatedItems.GroupBy(GetIngredientUnitKey))
        {
            if (!existingByIngredient.TryGetValue(incomingGroup.Key, out var oldGroup))
            {
                continue;
            }

            var remaining = oldGroup.ToList();
            var upcoming = incomingGroup.ToList();
            foreach (var incoming in upcoming)
            {
                var amount = GetAmountSignature(incoming);
                if (upcoming.Count(x => GetAmountSignature(x) == amount) != 1)
                {
                    continue;
                }

                var matchingOld = oldGroup.Where(x => GetAmountSignature(x) == amount).ToList();
                if (matchingOld.Count != 1)
                {
                    continue;
                }

                assignments[incoming] = (matchingOld[0], true);
                remaining.Remove(matchingOld[0]);
            }

            foreach (var incoming in upcoming.Where(x => !assignments.ContainsKey(x)))
            {
                if (remaining.Count == 0)
                {
                    break;
                }

                assignments[incoming] = (remaining[0], false);
                remaining.RemoveAt(0);
            }
        }

        var retainedIds = new HashSet<Guid>();
        for (var index = 0; index < generatedItems.Count; index++)
        {
            var generated = generatedItems[index];
            if (assignments.TryGetValue(generated, out var match))
            {
                var previous = match.Existing;
                var amountChanged = GetAmountSignature(previous) != GetAmountSignature(generated);
                previous.Name = generated.Name;
                previous.NormalizedName = generated.NormalizedName;
                previous.QuantityText = generated.QuantityText;
                previous.QuantityContext = generated.QuantityContext;
                previous.Unit = generated.Unit;
                previous.SortOrder = index;
                previous.RegenerationNote =
                    match.KeepState ? null
                    : amountChanged ? "Ilość uległa zmianie — sprawdź dostępność."
                    : "Dopasowanie pozycji jest niejednoznaczne — sprawdź dostępność.";
                if (!match.KeepState)
                {
                    previous.State = ChecklistState.NieMam;
                }

                retainedIds.Add(previous.Id);
                continue;
            }

            generated.SortOrder = index;
            shoppingList.Items.Add(generated);
            dbContext.ShoppingListItems.Add(generated);
        }

        var stale = shoppingList
            .Items.Where(x => existingIds.Contains(x.Id) && !retainedIds.Contains(x.Id))
            .ToList();
        dbContext.ShoppingListItems.RemoveRange(stale);

        var nextSortOrder = generatedItems.Count;
        foreach (var manual in shoppingList.Items.Where(x => x.IsManual).OrderBy(x => x.SortOrder))
        {
            manual.SortOrder = nextSortOrder++;
        }
    }

    private static string GetAmountSignature(ShoppingListItem item)
    {
        var amount = TryParseDecimal(item.QuantityText, out var numeric)
            ? $"numeric:{FormatQuantity(numeric)}"
            : $"text:{TextNormalizer.Normalize(item.QuantityText ?? string.Empty)}";
        return $"{amount}|{item.QuantityContext ?? string.Empty}";
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
                    ToDisplayText(x),
                    x.QuantityContext,
                    x.RegenerationNote
                ))
                .ToList()
        );
    }

    private static string ToDisplayText(ShoppingListItem item)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(item.QuantityText))
        {
            parts.Add(item.QuantityText);
        }

        if (!string.IsNullOrWhiteSpace(item.Unit))
        {
            parts.Add(item.Unit);
        }

        parts.Add(item.Name);
        var text = string.Join(" ", parts);
        return item.QuantityContext is null ? text : $"{text} ({item.QuantityContext})";
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
    string DisplayText,
    string? QuantityContext,
    string? RegenerationNote
);

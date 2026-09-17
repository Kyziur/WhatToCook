using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WhatToCook.Api.Domain.Planning;
using WhatToCook.Api.Features.Shopping.Lists;
using WhatToCook.Api.Infrastructure.Data;

namespace WhatToCook.Api.Features.Planning.Plans;

public static class PlansEndpoints
{
    private const int MaxPlanEntries = 500;

    public static IEndpointRouteBuilder MapMealPlanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/plans").WithTags("Planning");

        group.MapGet("/", GetMealPlans).WithName("GetMealPlans");
        group.MapGet("/{planId:guid}", GetMealPlanDetails).WithName("GetMealPlanDetails");
        group.MapPost("/", CreateMealPlan).WithName("CreateMealPlan");
        group.MapPut("/{planId:guid}", UpdateMealPlan).WithName("UpdateMealPlan");
        group
            .MapPost("/{planId:guid}/shopping-generated", MarkShoppingGenerated)
            .WithName("MarkShoppingGenerated");

        return app;
    }

    private static async Task<IResult> GetMealPlans(
        AppDbContext dbContext,
        CancellationToken cancellationToken
    )
    {
        var plans = await dbContext
            .MealPlans.AsNoTracking()
            .Include(x => x.PlannedRecipes)
            .OrderByDescending(x => x.DateFrom)
            .ThenByDescending(x => x.Name)
            .Select(x => new MealPlanSummaryResponse(
                x.Id,
                x.Name,
                x.DateFrom,
                x.DateTo,
                x.HasGeneratedShoppingList,
                x.PlannedRecipes.Count
            ))
            .ToListAsync(cancellationToken);

        return Results.Ok(plans);
    }

    private static async Task<IResult> GetMealPlanDetails(
        Guid planId,
        AppDbContext dbContext,
        CancellationToken cancellationToken
    )
    {
        var plan = await dbContext
            .MealPlans.AsNoTracking()
            .Include(x => x.PlannedRecipes)
                .ThenInclude(x => x.Recipe)
            .SingleOrDefaultAsync(x => x.Id == planId, cancellationToken);

        if (plan is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(ToDetailsResponse(plan));
    }

    private static async Task<IResult> CreateMealPlan(
        CreateMealPlanRequest request,
        AppDbContext dbContext,
        CancellationToken cancellationToken
    )
    {
        var validation = ValidateRange(request.DateFrom, request.DateTo);
        if (validation is not null)
        {
            return validation;
        }

        var name = string.IsNullOrWhiteSpace(request.Name)
            ? BuildDefaultPlanName(request.DateFrom, request.DateTo)
            : request.Name.Trim();

        var plan = new MealPlan
        {
            Name = name,
            DateFrom = request.DateFrom,
            DateTo = request.DateTo,
            HasGeneratedShoppingList = false,
        };

        dbContext.MealPlans.Add(plan);
        await dbContext.SaveChangesAsync(cancellationToken);

        var createdPlan = await dbContext
            .MealPlans.AsNoTracking()
            .Include(x => x.PlannedRecipes)
                .ThenInclude(x => x.Recipe)
            .SingleAsync(x => x.Id == plan.Id, cancellationToken);

        return Results.Created($"/api/plans/{plan.Id}", ToDetailsResponse(createdPlan));
    }

    private static async Task<IResult> UpdateMealPlan(
        Guid planId,
        UpdateMealPlanRequest request,
        AppDbContext dbContext,
        ShoppingListService shoppingListService,
        CancellationToken cancellationToken
    )
    {
        var validation = ValidateUpdateRequest(request);
        if (validation is not null)
        {
            return validation;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );
        await shoppingListService.AcquirePlanMutationLockAsync(planId, cancellationToken);

        var plan = await dbContext.MealPlans.SingleOrDefaultAsync(
            x => x.Id == planId,
            cancellationToken
        );

        if (plan is null)
        {
            return Results.NotFound();
        }

        var existingEntries = await dbContext
            .PlannedRecipes.AsNoTracking()
            .Where(x => x.MealPlanId == planId)
            .ToListAsync(cancellationToken);

        var recipeIds = request.Entries.Select(x => x.RecipeId).Distinct().ToArray();
        var recipes = await dbContext
            .Recipes.AsNoTracking()
            .Where(x => recipeIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var existingEntriesById = existingEntries.ToDictionary(x => x.Id);
        foreach (var entry in request.Entries)
        {
            if (!recipes.TryGetValue(entry.RecipeId, out var recipe))
            {
                return Results.ValidationProblem(
                    new Dictionary<string, string[]>
                    {
                        [nameof(request.Entries)] = ["One of selected recipes does not exist."],
                    }
                );
            }

            var isExistingEntry =
                entry.Id.HasValue && existingEntriesById.ContainsKey(entry.Id.Value);
            if (!recipe.IsActive && !isExistingEntry)
            {
                return Results.ValidationProblem(
                    new Dictionary<string, string[]>
                    {
                        [nameof(request.Entries)] =
                        [
                            $"Recipe '{recipe.Title}' is inactive and cannot be added to a new day.",
                        ],
                    }
                );
            }
        }
        if (
            plan.HasGeneratedShoppingList
            && !request.RegenerateShoppingList
            && HasPlanChanged(plan, existingEntries, request)
        )
        {
            return Results.Conflict(
                new
                {
                    code = "shopping_list_regeneration_required",
                    message = "The shopping list must be regenerated after changing the meal plan.",
                }
            );
        }

        plan.Name = request.Name.Trim();
        plan.DateFrom = request.DateFrom;
        plan.DateTo = request.DateTo;
        plan.UpdatedAt = DateTimeOffset.UtcNow;
        if (request.RegenerateShoppingList)
        {
            plan.HasGeneratedShoppingList = true;
        }

        try
        {
            await dbContext
                .PlannedRecipes.Where(x => x.MealPlanId == plan.Id)
                .ExecuteDeleteAsync(cancellationToken);

            var replacementEntries = request
                .Entries.OrderBy(x => x.PlannedDate)
                .ThenBy(x => x.SortOrder)
                .Select(entry => new PlannedRecipe
                {
                    Id = entry.Id ?? Guid.NewGuid(),
                    MealPlanId = plan.Id,
                    RecipeId = entry.RecipeId,
                    PlannedDate = entry.PlannedDate,
                    Multiplier = entry.Multiplier,
                    SortOrder = entry.SortOrder,
                })
                .ToList();
            if (replacementEntries.Count > 0)
            {
                dbContext.PlannedRecipes.AddRange(replacementEntries);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            if (request.RegenerateShoppingList)
            {
                await shoppingListService.GenerateForPlanAsync(plan.Id, cancellationToken);
            }

            var updatedPlan = await dbContext
                .MealPlans.AsNoTracking()
                .Include(x => x.PlannedRecipes)
                    .ThenInclude(x => x.Recipe)
                .SingleAsync(x => x.Id == plan.Id, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Results.Ok(ToDetailsResponse(updatedPlan));
        }
        catch (DbUpdateConcurrencyException)
        {
            return Results.Conflict(
                new
                {
                    code = "save_conflict",
                    message = "Nie udało się zapisać planu z powodu równoległej zmiany. Odśwież dane i spróbuj ponownie.",
                }
            );
        }
        catch (DbUpdateException exception)
            when (exception.InnerException
                    is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }
            )
        {
            return Results.Conflict(
                new
                {
                    message = "The same recipe can appear at most once per day in the same plan.",
                }
            );
        }
    }

    private static async Task<IResult> MarkShoppingGenerated(
        Guid planId,
        ShoppingListService shoppingListService,
        CancellationToken cancellationToken
    )
    {
        var generated = await shoppingListService.GenerateForPlanAsync(planId, cancellationToken);
        if (generated is null)
        {
            return Results.NotFound();
        }

        return Results.NoContent();
    }

    private static IResult? ValidateRange(DateOnly from, DateOnly to)
    {
        if (to < from)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    [nameof(CreateMealPlanRequest.DateTo)] =
                    [
                        "DateTo must be greater than or equal to DateFrom.",
                    ],
                }
            );
        }

        return null;
    }

    private static IResult? ValidateUpdateRequest(UpdateMealPlanRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors[nameof(request.Name)] = ["Name is required."];
        }

        if (request.Name.Trim().Length > 200)
        {
            errors[nameof(request.Name)] = ["Name must be at most 200 characters."];
        }

        if (request.Entries.Count > MaxPlanEntries)
        {
            errors[nameof(request.Entries)] =
            [
                $"A plan can contain at most {MaxPlanEntries} entries.",
            ];
        }

        if (request.DateTo < request.DateFrom)
        {
            errors[nameof(request.DateTo)] = ["DateTo must be greater than or equal to DateFrom."];
        }

        if (
            request.Entries.Any(entry =>
                entry.PlannedDate < request.DateFrom || entry.PlannedDate > request.DateTo
            )
        )
        {
            errors[nameof(request.Entries)] =
            [
                "Every planned recipe date must be within the selected plan range.",
            ];
        }

        if (request.Entries.Any(entry => entry.Multiplier <= 0))
        {
            errors[nameof(request.Entries)] = ["Multiplier must be a positive numeric value."];
        }

        var duplicateInDay = request
            .Entries.GroupBy(x => new { x.PlannedDate, x.RecipeId })
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateInDay is not null)
        {
            errors[nameof(request.Entries)] =
            [
                "The same recipe can appear at most once per day in the same plan.",
            ];
        }

        return errors.Count == 0 ? null : Results.ValidationProblem(errors);
    }

    private static MealPlanDetailsResponse ToDetailsResponse(MealPlan plan)
    {
        return new MealPlanDetailsResponse(
            plan.Id,
            plan.Name,
            plan.DateFrom,
            plan.DateTo,
            plan.HasGeneratedShoppingList,
            plan.PlannedRecipes.OrderBy(x => x.PlannedDate)
                .ThenBy(x => x.SortOrder)
                .Select(x => new PlannedRecipeEntryResponse(
                    x.Id,
                    x.RecipeId,
                    x.Recipe.Title,
                    x.Recipe.IsActive,
                    x.PlannedDate,
                    x.Multiplier,
                    x.SortOrder
                ))
                .ToList()
        );
    }

    private static bool HasPlanChanged(
        MealPlan existingPlan,
        IReadOnlyCollection<PlannedRecipe> existingEntries,
        UpdateMealPlanRequest request
    )
    {
        if (
            !string.Equals(existingPlan.Name, request.Name.Trim(), StringComparison.Ordinal)
            || existingPlan.DateFrom != request.DateFrom
            || existingPlan.DateTo != request.DateTo
        )
        {
            return true;
        }

        if (existingEntries.Count != request.Entries.Count)
        {
            return true;
        }

        var byId = existingEntries.ToDictionary(x => x.Id);
        foreach (var requestEntry in request.Entries)
        {
            if (
                !requestEntry.Id.HasValue
                || !byId.TryGetValue(requestEntry.Id.Value, out var existingEntry)
            )
            {
                return true;
            }

            if (
                existingEntry.RecipeId != requestEntry.RecipeId
                || existingEntry.PlannedDate != requestEntry.PlannedDate
                || existingEntry.Multiplier != requestEntry.Multiplier
                || existingEntry.SortOrder != requestEntry.SortOrder
            )
            {
                return true;
            }
        }

        return false;
    }

    private static string BuildDefaultPlanName(DateOnly from, DateOnly to) =>
        $"Plan tydzień {from:dd.MM} - {to:dd.MM}";

    private sealed class CreateMealPlanRequest
    {
        public string? Name { get; init; }
        public DateOnly DateFrom { get; init; }
        public DateOnly DateTo { get; init; }
    }

    private sealed class UpdateMealPlanRequest
    {
        public string Name { get; init; } = string.Empty;
        public DateOnly DateFrom { get; init; }
        public DateOnly DateTo { get; init; }
        public bool RegenerateShoppingList { get; init; }
        public List<PlannedRecipeUpsertRequest> Entries { get; init; } = [];
    }

    private sealed class PlannedRecipeUpsertRequest
    {
        public Guid? Id { get; init; }
        public Guid RecipeId { get; init; }
        public DateOnly PlannedDate { get; init; }
        public decimal Multiplier { get; init; } = 1m;
        public int SortOrder { get; init; }
    }

    private sealed record MealPlanSummaryResponse(
        Guid Id,
        string Name,
        DateOnly DateFrom,
        DateOnly DateTo,
        bool HasGeneratedShoppingList,
        int PlannedRecipeCount
    );

    private sealed record MealPlanDetailsResponse(
        Guid Id,
        string Name,
        DateOnly DateFrom,
        DateOnly DateTo,
        bool HasGeneratedShoppingList,
        IReadOnlyList<PlannedRecipeEntryResponse> Entries
    );

    private sealed record PlannedRecipeEntryResponse(
        Guid Id,
        Guid RecipeId,
        string RecipeTitle,
        bool RecipeIsActive,
        DateOnly PlannedDate,
        decimal Multiplier,
        int SortOrder
    );
}

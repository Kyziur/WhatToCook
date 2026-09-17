using Microsoft.EntityFrameworkCore;
using Npgsql;
using WhatToCook.Api.Domain.Recipes;
using WhatToCook.Api.Features.Recipes.Imports;
using WhatToCook.Api.Infrastructure.Data;

namespace WhatToCook.Api.Features.Recipes.Shared;

public interface IRecipeWriteService
{
    Task<RecipeWriteResult> CreateAsync(
        RecipeUpsertRequest request,
        CancellationToken cancellationToken
    );

    Task<RecipeWriteResult> UpdateAsync(
        Guid recipeId,
        RecipeUpsertRequest request,
        CancellationToken cancellationToken
    );
}

public sealed class RecipeWriteService(AppDbContext dbContext) : IRecipeWriteService
{
    public async Task<RecipeWriteResult> CreateAsync(
        RecipeUpsertRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await InTransactionAsync(
                () => CreateCoreAsync(request, cancellationToken),
                cancellationToken
            );
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            return RecipeWriteResult.Conflict(
                "A concurrent recipe change conflicts with existing data."
            );
        }
    }

    private async Task<RecipeWriteResult> CreateCoreAsync(
        RecipeUpsertRequest request,
        CancellationToken cancellationToken
    )
    {
        var errors = RecipeWriteValidator.Validate(request);
        if (errors.Count > 0)
        {
            return RecipeWriteResult.ValidationFailure(errors);
        }

        var normalizedTitle = TextNormalizer.Normalize(request.Title);
        var titleExists = await dbContext.Recipes.AnyAsync(
            x => x.NormalizedTitle == normalizedTitle,
            cancellationToken
        );

        if (titleExists)
        {
            return RecipeWriteResult.Conflict("Recipe title must be unique.");
        }

        var recipe = new Recipe();
        await ApplyRecipeValues(recipe, request, cancellationToken);

        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync(cancellationToken);

        var createdRecipe = await LoadRecipeAsync(recipe.Id, cancellationToken);
        return RecipeWriteResult.Success(createdRecipe.ToDetailsResponse());
    }

    public async Task<RecipeWriteResult> UpdateAsync(
        Guid recipeId,
        RecipeUpsertRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await InTransactionAsync(
                () => UpdateCoreAsync(recipeId, request, cancellationToken),
                cancellationToken
            );
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            return RecipeWriteResult.Conflict(
                "A concurrent recipe change conflicts with existing data."
            );
        }
    }

    private async Task<RecipeWriteResult> UpdateCoreAsync(
        Guid recipeId,
        RecipeUpsertRequest request,
        CancellationToken cancellationToken
    )
    {
        var errors = RecipeWriteValidator.Validate(request);
        if (errors.Count > 0)
        {
            return RecipeWriteResult.ValidationFailure(errors);
        }

        var recipe = await dbContext.Recipes.SingleOrDefaultAsync(
            x => x.Id == recipeId,
            cancellationToken
        );

        if (recipe is null)
        {
            return RecipeWriteResult.NotFound();
        }

        var normalizedTitle = TextNormalizer.Normalize(request.Title);
        var titleExists = await dbContext.Recipes.AnyAsync(
            x => x.Id != recipeId && x.NormalizedTitle == normalizedTitle,
            cancellationToken
        );

        if (titleExists)
        {
            return RecipeWriteResult.Conflict("Recipe title must be unique.");
        }

        await ApplyRecipeValuesForUpdate(recipe, request, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var updatedRecipe = await LoadRecipeAsync(recipeId, cancellationToken);
        return RecipeWriteResult.Success(updatedRecipe.ToDetailsResponse());
    }

    private async Task<T> InTransactionAsync<T>(
        Func<Task<T>> action,
        CancellationToken cancellationToken
    )
    {
        if (dbContext.Database.CurrentTransaction is not null)
        {
            return await action();
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );
        try
        {
            var result = await action();
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception) =>
        exception.InnerException
            is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    private async Task<Recipe> LoadRecipeAsync(Guid recipeId, CancellationToken cancellationToken)
    {
        return await dbContext
            .Recipes.AsNoTracking()
            .Include(x => x.RecipeTags)
                .ThenInclude(x => x.Tag)
            .Include(x => x.Ingredients)
                .ThenInclude(x => x.Ingredient)
            .Include(x => x.Steps)
            .SingleAsync(x => x.Id == recipeId, cancellationToken);
    }

    private async Task ApplyRecipeValues(
        Recipe recipe,
        RecipeUpsertRequest request,
        CancellationToken cancellationToken
    )
    {
        recipe.Title = request.Title.Trim();
        recipe.NormalizedTitle = TextNormalizer.Normalize(request.Title);
        recipe.Servings = request.Servings;
        recipe.Source = string.IsNullOrWhiteSpace(request.Source) ? null : request.Source.Trim();
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        recipe.Ingredients.Clear();
        var ingredientRequests = request
            .Ingredients.Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Select(NormalizeIngredientRequest)
            .ToArray();
        var ingredientDisplayNamesByNormalizedName = ingredientRequests
            .Select(x => x.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToDictionary(TextNormalizer.Normalize, x => x, StringComparer.Ordinal);
        var ingredientsByNormalizedName = await GetOrCreateIngredientsAsync(
            ingredientDisplayNamesByNormalizedName,
            cancellationToken
        );

        for (var index = 0; index < ingredientRequests.Length; index++)
        {
            var requestIngredient = ingredientRequests[index];
            var normalizedIngredientName = TextNormalizer.Normalize(requestIngredient.Name);
            var ingredient = ingredientsByNormalizedName[normalizedIngredientName];

            recipe.Ingredients.Add(
                new RecipeIngredient
                {
                    Ingredient = ingredient,
                    QuantityText = string.IsNullOrWhiteSpace(requestIngredient.QuantityText)
                        ? null
                        : requestIngredient.QuantityText,
                    Unit = string.IsNullOrWhiteSpace(requestIngredient.Unit)
                        ? null
                        : requestIngredient.Unit,
                    SortOrder = index,
                }
            );
        }

        recipe.Steps.Clear();
        var validSteps = request
            .Steps.Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .ToArray();

        for (var index = 0; index < validSteps.Length; index++)
        {
            recipe.Steps.Add(new RecipeStep { Text = validSteps[index], SortOrder = index });
        }

        recipe.RecipeTags.Clear();
        var tagDisplayNamesByNormalizedName = request
            .Tags.Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToDictionary(TextNormalizer.Normalize, x => x, StringComparer.Ordinal);
        var tagsByNormalizedName = await GetOrCreateTagsAsync(
            tagDisplayNamesByNormalizedName,
            cancellationToken
        );
        foreach (var normalizedTag in tagDisplayNamesByNormalizedName.Keys)
        {
            recipe.RecipeTags.Add(new RecipeTag { Tag = tagsByNormalizedName[normalizedTag] });
        }
    }

    private async Task ApplyRecipeValuesForUpdate(
        Recipe recipe,
        RecipeUpsertRequest request,
        CancellationToken cancellationToken
    )
    {
        recipe.Title = request.Title.Trim();
        recipe.NormalizedTitle = TextNormalizer.Normalize(request.Title);
        recipe.Servings = request.Servings;
        recipe.Source = string.IsNullOrWhiteSpace(request.Source) ? null : request.Source.Trim();
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext
            .RecipeIngredients.Where(x => x.RecipeId == recipe.Id)
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext
            .RecipeSteps.Where(x => x.RecipeId == recipe.Id)
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext
            .RecipeTags.Where(x => x.RecipeId == recipe.Id)
            .ExecuteDeleteAsync(cancellationToken);

        var ingredientRequests = request
            .Ingredients.Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Select(NormalizeIngredientRequest)
            .ToArray();
        var ingredientDisplayNamesByNormalizedName = ingredientRequests
            .Select(x => x.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToDictionary(TextNormalizer.Normalize, x => x, StringComparer.Ordinal);
        var ingredientsByNormalizedName = await GetOrCreateIngredientsAsync(
            ingredientDisplayNamesByNormalizedName,
            cancellationToken
        );

        for (var index = 0; index < ingredientRequests.Length; index++)
        {
            var requestIngredient = ingredientRequests[index];
            var normalizedIngredientName = TextNormalizer.Normalize(requestIngredient.Name);
            var ingredient = ingredientsByNormalizedName[normalizedIngredientName];

            dbContext.RecipeIngredients.Add(
                new RecipeIngredient
                {
                    RecipeId = recipe.Id,
                    Ingredient = ingredient,
                    QuantityText = string.IsNullOrWhiteSpace(requestIngredient.QuantityText)
                        ? null
                        : requestIngredient.QuantityText,
                    Unit = string.IsNullOrWhiteSpace(requestIngredient.Unit)
                        ? null
                        : requestIngredient.Unit,
                    SortOrder = index,
                }
            );
        }

        var validSteps = request
            .Steps.Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .ToArray();

        for (var index = 0; index < validSteps.Length; index++)
        {
            dbContext.RecipeSteps.Add(
                new RecipeStep
                {
                    RecipeId = recipe.Id,
                    Text = validSteps[index],
                    SortOrder = index,
                }
            );
        }

        var tagDisplayNamesByNormalizedName = request
            .Tags.Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToDictionary(TextNormalizer.Normalize, x => x, StringComparer.Ordinal);
        var tagsByNormalizedName = await GetOrCreateTagsAsync(
            tagDisplayNamesByNormalizedName,
            cancellationToken
        );
        foreach (var normalizedTag in tagDisplayNamesByNormalizedName.Keys)
        {
            dbContext.RecipeTags.Add(
                new RecipeTag { RecipeId = recipe.Id, Tag = tagsByNormalizedName[normalizedTag] }
            );
        }
    }

    private static RecipeIngredientRequest NormalizeIngredientRequest(
        RecipeIngredientRequest ingredient
    )
    {
        var name = ingredient.Name.Trim();
        var quantity = ingredient.QuantityText?.Trim();
        var unit = ingredient.Unit?.Trim();
        if (!string.IsNullOrWhiteSpace(quantity) || !string.IsNullOrWhiteSpace(unit))
        {
            return new RecipeIngredientRequest
            {
                Name = name,
                QuantityText = quantity,
                Unit = unit,
            };
        }

        var parsed = RecipeImportIngredientParser.ParseIngredientText(name);
        return new RecipeIngredientRequest
        {
            Name = parsed.Name,
            QuantityText = parsed.QuantityText,
            Unit = parsed.Unit,
        };
    }

    private async Task<Dictionary<string, Ingredient>> GetOrCreateIngredientsAsync(
        IReadOnlyDictionary<string, string> displayNamesByNormalizedName,
        CancellationToken cancellationToken
    )
    {
        var normalizedNames = displayNamesByNormalizedName.Keys.ToArray();
        var ingredientsByNormalizedName = await dbContext
            .Ingredients.Where(x => normalizedNames.Contains(x.NormalizedName))
            .ToDictionaryAsync(x => x.NormalizedName, StringComparer.Ordinal, cancellationToken);

        foreach (var (normalizedName, displayName) in displayNamesByNormalizedName)
        {
            if (ingredientsByNormalizedName.ContainsKey(normalizedName))
            {
                continue;
            }

            var ingredient = new Ingredient
            {
                DisplayName = displayName,
                NormalizedName = normalizedName,
            };

            dbContext.Ingredients.Add(ingredient);
            ingredientsByNormalizedName[normalizedName] = ingredient;
        }

        return ingredientsByNormalizedName;
    }

    private async Task<Dictionary<string, Tag>> GetOrCreateTagsAsync(
        IReadOnlyDictionary<string, string> displayNamesByNormalizedName,
        CancellationToken cancellationToken
    )
    {
        var normalizedNames = displayNamesByNormalizedName.Keys.ToArray();
        var tagsByNormalizedName = await dbContext
            .Tags.Where(x => normalizedNames.Contains(x.NormalizedName))
            .ToDictionaryAsync(x => x.NormalizedName, StringComparer.Ordinal, cancellationToken);

        foreach (var (normalizedName, displayName) in displayNamesByNormalizedName)
        {
            if (tagsByNormalizedName.ContainsKey(normalizedName))
            {
                continue;
            }

            var tag = new Tag { DisplayName = displayName, NormalizedName = normalizedName };

            dbContext.Tags.Add(tag);
            tagsByNormalizedName[normalizedName] = tag;
        }

        return tagsByNormalizedName;
    }
}

public sealed record RecipeWriteResult(
    RecipeWriteStatus Status,
    RecipeDetailsResponse? Recipe,
    string? ErrorMessage,
    IReadOnlyDictionary<string, string[]> ValidationErrors
)
{
    public static RecipeWriteResult Success(RecipeDetailsResponse recipe) =>
        new(RecipeWriteStatus.Success, recipe, null, new Dictionary<string, string[]>());

    public static RecipeWriteResult ValidationFailure(
        IReadOnlyDictionary<string, string[]> validationErrors
    ) => new(RecipeWriteStatus.ValidationFailure, null, null, validationErrors);

    public static RecipeWriteResult Conflict(string errorMessage) =>
        new(RecipeWriteStatus.Conflict, null, errorMessage, new Dictionary<string, string[]>());

    public static RecipeWriteResult NotFound() =>
        new(RecipeWriteStatus.NotFound, null, null, new Dictionary<string, string[]>());
}

public enum RecipeWriteStatus
{
    Success = 1,
    ValidationFailure = 2,
    Conflict = 3,
    NotFound = 4,
}

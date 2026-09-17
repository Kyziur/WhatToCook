using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhatToCook.Api.Features.Recipes.Shared;
using WhatToCook.Api.Infrastructure.Data;
using WhatToCook.Api.Infrastructure.Storage;

namespace WhatToCook.Api.Features.Recipes.Editor;

public static class EditorEndpoints
{
    public static IEndpointRouteBuilder MapRecipeEditorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/recipes").WithTags("Recipes");

        group.MapPost("/", CreateRecipe).WithName("CreateRecipe");

        group.MapPut("/{recipeId:guid}", UpdateRecipe).WithName("UpdateRecipe");

        group.MapPost("/{recipeId:guid}/archive", ArchiveRecipe).WithName("ArchiveRecipe");

        group
            .MapPost("/{recipeId:guid}/photo", UploadMainPhoto)
            .DisableAntiforgery()
            .WithName("UploadMainPhoto");

        return app;
    }

    private static async Task<IResult> CreateRecipe(
        RecipeUpsertRequest request,
        IRecipeWriteService recipeWriteService,
        ILogger<EditorEndpointsLog> logger,
        CancellationToken cancellationToken
    )
    {
        var result = await recipeWriteService.CreateAsync(request, cancellationToken);
        if (result.Status == RecipeWriteStatus.ValidationFailure)
        {
            logger.LogWarning(
                "Recipe create request failed validation for title {Title}.",
                request.Title
            );
            return Results.ValidationProblem(result.ValidationErrors);
        }

        if (result.Status == RecipeWriteStatus.Conflict)
        {
            logger.LogWarning(
                "Recipe create request rejected because normalized title {NormalizedTitle} already exists.",
                TextNormalizer.Normalize(request.Title)
            );
            return Results.Conflict(
                new { message = result.ErrorMessage ?? "Recipe title must be unique." }
            );
        }

        logger.LogInformation(
            "Created recipe {RecipeId} with title {Title}.",
            result.Recipe!.Id,
            result.Recipe.Title
        );

        return Results.Created($"/api/recipes/{result.Recipe.Id}", result.Recipe);
    }

    private static async Task<IResult> UpdateRecipe(
        Guid recipeId,
        RecipeUpsertRequest request,
        IRecipeWriteService recipeWriteService,
        ILogger<EditorEndpointsLog> logger,
        CancellationToken cancellationToken
    )
    {
        var result = await recipeWriteService.UpdateAsync(recipeId, request, cancellationToken);
        if (result.Status == RecipeWriteStatus.ValidationFailure)
        {
            logger.LogWarning(
                "Recipe update request failed validation for recipe {RecipeId}.",
                recipeId
            );
            return Results.ValidationProblem(result.ValidationErrors);
        }

        if (result.Status == RecipeWriteStatus.NotFound)
        {
            logger.LogWarning(
                "Recipe update request failed because recipe {RecipeId} was not found.",
                recipeId
            );
            return Results.NotFound();
        }

        if (result.Status == RecipeWriteStatus.Conflict)
        {
            logger.LogWarning(
                "Recipe update request rejected because normalized title {NormalizedTitle} already exists.",
                TextNormalizer.Normalize(request.Title)
            );
            return Results.Conflict(
                new { message = result.ErrorMessage ?? "Recipe title must be unique." }
            );
        }

        logger.LogInformation("Updated recipe {RecipeId}.", recipeId);

        return Results.Ok(result.Recipe);
    }

    private static async Task<IResult> ArchiveRecipe(
        Guid recipeId,
        AppDbContext dbContext,
        ILogger<EditorEndpointsLog> logger,
        CancellationToken cancellationToken
    )
    {
        var recipe = await dbContext.Recipes.SingleOrDefaultAsync(
            x => x.Id == recipeId,
            cancellationToken
        );
        if (recipe is null)
        {
            logger.LogWarning(
                "Recipe archive request failed because recipe {RecipeId} was not found.",
                recipeId
            );
            return Results.NotFound();
        }

        recipe.IsActive = false;
        recipe.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Archived recipe {RecipeId}.", recipeId);

        return Results.NoContent();
    }

    private static async Task<IResult> UploadMainPhoto(
        Guid recipeId,
        [FromForm] IFormFile? file,
        AppDbContext dbContext,
        IRecipeImageStorage imageStorage,
        ILogger<EditorEndpointsLog> logger,
        CancellationToken cancellationToken
    )
    {
        if (file is null || file.Length == 0)
        {
            logger.LogWarning(
                "Recipe photo upload failed because no file was supplied for recipe {RecipeId}.",
                recipeId
            );
            return Results.BadRequest(new { message = "Photo file is required." });
        }

        var recipe = await dbContext.Recipes.SingleOrDefaultAsync(
            x => x.Id == recipeId,
            cancellationToken
        );
        if (recipe is null)
        {
            logger.LogWarning(
                "Recipe photo upload failed because recipe {RecipeId} was not found.",
                recipeId
            );
            return Results.NotFound();
        }

        string? storedPath = null;
        var previousPath = recipe.MainPhotoPath;
        try
        {
            storedPath = await imageStorage.SaveAsync(file, cancellationToken);
            recipe.MainPhotoPath = storedPath;
            recipe.UpdatedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (InvalidDataException exception)
        {
            return Results.BadRequest(new { message = exception.Message });
        }
        catch
        {
            if (storedPath is not null)
            {
                await imageStorage.DeleteAsync(storedPath, CancellationToken.None);
            }

            throw;
        }

        if (!string.IsNullOrWhiteSpace(previousPath) && previousPath != storedPath)
        {
            try
            {
                await imageStorage.DeleteAsync(previousPath, cancellationToken);
            }
            catch (IOException exception)
            {
                logger.LogWarning(
                    exception,
                    "Could not remove replaced recipe photo {PhotoPath} for recipe {RecipeId}.",
                    previousPath,
                    recipeId
                );
            }
        }

        logger.LogInformation(
            "Uploaded main photo for recipe {RecipeId} to path {StoredPath}.",
            recipeId,
            storedPath
        );

        return Results.Ok(new { mainPhotoPath = storedPath });
    }

    private sealed class EditorEndpointsLog;
}

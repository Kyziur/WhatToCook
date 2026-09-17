using Microsoft.AspNetCore.Mvc;

namespace WhatToCook.Api.Features.Recipes.Imports;

public static class ImportEndpoints
{
    public static IEndpointRouteBuilder MapRecipeImportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/recipe-imports").WithTags("Recipe Imports");

        group.MapPost("/url", CreateFromUrl).WithName("CreateRecipeImportFromUrl");
        group.MapGet("/{draftId:guid}", GetDraft).WithName("GetRecipeImportDraft");
        group.MapPut("/{draftId:guid}", UpdateDraft).WithName("UpdateRecipeImportDraft");
        group
            .MapPost("/{draftId:guid}/finalize", FinalizeDraft)
            .WithName("FinalizeRecipeImportDraft");

        return app;
    }

    private static async Task<IResult> CreateFromUrl(
        CreateRecipeImportFromUrlRequest request,
        IRecipeImportService importService,
        CancellationToken cancellationToken
    )
    {
        var result = await importService.CreateFromUrlAsync(request.Url, cancellationToken);
        return result.Status switch
        {
            RecipeImportCreateStatus.Success => Results.Created(
                $"/api/recipe-imports/{result.Draft!.Id}",
                result.Draft
            ),
            RecipeImportCreateStatus.ValidationFailure => Results.BadRequest(
                new { message = result.ErrorMessage }
            ),
            RecipeImportCreateStatus.UnsupportedSource => Results.UnprocessableEntity(
                new { message = result.ErrorMessage }
            ),
            _ => Results.StatusCode(StatusCodes.Status500InternalServerError),
        };
    }

    private static async Task<IResult> GetDraft(
        Guid draftId,
        IRecipeImportService importService,
        CancellationToken cancellationToken
    )
    {
        var draft = await importService.GetDraftAsync(draftId, cancellationToken);
        return draft is null ? Results.NotFound() : Results.Ok(draft);
    }

    private static async Task<IResult> UpdateDraft(
        Guid draftId,
        UpdateRecipeImportDraftRequest request,
        IRecipeImportService importService,
        CancellationToken cancellationToken
    )
    {
        var result = await importService.UpdateDraftAsync(draftId, request, cancellationToken);
        return result.Status switch
        {
            RecipeImportUpdateStatus.Success => Results.Ok(result.Draft),
            RecipeImportUpdateStatus.NotFound => Results.NotFound(),
            RecipeImportUpdateStatus.ValidationFailure => Results.BadRequest(
                new { message = result.ErrorMessage }
            ),
            _ => Results.StatusCode(StatusCodes.Status500InternalServerError),
        };
    }

    private static async Task<IResult> FinalizeDraft(
        Guid draftId,
        IRecipeImportService importService,
        CancellationToken cancellationToken
    )
    {
        var result = await importService.FinalizeDraftAsync(draftId, cancellationToken);
        return result.Status switch
        {
            RecipeImportFinalizeStatus.Success => Results.Created(
                $"/api/recipes/{result.Recipe!.Id}",
                result.Recipe
            ),
            RecipeImportFinalizeStatus.NotFound => Results.NotFound(),
            RecipeImportFinalizeStatus.Blocked => Results.BadRequest(result.Draft),
            RecipeImportFinalizeStatus.Conflict => Results.Conflict(
                new { message = result.ErrorMessage, draft = result.Draft }
            ),
            RecipeImportFinalizeStatus.AlreadyFinalized => Results.Conflict(
                new
                {
                    message = "Import draft has already been finalized.",
                    recipeId = result.FinalizedRecipeId,
                }
            ),
            _ => Results.StatusCode(StatusCodes.Status500InternalServerError),
        };
    }
}

using Microsoft.EntityFrameworkCore;
using WhatToCook.Api.Domain.Recipes;
using WhatToCook.Api.Features.Recipes.Shared;
using WhatToCook.Api.Infrastructure.Data;
using SharedRecipeDetailsResponse = WhatToCook.Api.Features.Recipes.Shared.RecipeDetailsResponse;
using SharedRecipeIngredientRequest = WhatToCook.Api.Features.Recipes.Shared.RecipeIngredientRequest;

namespace WhatToCook.Api.Features.Recipes.Imports;

public interface IRecipeImportService
{
    Task<RecipeImportCreateResult> CreateFromUrlAsync(
        string url,
        CancellationToken cancellationToken
    );

    Task<RecipeImportDraftResponse?> GetDraftAsync(
        Guid draftId,
        CancellationToken cancellationToken
    );

    Task<RecipeImportUpdateResult> UpdateDraftAsync(
        Guid draftId,
        UpdateRecipeImportDraftRequest request,
        CancellationToken cancellationToken
    );

    Task<RecipeImportFinalizeResult> FinalizeDraftAsync(
        Guid draftId,
        CancellationToken cancellationToken
    );
}

public sealed class RecipeImportService(
    AppDbContext dbContext,
    IRecipeImportPageFetcher pageFetcher,
    IRecipeImportSourceAdapterResolver sourceAdapterResolver,
    IRecipeWriteService recipeWriteService
) : IRecipeImportService
{
    public async Task<RecipeImportCreateResult> CreateFromUrlAsync(
        string url,
        CancellationToken cancellationToken
    )
    {
        if (
            !Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        )
        {
            return RecipeImportCreateResult.ValidationFailure("A valid recipe URL is required.");
        }

        var adapter = sourceAdapterResolver.Resolve(uri);
        if (adapter is null)
        {
            return RecipeImportCreateResult.UnsupportedSource(
                "This recipe source is not supported yet."
            );
        }

        string html;
        try
        {
            html = await pageFetcher.FetchAsync(uri, cancellationToken);
        }
        catch
        {
            return RecipeImportCreateResult.UnsupportedSource(
                "The recipe page could not be fetched or read."
            );
        }

        var extraction = adapter.Extract(uri, html);
        if (!extraction.IsSuccess || extraction.Draft is null)
        {
            return RecipeImportCreateResult.UnsupportedSource(
                extraction.ErrorMessage ?? "The recipe page could not be extracted into a draft."
            );
        }

        var draft = new RecipeImportDraft
        {
            SourceType = RecipeImportSourceType.Url,
            SourceUrl = uri.ToString(),
            RawContent = extraction.Draft.RawContent,
            Title = extraction.Draft.Title,
            Servings = extraction.Draft.Servings,
            Source = extraction.Draft.Source,
        };
        draft.Ingredients = extraction
            .Draft.Ingredients.Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Select(
                (x, index) =>
                    new RecipeImportIngredient
                    {
                        DraftId = draft.Id,
                        Name = x.Name.Trim(),
                        QuantityText = string.IsNullOrWhiteSpace(x.QuantityText)
                            ? null
                            : x.QuantityText.Trim(),
                        Unit = string.IsNullOrWhiteSpace(x.Unit) ? null : x.Unit.Trim(),
                        SortOrder = index,
                    }
            )
            .ToList();
        draft.Steps = extraction
            .Draft.Steps.Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .Select(
                (x, index) =>
                    new RecipeImportStep
                    {
                        DraftId = draft.Id,
                        Text = x,
                        SortOrder = index,
                    }
            )
            .ToList();
        draft.Tags = extraction
            .Draft.Tags.Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .Select(
                (x, index) =>
                    new RecipeImportTag
                    {
                        DraftId = draft.Id,
                        Text = x,
                        SortOrder = index,
                    }
            )
            .ToList();
        var evaluation = await EvaluateDraftAsync(
            draft.Title,
            draft.Servings,
            extraction.Draft.Ingredients.ToList(),
            extraction.Draft.Steps.ToList(),
            extraction.Draft.Tags.ToList(),
            cancellationToken
        );
        draft.Status = evaluation.Status;
        draft.Issues = CreateIssueEntities(draft.Id, evaluation.Issues).ToList();

        dbContext.RecipeImportDrafts.Add(draft);
        await dbContext.SaveChangesAsync(cancellationToken);

        return RecipeImportCreateResult.Success(MapDraft(draft));
    }

    public async Task<RecipeImportDraftResponse?> GetDraftAsync(
        Guid draftId,
        CancellationToken cancellationToken
    )
    {
        var draft = await LoadDraftAsync(draftId, cancellationToken);
        return draft is null ? null : MapDraft(draft);
    }

    public async Task<RecipeImportUpdateResult> UpdateDraftAsync(
        Guid draftId,
        UpdateRecipeImportDraftRequest request,
        CancellationToken cancellationToken
    )
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );
        var draft = await dbContext.RecipeImportDrafts.SingleOrDefaultAsync(
            x => x.Id == draftId,
            cancellationToken
        );
        if (draft is null)
        {
            return RecipeImportUpdateResult.NotFound();
        }

        if (draft.Status == RecipeImportStatus.Finalized)
        {
            return RecipeImportUpdateResult.ValidationFailure(
                "Finalized import drafts cannot be edited."
            );
        }

        draft.Title = request.Title?.Trim();
        draft.Servings = request.Servings;
        draft.Source = string.IsNullOrWhiteSpace(request.Source) ? null : request.Source.Trim();
        draft.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext
            .RecipeImportIngredients.Where(x => x.DraftId == draftId)
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext
            .RecipeImportSteps.Where(x => x.DraftId == draftId)
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext
            .RecipeImportTags.Where(x => x.DraftId == draftId)
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext
            .RecipeImportIssues.Where(x => x.DraftId == draftId)
            .ExecuteDeleteAsync(cancellationToken);

        var sanitizedIngredients = request
            .Ingredients.Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => new RecipeImportIngredientRequest
            {
                Name = x.Name.Trim(),
                QuantityText = string.IsNullOrWhiteSpace(x.QuantityText)
                    ? null
                    : x.QuantityText.Trim(),
                Unit = string.IsNullOrWhiteSpace(x.Unit) ? null : x.Unit.Trim(),
            })
            .ToList();
        var sanitizedSteps = request.Steps.Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        var sanitizedTags = request.Tags.Select(x => x.Trim()).Where(x => x.Length > 0).ToList();

        dbContext.RecipeImportIngredients.AddRange(
            sanitizedIngredients.Select(
                (x, index) =>
                    new RecipeImportIngredient
                    {
                        DraftId = draftId,
                        Name = x.Name,
                        QuantityText = x.QuantityText,
                        Unit = x.Unit,
                        SortOrder = index,
                    }
            )
        );
        dbContext.RecipeImportSteps.AddRange(
            sanitizedSteps.Select(
                (x, index) =>
                    new RecipeImportStep
                    {
                        DraftId = draftId,
                        Text = x,
                        SortOrder = index,
                    }
            )
        );
        dbContext.RecipeImportTags.AddRange(
            sanitizedTags.Select(
                (x, index) =>
                    new RecipeImportTag
                    {
                        DraftId = draftId,
                        Text = x,
                        SortOrder = index,
                    }
            )
        );

        var evaluation = await EvaluateDraftAsync(
            draft.Title,
            draft.Servings,
            sanitizedIngredients,
            sanitizedSteps,
            sanitizedTags,
            cancellationToken
        );
        draft.Status = evaluation.Status;
        dbContext.RecipeImportIssues.AddRange(CreateIssueEntities(draftId, evaluation.Issues));
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return RecipeImportUpdateResult.Success((await GetDraftAsync(draftId, cancellationToken))!);
    }

    public async Task<RecipeImportFinalizeResult> FinalizeDraftAsync(
        Guid draftId,
        CancellationToken cancellationToken
    )
    {
        var draft = await LoadDraftAsync(draftId, cancellationToken);
        if (draft is null)
        {
            return RecipeImportFinalizeResult.NotFound();
        }

        if (draft.Status == RecipeImportStatus.Finalized)
        {
            return RecipeImportFinalizeResult.AlreadyFinalized(draft.FinalizedRecipeId);
        }

        var ingredientRequests = draft
            .Ingredients.OrderBy(x => x.SortOrder)
            .Select(x => new RecipeImportIngredientRequest
            {
                Name = x.Name,
                QuantityText = x.QuantityText,
                Unit = x.Unit,
            })
            .ToList();
        var steps = draft.Steps.OrderBy(x => x.SortOrder).Select(x => x.Text).ToList();
        var tags = draft.Tags.OrderBy(x => x.SortOrder).Select(x => x.Text).ToList();

        var evaluation = await EvaluateDraftAsync(
            draft.Title,
            draft.Servings,
            ingredientRequests,
            steps,
            tags,
            cancellationToken
        );
        if (evaluation.Status != RecipeImportStatus.ReadyToFinalize)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                cancellationToken
            );
            var trackedDraft = await dbContext.RecipeImportDrafts.SingleAsync(
                x => x.Id == draftId,
                cancellationToken
            );
            trackedDraft.Status = evaluation.Status;
            trackedDraft.UpdatedAt = DateTimeOffset.UtcNow;
            await dbContext
                .RecipeImportIssues.Where(x => x.DraftId == draftId)
                .ExecuteDeleteAsync(cancellationToken);
            dbContext.RecipeImportIssues.AddRange(CreateIssueEntities(draftId, evaluation.Issues));
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return RecipeImportFinalizeResult.Blocked(
                (await GetDraftAsync(draftId, cancellationToken))!
            );
        }

        var request = new RecipeUpsertRequest
        {
            Title = draft.Title ?? string.Empty,
            Servings = draft.Servings ?? 0,
            Source = draft.Source,
            Ingredients = ingredientRequests
                .Select(x => new SharedRecipeIngredientRequest
                {
                    Name = x.Name,
                    QuantityText = x.QuantityText,
                    Unit = x.Unit,
                })
                .ToList(),
            Steps = steps,
            Tags = tags,
        };

        await using var finalizeTransaction = await dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );
        var result = await recipeWriteService.CreateAsync(request, cancellationToken);
        if (result.Status == RecipeWriteStatus.ValidationFailure)
        {
            var trackedDraft = await dbContext.RecipeImportDrafts.SingleAsync(
                x => x.Id == draftId,
                cancellationToken
            );
            trackedDraft.Status = RecipeImportStatus.NeedsReview;
            trackedDraft.UpdatedAt = DateTimeOffset.UtcNow;
            await dbContext
                .RecipeImportIssues.Where(x => x.DraftId == draftId)
                .ExecuteDeleteAsync(cancellationToken);
            var issues = result
                .ValidationErrors.SelectMany(pair =>
                    pair.Value.Select(message => new DraftIssue(
                        pair.Key,
                        "validation",
                        message,
                        RecipeImportIssueSeverity.Blocker
                    ))
                )
                .ToList();
            dbContext.RecipeImportIssues.AddRange(CreateIssueEntities(draftId, issues));
            await dbContext.SaveChangesAsync(cancellationToken);
            await finalizeTransaction.CommitAsync(cancellationToken);
            return RecipeImportFinalizeResult.Blocked(
                (await GetDraftAsync(draftId, cancellationToken))!
            );
        }

        if (result.Status == RecipeWriteStatus.Conflict)
        {
            var trackedDraft = await dbContext.RecipeImportDrafts.SingleAsync(
                x => x.Id == draftId,
                cancellationToken
            );
            trackedDraft.Status = RecipeImportStatus.NeedsReview;
            trackedDraft.UpdatedAt = DateTimeOffset.UtcNow;
            await dbContext
                .RecipeImportIssues.Where(x => x.DraftId == draftId)
                .ExecuteDeleteAsync(cancellationToken);
            dbContext.RecipeImportIssues.Add(
                new RecipeImportIssue
                {
                    DraftId = draftId,
                    FieldPath = nameof(draft.Title),
                    Code = "duplicate-title",
                    Message = result.ErrorMessage ?? "Recipe title must be unique.",
                    Severity = RecipeImportIssueSeverity.Blocker,
                    SortOrder = 0,
                }
            );
            await dbContext.SaveChangesAsync(cancellationToken);
            await finalizeTransaction.CommitAsync(cancellationToken);
            return RecipeImportFinalizeResult.Conflict(
                (await GetDraftAsync(draftId, cancellationToken))!,
                result.ErrorMessage
            );
        }

        var finalizedDraft = await dbContext.RecipeImportDrafts.SingleAsync(
            x => x.Id == draftId,
            cancellationToken
        );
        finalizedDraft.Status = RecipeImportStatus.Finalized;
        finalizedDraft.FinalizedRecipeId = result.Recipe!.Id;
        finalizedDraft.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext
            .RecipeImportIssues.Where(x => x.DraftId == draftId)
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await finalizeTransaction.CommitAsync(cancellationToken);

        return RecipeImportFinalizeResult.Success(result.Recipe!);
    }

    private async Task<RecipeImportDraft?> LoadDraftAsync(
        Guid draftId,
        CancellationToken cancellationToken
    )
    {
        return await dbContext
            .RecipeImportDrafts.AsNoTracking()
            .Include(x => x.Ingredients)
            .Include(x => x.Steps)
            .Include(x => x.Tags)
            .Include(x => x.Issues)
            .SingleOrDefaultAsync(x => x.Id == draftId, cancellationToken);
    }

    private async Task<DraftEvaluation> EvaluateDraftAsync(
        string? title,
        int? servings,
        IReadOnlyList<RecipeImportIngredientRequest> ingredients,
        IReadOnlyList<string> steps,
        IReadOnlyList<string> tags,
        CancellationToken cancellationToken
    )
    {
        var request = new RecipeUpsertRequest
        {
            Title = title ?? string.Empty,
            Servings = servings ?? 0,
            Ingredients = ingredients
                .Select(x => new SharedRecipeIngredientRequest
                {
                    Name = x.Name,
                    QuantityText = x.QuantityText,
                    Unit = x.Unit,
                })
                .ToList(),
            Steps = steps.ToList(),
            Tags = tags.ToList(),
        };

        var validationErrors = RecipeWriteValidator.Validate(request);
        var issues = new List<DraftIssue>();
        foreach (var (fieldPath, messages) in validationErrors)
        {
            foreach (var message in messages)
            {
                issues.Add(
                    new DraftIssue(
                        fieldPath,
                        "validation",
                        message,
                        RecipeImportIssueSeverity.Blocker
                    )
                );
            }
        }

        if (!string.IsNullOrWhiteSpace(title))
        {
            var normalizedTitle = TextNormalizer.Normalize(title);
            var titleExists = await dbContext.Recipes.AnyAsync(
                x => x.NormalizedTitle == normalizedTitle,
                cancellationToken
            );

            if (titleExists)
            {
                issues.Add(
                    new DraftIssue(
                        nameof(RecipeImportDraft.Title),
                        "duplicate-title",
                        "Recipe title must be unique.",
                        RecipeImportIssueSeverity.Blocker
                    )
                );
            }
        }

        var status = issues.Any(x => x.Severity == RecipeImportIssueSeverity.Blocker)
            ? RecipeImportStatus.NeedsReview
            : RecipeImportStatus.ReadyToFinalize;

        return new DraftEvaluation(status, issues);
    }

    private static IReadOnlyList<RecipeImportIssue> CreateIssueEntities(
        Guid draftId,
        IReadOnlyList<DraftIssue> issues
    )
    {
        return issues
            .Select(
                (x, index) =>
                    new RecipeImportIssue
                    {
                        DraftId = draftId,
                        FieldPath = x.FieldPath,
                        Code = x.Code,
                        Message = x.Message,
                        Severity = x.Severity,
                        SortOrder = index,
                    }
            )
            .ToList();
    }

    private static RecipeImportDraftResponse MapDraft(RecipeImportDraft draft)
    {
        return new RecipeImportDraftResponse(
            draft.Id,
            draft.SourceType.ToString().ToLowerInvariant(),
            draft.Status.ToString().ToLowerInvariant(),
            draft.SourceUrl,
            draft.Title,
            draft.Servings,
            draft.Source,
            draft.Status == RecipeImportStatus.ReadyToFinalize,
            draft.FinalizedRecipeId,
            draft
                .Ingredients.OrderBy(x => x.SortOrder)
                .Select(x => new RecipeImportIngredientResponse(x.Name, x.QuantityText, x.Unit))
                .ToList(),
            draft.Steps.OrderBy(x => x.SortOrder).Select(x => x.Text).ToList(),
            draft.Tags.OrderBy(x => x.SortOrder).Select(x => x.Text).ToList(),
            draft
                .Issues.OrderBy(x => x.SortOrder)
                .Select(x => new RecipeImportIssueResponse(
                    x.FieldPath,
                    x.Code,
                    x.Message,
                    x.Severity.ToString().ToLowerInvariant()
                ))
                .ToList()
        );
    }

    private sealed record DraftEvaluation(
        RecipeImportStatus Status,
        IReadOnlyList<DraftIssue> Issues
    );

    private sealed record DraftIssue(
        string FieldPath,
        string Code,
        string Message,
        RecipeImportIssueSeverity Severity
    );
}

public sealed record RecipeImportCreateResult(
    RecipeImportCreateStatus Status,
    RecipeImportDraftResponse? Draft,
    string? ErrorMessage
)
{
    public static RecipeImportCreateResult Success(RecipeImportDraftResponse draft) =>
        new(RecipeImportCreateStatus.Success, draft, null);

    public static RecipeImportCreateResult ValidationFailure(string message) =>
        new(RecipeImportCreateStatus.ValidationFailure, null, message);

    public static RecipeImportCreateResult UnsupportedSource(string message) =>
        new(RecipeImportCreateStatus.UnsupportedSource, null, message);
}

public enum RecipeImportCreateStatus
{
    Success = 1,
    ValidationFailure = 2,
    UnsupportedSource = 3,
}

public sealed record RecipeImportUpdateResult(
    RecipeImportUpdateStatus Status,
    RecipeImportDraftResponse? Draft,
    string? ErrorMessage
)
{
    public static RecipeImportUpdateResult Success(RecipeImportDraftResponse draft) =>
        new(RecipeImportUpdateStatus.Success, draft, null);

    public static RecipeImportUpdateResult NotFound() =>
        new(RecipeImportUpdateStatus.NotFound, null, null);

    public static RecipeImportUpdateResult ValidationFailure(string message) =>
        new(RecipeImportUpdateStatus.ValidationFailure, null, message);
}

public enum RecipeImportUpdateStatus
{
    Success = 1,
    NotFound = 2,
    ValidationFailure = 3,
}

public sealed record RecipeImportFinalizeResult(
    RecipeImportFinalizeStatus Status,
    SharedRecipeDetailsResponse? Recipe,
    RecipeImportDraftResponse? Draft,
    string? ErrorMessage,
    Guid? FinalizedRecipeId
)
{
    public static RecipeImportFinalizeResult Success(SharedRecipeDetailsResponse recipe) =>
        new(RecipeImportFinalizeStatus.Success, recipe, null, null, recipe.Id);

    public static RecipeImportFinalizeResult NotFound() =>
        new(RecipeImportFinalizeStatus.NotFound, null, null, null, null);

    public static RecipeImportFinalizeResult Blocked(RecipeImportDraftResponse draft) =>
        new(RecipeImportFinalizeStatus.Blocked, null, draft, null, null);

    public static RecipeImportFinalizeResult Conflict(
        RecipeImportDraftResponse draft,
        string? errorMessage
    ) => new(RecipeImportFinalizeStatus.Conflict, null, draft, errorMessage, null);

    public static RecipeImportFinalizeResult AlreadyFinalized(Guid? recipeId) =>
        new(RecipeImportFinalizeStatus.AlreadyFinalized, null, null, null, recipeId);
}

public enum RecipeImportFinalizeStatus
{
    Success = 1,
    NotFound = 2,
    Blocked = 3,
    Conflict = 4,
    AlreadyFinalized = 5,
}

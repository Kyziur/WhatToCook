using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WhatToCook.Web.Components.Features.Recipes.Shared;

namespace WhatToCook.Web.Components.Features.Recipes.Import.Services;

public sealed class ApiRecipeImportService(HttpClient httpClient) : IRecipeImportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<RecipeImportCreateResult> CreateFromUrlAsync(
        string url,
        CancellationToken cancellationToken = default
    )
    {
        var response = await httpClient.PostAsJsonAsync(
            "/api/recipe-imports/url",
            new CreateImportRequestDto(url),
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var draft = await response.Content.ReadFromJsonAsync<RecipeImportDraftDto>(
                JsonOptions,
                cancellationToken
            );
            return draft is null
                ? RecipeImportCreateResult.Failure(
                    "Nie udało się odczytać utworzonego szkicu importu."
                )
                : RecipeImportCreateResult.Success(MapDraft(draft));
        }

        var message = await ReadMessageAsync(response, cancellationToken);
        return RecipeImportCreateResult.Failure(
            message ?? "Nie udało się utworzyć szkicu importu."
        );
    }

    public async Task<RecipeImportDraft?> GetDraftAsync(
        Guid draftId,
        CancellationToken cancellationToken = default
    )
    {
        var response = await httpClient.GetAsync(
            $"/api/recipe-imports/{draftId}",
            cancellationToken
        );
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var draft = await response.Content.ReadFromJsonAsync<RecipeImportDraftDto>(
            JsonOptions,
            cancellationToken
        );
        return draft is null ? null : MapDraft(draft);
    }

    public async Task<RecipeImportUpdateResult> UpdateDraftAsync(
        Guid draftId,
        RecipeImportDraftUpdateRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var response = await httpClient.PutAsJsonAsync(
            $"/api/recipe-imports/{draftId}",
            new UpdateDraftRequestDto(
                request.Title,
                request.Servings,
                request.Source,
                request
                    .Ingredients.Select(x => new RecipeImportIngredientDto
                    {
                        Name = x.Name,
                        QuantityText = x.QuantityText,
                        Unit = x.Unit,
                    })
                    .ToList(),
                request.Steps.ToList(),
                request.Tags.ToList()
            ),
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var draft = await response.Content.ReadFromJsonAsync<RecipeImportDraftDto>(
                JsonOptions,
                cancellationToken
            );
            return draft is null
                ? RecipeImportUpdateResult.Failure("Nie udało się odczytać zapisanego szkicu.")
                : RecipeImportUpdateResult.Success(MapDraft(draft));
        }

        var message = await ReadMessageAsync(response, cancellationToken);
        return RecipeImportUpdateResult.Failure(message ?? "Nie udało się zapisać szkicu importu.");
    }

    public async Task<RecipeImportFinalizeResult> FinalizeDraftAsync(
        Guid draftId,
        CancellationToken cancellationToken = default
    )
    {
        var response = await httpClient.PostAsync(
            $"/api/recipe-imports/{draftId}/finalize",
            content: null,
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var recipe = await response.Content.ReadFromJsonAsync<RecipeDetailsDto>(
                JsonOptions,
                cancellationToken
            );
            return recipe is null
                ? RecipeImportFinalizeResult.Failure(
                    null,
                    "Nie udało się odczytać zapisanego przepisu."
                )
                : RecipeImportFinalizeResult.Success(MapRecipe(recipe));
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var draft = await response.Content.ReadFromJsonAsync<RecipeImportDraftDto>(
                JsonOptions,
                cancellationToken
            );
            return RecipeImportFinalizeResult.Failure(
                draft is null ? null : MapDraft(draft),
                "Szkic wymaga jeszcze poprawek przed finalizacją."
            );
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var conflict = await response.Content.ReadFromJsonAsync<FinalizeConflictDto>(
                JsonOptions,
                cancellationToken
            );
            return RecipeImportFinalizeResult.Failure(
                conflict?.Draft is null ? null : MapDraft(conflict.Draft),
                conflict?.Message ?? "Nie udało się sfinalizować importu."
            );
        }

        var message = await ReadMessageAsync(response, cancellationToken);
        return RecipeImportFinalizeResult.Failure(
            null,
            message ?? "Nie udało się sfinalizować importu."
        );
    }

    private static RecipeImportDraft MapDraft(RecipeImportDraftDto source) =>
        new(
            source.Id,
            source.SourceType,
            source.Status,
            source.SourceUrl,
            source.Title,
            source.Servings,
            source.Source,
            source.CanFinalize,
            source.FinalizedRecipeId,
            source
                .Ingredients.Select(x => new RecipeImportIngredient(x.Name, x.QuantityText, x.Unit))
                .ToList(),
            source.Steps.ToList(),
            source.Tags.ToList(),
            source
                .Issues.Select(x => new RecipeImportIssue(
                    x.FieldPath,
                    x.Code,
                    x.Message,
                    x.Severity
                ))
                .ToList()
        );

    private static RecipeCatalogItem MapRecipe(RecipeDetailsDto source) =>
        new(
            source.Id,
            source.Title,
            source.Servings,
            source.Source,
            source.MainPhotoPath,
            source.IsActive,
            source.Tags.ToList(),
            source
                .Ingredients.Select(x => new RecipeCatalogIngredient(
                    x.Ingredient,
                    x.QuantityText,
                    x.Unit
                ))
                .ToList(),
            source.Steps.ToList()
        );

    private static async Task<string?> ReadMessageAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken
    )
    {
        var payload = await response.Content.ReadFromJsonAsync<MessageDto>(
            JsonOptions,
            cancellationToken
        );
        return payload?.Message;
    }

    private sealed record CreateImportRequestDto(string Url);

    private sealed record UpdateDraftRequestDto(
        string? Title,
        int? Servings,
        string? Source,
        IReadOnlyList<RecipeImportIngredientDto> Ingredients,
        IReadOnlyList<string> Steps,
        IReadOnlyList<string> Tags
    );

    private sealed class RecipeImportDraftDto
    {
        public Guid Id { get; init; }
        public string SourceType { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string? SourceUrl { get; init; }
        public string? Title { get; init; }
        public int? Servings { get; init; }
        public string? Source { get; init; }
        public bool CanFinalize { get; init; }
        public Guid? FinalizedRecipeId { get; init; }
        public List<RecipeImportIngredientDto> Ingredients { get; init; } = [];
        public List<string> Steps { get; init; } = [];
        public List<string> Tags { get; init; } = [];
        public List<RecipeImportIssueDto> Issues { get; init; } = [];
    }

    private sealed class RecipeImportIngredientDto
    {
        public string Name { get; init; } = string.Empty;
        public string? QuantityText { get; init; }
        public string? Unit { get; init; }
    }

    private sealed class RecipeImportIssueDto
    {
        public string FieldPath { get; init; } = string.Empty;
        public string Code { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string Severity { get; init; } = string.Empty;
    }

    private sealed class FinalizeConflictDto
    {
        public string? Message { get; init; }
        public RecipeImportDraftDto? Draft { get; init; }
    }

    private sealed class RecipeDetailsDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public int Servings { get; init; }
        public string? Source { get; init; }
        public string? MainPhotoPath { get; init; }
        public bool IsActive { get; init; }
        public List<string> Tags { get; init; } = [];
        public List<RecipeIngredientDto> Ingredients { get; init; } = [];
        public List<string> Steps { get; init; } = [];
    }

    private sealed class RecipeIngredientDto
    {
        public string Ingredient { get; init; } = string.Empty;
        public string? QuantityText { get; init; }
        public string? Unit { get; init; }
    }

    private sealed class MessageDto
    {
        public string? Message { get; init; }
    }
}

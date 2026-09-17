using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace WhatToCook.Web.Components.Features.Recipes.Editor.Services;

public sealed class ApiRecipeEditorService(HttpClient httpClient) : IRecipeEditorService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<RecipeEditorDraft?> GetByIdAsync(
        Guid recipeId,
        CancellationToken cancellationToken = default
    )
    {
        var details = await httpClient.GetFromJsonAsync<RecipeDetailsDto>(
            $"/api/recipes/{recipeId}",
            JsonOptions,
            cancellationToken
        );

        if (details is null)
        {
            return null;
        }

        return new RecipeEditorDraft(
            details.Id,
            details.Title,
            details.Servings,
            details.Source,
            details.MainPhotoPath,
            details.Tags,
            details
                .Ingredients.Select(x => new RecipeSaveIngredient(
                    x.Ingredient,
                    x.QuantityText,
                    x.Unit
                ))
                .ToList(),
            details.Steps
        );
    }

    public async Task<RecipeSaveResult> SaveAsync(
        Guid? recipeId,
        RecipeSaveRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var endpoint = recipeId is null ? "/api/recipes" : $"/api/recipes/{recipeId}";
        var response = recipeId is null
            ? await httpClient.PostAsJsonAsync(endpoint, request, cancellationToken)
            : await httpClient.PutAsJsonAsync(endpoint, request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var details = await response.Content.ReadFromJsonAsync<RecipeDetailsDto>(
                JsonOptions,
                cancellationToken
            );
            return details is null
                ? RecipeSaveResult.Failure("Nie udalo sie odczytac odpowiedzi po zapisie.")
                : RecipeSaveResult.Success(details.Id);
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var validation = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(
                JsonOptions,
                cancellationToken
            );
            if (validation?.Errors is not null)
            {
                var errors = validation.Errors.ToDictionary(
                    pair => pair.Key,
                    pair => (IReadOnlyList<string>)pair.Value.ToList()
                );
                return RecipeSaveResult.Failure("Popraw oznaczone pola.", errors);
            }
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var conflict = await response.Content.ReadFromJsonAsync<ConflictDto>(
                JsonOptions,
                cancellationToken
            );
            return RecipeSaveResult.Failure(
                conflict?.Message ?? "Tytul przepisu musi byc unikalny."
            );
        }

        return RecipeSaveResult.Failure("Nie udalo sie zapisac przepisu. Sprobuj ponownie.");
    }

    public async Task<RecipePhotoUploadResult> UploadPhotoAsync(
        Guid recipeId,
        Stream stream,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken = default
    )
    {
        await using var bufferedStream = new MemoryStream();
        await stream.CopyToAsync(bufferedStream, cancellationToken);
        var bufferedFile = bufferedStream.ToArray();

        using var formData = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(bufferedFile);
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                contentType
            );
        }

        formData.Add(fileContent, "file", fileName);

        var response = await httpClient.PostAsync(
            $"/api/recipes/{recipeId}/photo",
            formData,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            return RecipePhotoUploadResult.Failure("Nie udalo sie przeslac zdjecia.");
        }

        var payload = await response.Content.ReadFromJsonAsync<UploadPhotoResponseDto>(
            JsonOptions,
            cancellationToken
        );

        return RecipePhotoUploadResult.Success(payload?.MainPhotoPath);
    }

    public async Task<IReadOnlyList<string>> GetIngredientSuggestionsAsync(
        string query,
        CancellationToken cancellationToken = default
    )
    {
        var encodedQuery = Uri.EscapeDataString(query ?? string.Empty);
        var suggestions = await httpClient.GetFromJsonAsync<List<string>>(
            $"/api/recipes/suggestions/ingredients?q={encodedQuery}",
            JsonOptions,
            cancellationToken
        );

        return suggestions ?? [];
    }

    public async Task<IReadOnlyList<string>> GetTagSuggestionsAsync(
        string query,
        CancellationToken cancellationToken = default
    )
    {
        var encodedQuery = Uri.EscapeDataString(query ?? string.Empty);
        var suggestions = await httpClient.GetFromJsonAsync<List<string>>(
            $"/api/recipes/suggestions/tags?q={encodedQuery}",
            JsonOptions,
            cancellationToken
        );

        return suggestions ?? [];
    }

    private sealed class RecipeDetailsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Servings { get; set; }
        public string? Source { get; set; }
        public string? MainPhotoPath { get; set; }
        public List<string> Tags { get; set; } = [];
        public List<RecipeIngredientDto> Ingredients { get; set; } = [];
        public List<string> Steps { get; set; } = [];
    }

    private sealed class RecipeIngredientDto
    {
        public string Ingredient { get; set; } = string.Empty;
        public string? QuantityText { get; set; }
        public string? Unit { get; set; }
    }

    private sealed class UploadPhotoResponseDto
    {
        public string? MainPhotoPath { get; set; }
    }

    private sealed class ConflictDto
    {
        public string? Message { get; set; }
    }
}

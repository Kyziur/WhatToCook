using Microsoft.AspNetCore.Components;
using WhatToCook.Web.Components.Features.Recipes.Import.Services;

namespace WhatToCook.Web.Components.Features.Recipes.Import.Pages;

public partial class RecipeImportPage : IDisposable
{
    [Inject]
    private IRecipeImportService RecipeImportService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private string ImportUrl { get; set; } = string.Empty;
    private string FeedbackMessage { get; set; } = string.Empty;
    private bool IsError { get; set; }
    private bool IsSubmitting { get; set; }
    private bool IsInteractive { get; set; }
    private readonly CancellationTokenSource lifetimeCts = new();

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            IsInteractive = true;
            StateHasChanged();
        }
    }

    public void Dispose()
    {
        lifetimeCts.Cancel();
        lifetimeCts.Dispose();
    }

    private async Task CreateDraftAsync()
    {
        if (IsSubmitting)
        {
            return;
        }

        IsSubmitting = true;
        FeedbackMessage = string.Empty;
        IsError = false;

        RecipeImportCreateResult result;
        try
        {
            result = await RecipeImportService.CreateFromUrlAsync(ImportUrl, lifetimeCts.Token);
        }
        catch (OperationCanceledException) when (lifetimeCts.IsCancellationRequested)
        {
            IsSubmitting = false;
            return;
        }
        catch
        {
            IsSubmitting = false;
            FeedbackMessage = "Nie udało się utworzyć szkicu importu. Spróbuj ponownie za chwilę.";
            IsError = true;
            return;
        }
        IsSubmitting = false;

        if (!result.IsSuccess || result.Draft is null)
        {
            FeedbackMessage = result.ErrorMessage ?? "Nie udalo sie utworzyc szkicu importu.";
            IsError = true;
            return;
        }

        NavigationManager.NavigateTo($"/recipes/imports/{result.Draft.Id}");
    }
}

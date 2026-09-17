using Microsoft.Extensions.DependencyInjection;

namespace WhatToCook.Api.Features.Recipes.Imports;

public static class RecipeImportServiceCollectionExtensions
{
    public static IServiceCollection AddRecipeImportSources(this IServiceCollection services)
    {
        services.AddScoped<IRecipeImportSourceAdapterResolver, RecipeImportSourceAdapterResolver>();
        services.AddScoped<IRecipeImportSourceAdapter, AniaGotujeRecipeImportSourceAdapter>();
        services.AddScoped<IRecipeImportSourceAdapter, RozkosznyRecipeImportSourceAdapter>();
        services.AddScoped<IRecipeImportSourceAdapter, JadlonomiaRecipeImportSourceAdapter>();
        services.AddScoped<IRecipeImportSourceAdapter, MojeWypiekiRecipeImportSourceAdapter>();
        return services;
    }
}

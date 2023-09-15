using Microsoft.Extensions.DependencyInjection;

namespace WhatToCook.Application.Services;

public static class ApllicationServices
{
    public static void RegisterApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<RecipeService>();
        services.AddScoped<RecipeServiceQuery>();
        services.AddScoped<MealPlanningService>();
        services.AddScoped<MealPlanningServiceQuery>();
    }
}
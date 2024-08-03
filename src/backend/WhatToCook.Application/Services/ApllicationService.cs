using Microsoft.Extensions.DependencyInjection;

namespace WhatToCook.Application.Services;

public static class ApllicationServices
{
    public static void RegisterApplicationServices(this IServiceCollection services)
    {
        _ = services.AddScoped<RecipeService>();
        _ = services.AddScoped<RecipeServiceQuery>();
        _ = services.AddScoped<MealPlanningService>();
        _ = services.AddScoped<MealPlanningServiceQuery>();
    }
}
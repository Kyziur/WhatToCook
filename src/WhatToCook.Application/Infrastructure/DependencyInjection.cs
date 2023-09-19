using Microsoft.Extensions.DependencyInjection;
using WhatToCook.Application.Infrastructure.Repositories;

namespace WhatToCook.Application.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        return services.AddDbContext<DatabaseContext>()
            .AddScoped<IRecipesRepository, RecipesRepository>()
            .AddScoped<IMealPlanningRepository, MealPlanningRepository>();
    }
}
using Microsoft.Extensions.DependencyInjection;

namespace WhatToCook.Application.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        return services.AddDbContext<DatabaseContext>();
    }
}

using Microsoft.EntityFrameworkCore;

namespace WhatToCook.Api.Infrastructure.Data;

public static class AppDbContextRegistration
{
    public static IServiceCollection AddAppDbContext(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(
            (serviceProvider, options) =>
            {
                var appConfiguration = serviceProvider.GetRequiredService<IConfiguration>();
                options.UseNpgsql(AppDbConnectionString.Resolve(appConfiguration));
            }
        );

        return services;
    }
}

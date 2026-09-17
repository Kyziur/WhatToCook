using Microsoft.EntityFrameworkCore;

namespace WhatToCook.Api.Infrastructure.Data;

public static class AppDbInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        IConfiguration configuration,
        IHostEnvironment environment,
        CancellationToken cancellationToken
    )
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);

        await RecipeCatalogSeeder.SeedIfEnabledAsync(
            dbContext,
            configuration,
            environment,
            cancellationToken
        );
    }
}

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using WhatToCook.Api.Features.Recipes.Imports;
using WhatToCook.Api.Infrastructure.Data;

namespace WhatToCook.Api.IntegrationTests.Specs;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer postgresContainer = new PostgreSqlBuilder(
        "postgres:17-alpine"
    )
        .WithDatabase($"wtc-tests-{Guid.NewGuid():N}")
        .WithUsername("wtc")
        .WithPassword("wtc")
        .Build();

    public string StorageRootPath { get; } =
        Path.Combine(Path.GetTempPath(), $"wtc-images-{Guid.NewGuid():N}");

    public Dictionary<string, string> ImportedPagesByUrl { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    protected virtual bool EnableSeedData => false;
    protected virtual string? SeedDataProfile => null;

    public Task InitializeAsync() => postgresContainer.StartAsync();

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await postgresContainer.DisposeAsync();
        TryDeleteDirectory(StorageRootPath);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(
            (_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection([
                    new KeyValuePair<string, string?>(
                        "ConnectionStrings:whattocook-db",
                        postgresContainer.GetConnectionString()
                    ),
                    new KeyValuePair<string, string?>(
                        "SeedData:Enabled",
                        EnableSeedData.ToString()
                    ),
                    new KeyValuePair<string, string?>("SeedData:Profile", SeedDataProfile),
                    new KeyValuePair<string, string?>("Storage:RecipeImagesPath", StorageRootPath),
                ]);
            }
        );

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.RemoveAll<IRecipeImportPageFetcher>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(postgresContainer.GetConnectionString())
            );
            services.AddSingleton<IRecipeImportPageFetcher>(
                new FakeRecipeImportPageFetcher(ImportedPagesByUrl)
            );

            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.Migrate();
        });
    }

    private static void TryDeleteDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        try
        {
            Directory.Delete(path, recursive: true);
        }
        catch (IOException)
        {
            // Ignore cleanup failures in test temp files.
        }
        catch (UnauthorizedAccessException)
        {
            // Ignore cleanup failures in test temp files.
        }
    }

    private sealed class FakeRecipeImportPageFetcher(Dictionary<string, string> pagesByUrl)
        : IRecipeImportPageFetcher
    {
        public Task<string> FetchAsync(Uri uri, CancellationToken cancellationToken)
        {
            if (pagesByUrl.TryGetValue(uri.ToString(), out var html))
            {
                return Task.FromResult(html);
            }

            throw new HttpRequestException($"No test HTML registered for {uri}.");
        }
    }
}

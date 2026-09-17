using Microsoft.Extensions.Configuration;
using WhatToCook.Api.Infrastructure.Data;

namespace WhatToCook.Api.IntegrationTests.Specs;

public sealed class AppDbConnectionStringTests
{
    [Fact]
    public void Resolve_ShouldFailWhenNoDatabaseConnectionStringIsConfigured()
    {
        var configuration = new ConfigurationBuilder().Build();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            AppDbConnectionString.Resolve(configuration)
        );

        Assert.Contains(
            "database connection string is required",
            exception.Message,
            StringComparison.OrdinalIgnoreCase
        );
    }

    [Fact]
    public void Resolve_ShouldAcceptAspirePostgresConnectionStringAlias()
    {
        var expected = "Host=postgres;Database=whattocook;Username=app;Password=from-secret";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?> { ["ConnectionStrings:postgres"] = expected }
            )
            .Build();

        var actual = AppDbConnectionString.Resolve(configuration);

        Assert.Equal(expected, actual);
    }
}

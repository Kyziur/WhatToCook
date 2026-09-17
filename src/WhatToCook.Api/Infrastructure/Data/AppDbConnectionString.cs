namespace WhatToCook.Api.Infrastructure.Data;

public static class AppDbConnectionString
{
    public static string Resolve(IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("whattocook-db")
            ?? configuration.GetConnectionString("postgres");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "A database connection string is required. Configure ConnectionStrings:whattocook-db."
            );
        }

        return connectionString;
    }
}

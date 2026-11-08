using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace WhatToCook.Api.IntegrationTests.Specs;

public sealed class RecipeLoggingPrivacySpecificationTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task RecipeWriteLogs_ShouldNotContainUserTitlesOrNormalizedTitles()
    {
        var logger = new CapturingEditorLoggerProvider();
        using var app = factory.WithWebHostBuilder(builder =>
            builder.ConfigureLogging(logging => logging.AddProvider(logger))
        );
        using var client = app.CreateClient();

        var marker = $"SensitiveRecipeTitle{Guid.NewGuid():N}";
        var valid = RecipeApiContracts.CreateRecipePayload(marker);
        using var createdResponse = await client.PostAsJsonAsync("/api/recipes", valid);
        Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
        var created =
            await createdResponse.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeDetailsDto>();
        Assert.NotNull(created);

        using var duplicateResponse = await client.PostAsJsonAsync("/api/recipes", valid);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        var invalid = RecipeApiContracts.CreateRecipePayload(marker + new string('x', 201));
        using var invalidResponse = await client.PostAsJsonAsync("/api/recipes", invalid);
        Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);

        var entries = logger.Entries.ToArray();
        Assert.Contains(entries, x => x.Message.Contains($"Created recipe {created.Id}"));
        Assert.Contains(entries, x => x.Message.Contains("title conflicts"));
        Assert.Contains(entries, x => x.Message.Contains("failed validation"));
        Assert.All(
            entries,
            entry =>
            {
                Assert.DoesNotContain(marker, entry.Message, StringComparison.OrdinalIgnoreCase);
                Assert.All(
                    entry.Fields,
                    field =>
                    {
                        Assert.DoesNotContain(
                            marker,
                            field.Value?.ToString() ?? string.Empty,
                            StringComparison.OrdinalIgnoreCase
                        );
                        Assert.DoesNotContain(
                            field.Key,
                            new[] { "Title", "NormalizedTitle", "PhotoPath", "StoredPath" }
                        );
                    }
                );
            }
        );
    }

    private sealed record LogEntry(
        string Message,
        IReadOnlyList<KeyValuePair<string, object?>> Fields
    );

    private sealed class CapturingEditorLoggerProvider : ILoggerProvider
    {
        public ConcurrentQueue<LogEntry> Entries { get; } = new();

        public ILogger CreateLogger(string categoryName) =>
            categoryName.Contains("EditorEndpointsLog", StringComparison.Ordinal)
                ? new CapturingLogger(Entries)
                : Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        public void Dispose() { }
    }

    private sealed class CapturingLogger(ConcurrentQueue<LogEntry> entries) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        )
        {
            var fields = state is IEnumerable<KeyValuePair<string, object?>> values
                ? values.ToArray()
                : [];
            entries.Enqueue(new LogEntry(formatter(state, exception), fields));
        }
    }
}

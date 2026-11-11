using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WhatToCook.Api.Infrastructure.Storage;

/// <summary>
/// Verifies that the configured recipe image directory is actually writable.
/// Unlike process liveness, storage readiness may legitimately fail when the mount is unavailable.
/// </summary>
public sealed class RecipeImageStorageHealthCheck(
    IConfiguration configuration,
    IWebHostEnvironment environment
) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    ) =>
        ProbeAsync(
            RecipeImagePathResolver.Resolve(configuration, environment.ContentRootPath),
            cancellationToken
        );

    public static async Task<HealthCheckResult> ProbeAsync(
        string directory,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!Directory.Exists(directory))
        {
            return HealthCheckResult.Unhealthy("Recipe image storage directory is unavailable.");
        }

        // No permanent data or recipe references. CreateNew avoids touching existing files.
        var probePath = Path.Combine(directory, $".readiness-{Guid.NewGuid():N}");
        try
        {
            await using var stream = new FileStream(
                probePath,
                FileMode.CreateNew,
                FileAccess.ReadWrite,
                FileShare.None,
                bufferSize: 1,
                options: FileOptions.Asynchronous | FileOptions.DeleteOnClose
            );
            byte[] marker = [0x5A];
            await stream.WriteAsync(marker, cancellationToken);
            await stream.FlushAsync(cancellationToken);
            stream.Position = 0;
            byte[] readBack = new byte[1];
            var bytesRead = await stream.ReadAsync(readBack, cancellationToken);
            return bytesRead == 1 && readBack[0] == marker[0]
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Recipe image storage read-back failed.");
        }
        catch (IOException)
        {
            return HealthCheckResult.Unhealthy("Recipe image storage is unavailable.");
        }
        catch (UnauthorizedAccessException)
        {
            return HealthCheckResult.Unhealthy("Recipe image storage is not writable.");
        }
    }
}

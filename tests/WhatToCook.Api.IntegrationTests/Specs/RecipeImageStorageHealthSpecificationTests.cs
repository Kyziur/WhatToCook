using Microsoft.Extensions.Diagnostics.HealthChecks;
using WhatToCook.Api.Infrastructure.Storage;

namespace WhatToCook.Api.IntegrationTests.Specs;

public sealed class RecipeImageStorageHealthSpecificationTests
{
    [Fact]
    public async Task WritableDirectory_ShouldBeHealthyAndLeaveNoProbeFile()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"wtc-readiness-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var result = await RecipeImageStorageHealthCheck.ProbeAsync(directory);
            Assert.Equal(HealthStatus.Healthy, result.Status);
            Assert.Empty(Directory.EnumerateFileSystemEntries(directory));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task MissingDirectory_ShouldBeUnhealthyAndNotCreateIt()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"wtc-missing-{Guid.NewGuid():N}");
        var result = await RecipeImageStorageHealthCheck.ProbeAsync(directory);
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.False(Directory.Exists(directory));
    }

    [Fact]
    public async Task FileInsteadOfDirectory_ShouldBeUnhealthy()
    {
        var file = Path.GetTempFileName();
        try
        {
            var result = await RecipeImageStorageHealthCheck.ProbeAsync(file);
            Assert.Equal(HealthStatus.Unhealthy, result.Status);
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Fact]
    public async Task CancelledProbe_ShouldHonorCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            RecipeImageStorageHealthCheck.ProbeAsync(Path.GetTempPath(), cancellation.Token)
        );
    }
}

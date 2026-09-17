using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace WhatToCook.Api.IntegrationTests.Specs;

public sealed class SelfHostedWebPlatformSpecificationTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public async Task SaveStructuredRecipeData_ShouldPersistInRelationalStore()
    {
        using var client = factory.CreateClient();
        var payload = RecipeApiContracts.CreateRecipePayload("Relacyjna pomidorowa");
        payload.Ingredients =
        [
            new RecipeApiContracts.RecipeIngredientPayload("pomidor", "1000", "g"),
            new RecipeApiContracts.RecipeIngredientPayload("bulion", "1", "l"),
        ];
        payload.Steps = ["Przygotuj baze", "Dopraw i podawaj"];

        var createResponse = await client.PostAsJsonAsync("/api/recipes", payload);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created =
            await createResponse.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeDetailsDto>(
                JsonOptions
            );
        Assert.NotNull(created);
        Assert.Equal(2, created!.Ingredients.Count);
        Assert.Equal(2, created.Steps.Count);
    }

    [Fact]
    public async Task SavedRecipes_ShouldRemainAvailableAcrossNewClientSessions()
    {
        var creatorClient = factory.CreateClient();
        var payload = RecipeApiContracts.CreateRecipePayload("Sesyjna salatka");
        var createResponse = await creatorClient.PostAsJsonAsync("/api/recipes", payload);
        createResponse.EnsureSuccessStatusCode();
        creatorClient.Dispose();

        using var readerClient = factory.CreateClient();
        var libraryResponse = await readerClient.GetAsync("/api/recipes");
        libraryResponse.EnsureSuccessStatusCode();

        var recipes = await libraryResponse.Content.ReadFromJsonAsync<
            List<RecipeApiContracts.RecipeSummaryDto>
        >(JsonOptions);
        Assert.NotNull(recipes);
        Assert.Contains(recipes!, x => x.Title == "Sesyjna salatka");
    }

    [Fact]
    public async Task StartWithExternalConfiguration_ShouldUseSuppliedStoragePath()
    {
        using var client = factory.CreateClient();
        var payload = RecipeApiContracts.CreateRecipePayload("Konfiguracja zewnetrzna");
        var createResponse = await client.PostAsJsonAsync("/api/recipes", payload);
        createResponse.EnsureSuccessStatusCode();

        var created =
            await createResponse.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeDetailsDto>(
                JsonOptions
            );
        Assert.NotNull(created);

        using var multipart = new MultipartFormDataContent();
        multipart.Add(
            new ByteArrayContent(
                Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///ywAAAAAAQABAAACAUwAOw==")
            ),
            "file",
            "photo.gif"
        );
        var uploadResponse = await client.PostAsync($"/api/recipes/{created!.Id}/photo", multipart);
        uploadResponse.EnsureSuccessStatusCode();

        var uploadJson = await uploadResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var photoPath = uploadJson.GetProperty("mainPhotoPath").GetString();
        Assert.False(string.IsNullOrWhiteSpace(photoPath));
        Assert.True(File.Exists(Path.Combine(factory.StorageRootPath, photoPath!)));
    }

    [Fact]
    public async Task ContainerBasedDeployment_ShouldStartSuccessfully()
    {
        var repositoryRoot = ResolveRepositoryRoot();
        var deployDirectory = Path.Combine(repositoryRoot, "deploy");
        var composeFile = Path.Combine(deployDirectory, "docker-compose.yml");
        Assert.True(File.Exists(composeFile));

        var projectName = $"wtc{Guid.NewGuid():N}"[..15];
        var dataRoot = Path.Combine(Path.GetTempPath(), $"wtc-selfhost-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dataRoot);

        var publicHttpPort = GetFreeTcpPort();
        var composeOverrideFile = Path.Combine(
            Path.GetTempPath(),
            $"wtc-selfhost-override-{Guid.NewGuid():N}.yml"
        );
        var dockerPlatform = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.Arm64 => "linux/arm64/v8",
            _ => "linux/amd64",
        };

        var env = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["PUBLIC_HTTP_PORT"] = publicHttpPort.ToString(),
            ["DATA_ROOT"] = dataRoot,
            ["POSTGRES_PASSWORD"] = "wtc-self-host",
            ["SEED_DATA_ENABLED"] = "false",
            ["DOCKER_PLATFORM"] = dockerPlatform,
        };
        var createdNetworks = Array.Empty<string>();

        try
        {
            File.WriteAllText(
                composeOverrideFile,
                """
                services:
                  web:
                    ports:
                      - "${PUBLIC_HTTP_PORT}:8080"
                """
            );

            createdNetworks = await EnsureDockerNetworksExistAsync(
                ["edge", "observability", "egress"],
                deployDirectory
            );

            var upResult = await RunProcessAsync(
                "docker",
                $"compose -f \"{composeFile}\" -f \"{composeOverrideFile}\" --project-name {projectName} up -d --build --wait",
                deployDirectory,
                env,
                timeoutMs: 600_000
            );

            if (upResult.ExitCode != 0)
            {
                var postgresLogs = await RunProcessAsync(
                    "docker",
                    $"compose -f \"{composeFile}\" -f \"{composeOverrideFile}\" --project-name {projectName} logs --no-color postgres",
                    deployDirectory,
                    env,
                    timeoutMs: 60_000
                );
                Assert.Fail(
                    $"docker compose up failed.{Environment.NewLine}{upResult.Output}{Environment.NewLine}"
                        + $"PostgreSQL logs:{Environment.NewLine}{postgresLogs.Output}"
                );
            }

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await WaitForHealthyResponseAsync(
                client,
                new Uri($"http://127.0.0.1:{publicHttpPort}/"),
                TimeSpan.FromSeconds(90)
            );
            var html = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("WhatToCook", html);
        }
        finally
        {
            await RunProcessAsync(
                "docker",
                $"compose -f \"{composeFile}\" -f \"{composeOverrideFile}\" --project-name {projectName} down --remove-orphans --volumes",
                deployDirectory,
                env,
                timeoutMs: 180_000
            );
            await RemoveDockerNetworksAsync(createdNetworks, deployDirectory);
            TryDeleteFile(composeOverrideFile);
            TryDeleteDirectory(dataRoot);
        }
    }

    [Fact]
    public void DeploymentDocumentation_ShouldCoverSelfHostedOperationalFlow()
    {
        var repositoryRoot = ResolveRepositoryRoot();
        var documentationPath = Path.Combine(repositoryRoot, "docs", "self-hosted-deployment.md");

        Assert.True(File.Exists(documentationPath));
        var document = File.ReadAllText(documentationPath);

        Assert.Contains("local network", document, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Komodo", document, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Bitwarden", document, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("DATA_ROOT", document, StringComparison.Ordinal);
        Assert.Contains("POSTGRES_PASSWORD", document, StringComparison.Ordinal);
        Assert.Contains("Interactive Server", document, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AppHost_ShouldDefineDockerComposeEnvironmentForAspireDeployment()
    {
        var repositoryRoot = ResolveRepositoryRoot();
        var appHostPath = Path.Combine(repositoryRoot, "src", "WhatToCook.AppHost", "AppHost.cs");

        Assert.True(File.Exists(appHostPath));

        var appHost = File.ReadAllText(appHostPath);
        Assert.Contains("AddDockerComposeEnvironment", appHost, StringComparison.Ordinal);
        Assert.Contains("compose", appHost, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<HttpResponseMessage> WaitForHealthyResponseAsync(
        HttpClient client,
        Uri uri,
        TimeSpan timeout
    )
    {
        var deadline = DateTimeOffset.UtcNow.Add(timeout);
        Exception? lastError = null;

        while (DateTimeOffset.UtcNow < deadline)
        {
            try
            {
                var response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
            }
            catch (Exception exception)
            {
                lastError = exception;
            }

            await Task.Delay(1_000);
        }

        throw new TimeoutException(
            $"Timed out waiting for successful response from {uri}.",
            lastError
        );
    }

    private static async Task<(int ExitCode, string Output)> RunProcessAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        IReadOnlyDictionary<string, string?> env,
        int timeoutMs
    )
    {
        var startInfo = new ProcessStartInfo(fileName, arguments)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        foreach (var pair in env)
        {
            startInfo.Environment[pair.Key] = pair.Value;
        }

        using var process = Process.Start(startInfo);
        Assert.NotNull(process);

        using var cts = new CancellationTokenSource(timeoutMs);
        var outputTask = process!.StandardOutput.ReadToEndAsync(cts.Token);
        var errorTask = process.StandardError.ReadToEndAsync(cts.Token);
        await process.WaitForExitAsync(cts.Token);

        var output = await outputTask;
        var error = await errorTask;
        var mergedOutput = new StringBuilder().AppendLine(output).AppendLine(error).ToString();

        return (process.ExitCode, mergedOutput);
    }

    private static async Task<string[]> EnsureDockerNetworksExistAsync(
        IEnumerable<string> networkNames,
        string workingDirectory
    )
    {
        var createdNetworks = new List<string>();

        foreach (var networkName in networkNames)
        {
            var inspectResult = await RunProcessAsync(
                "docker",
                $"network inspect {networkName}",
                workingDirectory,
                env: new Dictionary<string, string?>(),
                timeoutMs: 30_000
            );

            if (inspectResult.ExitCode == 0)
            {
                continue;
            }

            var createResult = await RunProcessAsync(
                "docker",
                $"network create {networkName}",
                workingDirectory,
                env: new Dictionary<string, string?>(),
                timeoutMs: 30_000
            );

            Assert.True(
                createResult.ExitCode == 0,
                $"docker network create {networkName} failed.{Environment.NewLine}{createResult.Output}"
            );

            createdNetworks.Add(networkName);
        }

        return [.. createdNetworks];
    }

    private static async Task RemoveDockerNetworksAsync(
        IEnumerable<string> networkNames,
        string workingDirectory
    )
    {
        foreach (var networkName in networkNames)
        {
            await RunProcessAsync(
                "docker",
                $"network rm {networkName}",
                workingDirectory,
                env: new Dictionary<string, string?>(),
                timeoutMs: 30_000
            );
        }
    }

    private static int GetFreeTcpPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static string ResolveRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "WhatToCook.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException(
            "Could not resolve repository root from test base path."
        );
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
            // Ignore temp cleanup errors.
        }
        catch (UnauthorizedAccessException)
        {
            // Ignore temp cleanup errors.
        }
    }

    private static void TryDeleteFile(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
            // Ignore temp cleanup errors.
        }
        catch (UnauthorizedAccessException)
        {
            // Ignore temp cleanup errors.
        }
    }
}

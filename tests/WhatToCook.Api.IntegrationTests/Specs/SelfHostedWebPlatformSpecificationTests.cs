using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

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
        var imageRoot = Path.Combine(dataRoot, "recipe-images");
        Directory.CreateDirectory(imageRoot);
        // CI's temporary bind mount is owned by the test runner, not the container's
        // non-root UID 1654. Allow that UID to exercise readiness in this disposable
        // directory only. Production setup MUST remain 1654:1654 with mode 0750.
        if (OperatingSystem.IsLinux())
        {
            File.SetUnixFileMode(
                imageRoot,
                UnixFileMode.UserRead
                    | UnixFileMode.UserWrite
                    | UnixFileMode.UserExecute
                    | UnixFileMode.GroupRead
                    | UnixFileMode.GroupWrite
                    | UnixFileMode.GroupExecute
                    | UnixFileMode.OtherRead
                    | UnixFileMode.OtherWrite
                    | UnixFileMode.OtherExecute
            );
        }

        var publicHttpPort = GetFreeTcpPort();
        var apiHttpPort = GetFreeTcpPort();
        var composeOverrideFile = Path.Combine(
            Path.GetTempPath(),
            $"wtc-selfhost-override-{Guid.NewGuid():N}.yml"
        );
        var composeArguments =
            $"compose -f \"{composeFile}\" -f \"{composeOverrideFile}\" --project-name {projectName}";
        var dockerPlatform = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.Arm64 => "linux/arm64/v8",
            _ => "linux/amd64",
        };

        var env = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["PUBLIC_HTTP_PORT"] = publicHttpPort.ToString(),
            ["API_HTTP_PORT"] = apiHttpPort.ToString(),
            ["DATA_ROOT"] = dataRoot,
            ["POSTGRES_PASSWORD"] = "wtc-self-host",
            ["SEED_DATA_ENABLED"] = "false",
            ["DOCKER_PLATFORM"] = dockerPlatform,
        };
        var createdNetworks = Array.Empty<string>();

        try
        {
            WriteWebDataProtectionCertificate(dataRoot);
            File.WriteAllText(
                composeOverrideFile,
                """
                services:
                  web:
                    ports:
                      - "127.0.0.1:${PUBLIC_HTTP_PORT}:8080"
                  api:
                    ports:
                      - "127.0.0.1:${API_HTTP_PORT}:8080"
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

            // Temporary Compose stack only: verify PostgreSQL-backed recipe creation and
            // write/read through the API container image bind mount as non-root UID.
            using var apiClient = new HttpClient
            {
                BaseAddress = new Uri($"http://127.0.0.1:{apiHttpPort}/"),
                Timeout = TimeSpan.FromSeconds(20),
            };
            using var createdResponse = await apiClient.PostAsJsonAsync(
                "api/recipes",
                RecipeApiContracts.CreateRecipePayload($"Deployment smoke {Guid.NewGuid():N}")
            );
            Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
            var created =
                await createdResponse.Content.ReadFromJsonAsync<RecipeApiContracts.RecipeDetailsDto>(
                    JsonOptions
                );
            Assert.NotNull(created);

            using var detailsResponse = await apiClient.GetAsync($"api/recipes/{created!.Id}");
            detailsResponse.EnsureSuccessStatusCode();

            var imageBytes = Convert.FromBase64String(
                "R0lGODlhAQABAIAAAAAAAP///ywAAAAAAQABAAACAUwAOw=="
            );
            using var uploadContent = new MultipartFormDataContent();
            uploadContent.Add(new ByteArrayContent(imageBytes), "file", "smoke.gif");
            using var uploadResponse = await apiClient.PostAsync(
                $"api/recipes/{created.Id}/photo",
                uploadContent
            );
            uploadResponse.EnsureSuccessStatusCode();
            var photoResult = await uploadResponse.Content.ReadFromJsonAsync<JsonElement>(
                JsonOptions
            );
            var photoName = photoResult.GetProperty("mainPhotoPath").GetString();
            Assert.False(string.IsNullOrWhiteSpace(photoName));
            Assert.Equal(Path.GetFileName(photoName), photoName);
            Assert.True(File.Exists(Path.Combine(imageRoot, photoName!)));
            using var photoResponse = await apiClient.GetAsync($"recipe-images/{photoName}");
            photoResponse.EnsureSuccessStatusCode();
            Assert.Equal(imageBytes, await photoResponse.Content.ReadAsByteArrayAsync());

            // Antiforgery state minted before a web-only recreation must still decrypt in the
            // recreated container: the framework reuses the existing cookie instead of replacing it.
            // Tampered and cookie-less submissions remain rejected.
            var pageUri = new Uri($"http://127.0.0.1:{publicHttpPort}/recipes/new");
            var antiforgeryCookies = new CookieContainer();
            using var antiforgeryClient = CreateAntiforgeryClient(antiforgeryCookies);
            var issuedForm = await GetRecipeEditorFormAsync(antiforgeryClient, pageUri);
            Assert.True(
                HasAntiforgeryCookie(antiforgeryCookies, pageUri),
                "GET /recipes/new did not issue an antiforgery cookie."
            );
            var issuedAntiforgeryCookieValue = antiforgeryCookies
                .GetCookies(pageUri)
                .Single(cookie =>
                    cookie.Name.StartsWith(".AspNetCore.Antiforgery", StringComparison.Ordinal)
                )
                .Value;

            async Task<string> ContainerIdAsync(string service) =>
                await GetComposeServiceContainerIdAsync(
                    composeArguments,
                    service,
                    deployDirectory,
                    env
                );

            var keyFilesBeforeRecreate = await ListDataProtectionKeyFilesAsync(
                composeArguments,
                deployDirectory,
                env
            );
            Assert.NotEmpty(keyFilesBeforeRecreate);
            var webContainerBeforeRecreate = await ContainerIdAsync("web");
            var apiContainerBeforeRecreate = await ContainerIdAsync("api");
            var postgresContainerBeforeRecreate = await ContainerIdAsync("postgres");

            var recreateWeb = await RunProcessAsync(
                "docker",
                $"{composeArguments} up -d --force-recreate --no-deps --wait web",
                deployDirectory,
                env,
                timeoutMs: 300_000
            );
            Assert.True(
                recreateWeb.ExitCode == 0,
                $"docker compose force-recreate web failed.{Environment.NewLine}{recreateWeb.Output}"
            );
            var webContainerAfterRecreate = await ContainerIdAsync("web");
            Assert.NotEqual(webContainerBeforeRecreate, webContainerAfterRecreate);
            Assert.Equal(apiContainerBeforeRecreate, await ContainerIdAsync("api"));
            Assert.Equal(postgresContainerBeforeRecreate, await ContainerIdAsync("postgres"));

            // Runtime proof: the recreated container's key directory keeps the image-owned
            // app UID/GID and 0700 mode. Only metadata is read; no key material.
            var keyDirectoryStat = await RunProcessAsync(
                "docker",
                $"{composeArguments} exec -T web stat -c %u:%g:%a {WebKeyRingPath}",
                deployDirectory,
                env,
                timeoutMs: 60_000
            );
            Assert.True(
                keyDirectoryStat.ExitCode == 0,
                $"Inspecting web Data Protection directory failed.{Environment.NewLine}{keyDirectoryStat.Output}"
            );
            Assert.Equal("1654:1654:700", keyDirectoryStat.Output.Trim());

            // The certificate must be mounted read-only into the recreated web container.
            var webInspect = await RunProcessAsync(
                "docker",
                $"inspect --format \"{{{{json .Mounts}}}}\" {webContainerAfterRecreate}",
                deployDirectory,
                env,
                timeoutMs: 30_000
            );
            Assert.True(
                webInspect.ExitCode == 0,
                $"docker inspect web mounts failed.{Environment.NewLine}{webInspect.Output}"
            );
            using var mounts = JsonDocument.Parse(webInspect.Output);
            var certificateMount = mounts
                .RootElement.EnumerateArray()
                .Single(mount =>
                    mount.GetProperty("Destination").GetString() == WebCertificateTarget
                );
            Assert.False(certificateMount.GetProperty("RW").GetBoolean());

            // Reuse the pre-recreation CookieContainer. The framework decrypts the existing
            // antiforgery cookie and keeps it; a replacement Set-Cookie would mean the recreated
            // web could not decrypt it. Cookie values are compared as booleans and never printed.
            using var reissuedPageResponse = await antiforgeryClient.GetAsync(pageUri);
            Assert.Equal(HttpStatusCode.OK, reissuedPageResponse.StatusCode);
            Assert.False(
                reissuedPageResponse.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders)
                    && setCookieHeaders.Any(header =>
                        header.StartsWith(".AspNetCore.Antiforgery", StringComparison.Ordinal)
                    ),
                "Recreated web replaced the pre-recreation antiforgery cookie instead of decrypting it."
            );
            var reissuedForm = ParseRecipeEditorForm(
                await reissuedPageResponse.Content.ReadAsStringAsync()
            );
            Assert.False(string.IsNullOrEmpty(reissuedForm.RequestToken));
            Assert.True(
                string.Equals(
                    issuedAntiforgeryCookieValue,
                    antiforgeryCookies
                        .GetCookies(pageUri)
                        .Single(cookie =>
                            cookie.Name.StartsWith(
                                ".AspNetCore.Antiforgery",
                                StringComparison.Ordinal
                            )
                        )
                        .Value,
                    StringComparison.Ordinal
                ),
                "Antiforgery cookie value changed across web recreation."
            );

            var tamperedForm = WithRequestToken(issuedForm, TamperToken(issuedForm.RequestToken));
            var tamperedStatus = await PostRecipeEditorFormAsync(
                antiforgeryClient,
                pageUri,
                tamperedForm
            );
            Assert.Equal(HttpStatusCode.BadRequest, tamperedStatus);

            using var cookielessClient = CreateAntiforgeryClient(new CookieContainer());
            var cookielessStatus = await PostRecipeEditorFormAsync(
                cookielessClient,
                pageUri,
                issuedForm
            );
            Assert.Equal(HttpStatusCode.BadRequest, cookielessStatus);

            Assert.Equal(
                keyFilesBeforeRecreate,
                await ListDataProtectionKeyFilesAsync(composeArguments, deployDirectory, env)
            );
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
    public async Task DeploymentComposeConfig_ShouldEnforceServiceStorageAndNetworkContract()
    {
        var deployDirectory = Path.Combine(ResolveRepositoryRoot(), "deploy");
        var composeFile = Path.Combine(deployDirectory, "docker-compose.yml");
        var dataRoot = Path.Combine(Path.GetTempPath(), $"wtc-contract-{Guid.NewGuid():N}");
        var projectName = $"wtccontract{Guid.NewGuid():N}"[..20];
        var env = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["DATA_ROOT"] = dataRoot,
            ["POSTGRES_PASSWORD"] = "test-only-password",
            ["DOCKER_PLATFORM"] = "linux/amd64",
        };

        var rendered = await RunProcessAsync(
            "docker",
            $"compose -f \"{composeFile}\" --project-name {projectName} config --format json",
            deployDirectory,
            env,
            timeoutMs: 30_000
        );
        Assert.Equal(0, rendered.ExitCode);

        using var document = JsonDocument.Parse(rendered.Output);
        var root = document.RootElement;
        Assert.Equal(projectName, root.GetProperty("name").GetString());
        var services = root.GetProperty("services");
        var api = services.GetProperty("api");
        var web = services.GetProperty("web");
        var postgres = services.GetProperty("postgres");

        var apiNetworks = api.GetProperty("networks");
        var webNetworks = web.GetProperty("networks");
        Assert.False(apiNetworks.TryGetProperty("edge", out _));
        Assert.True(webNetworks.TryGetProperty("edge", out _));
        Assert.False(postgres.GetProperty("networks").TryGetProperty("edge", out _));

        Assert.False(api.TryGetProperty("ports", out _));
        Assert.False(postgres.TryGetProperty("ports", out _));
        Assert.Equal(
            "http://api:8080",
            web.GetProperty("environment").GetProperty("Api__BaseUrl").GetString()
        );
        Assert.Equal(
            "/data/recipe-images",
            api.GetProperty("environment").GetProperty("Storage__RecipeImagesPath").GetString()
        );
        var recipeImagesVolume = api.GetProperty("volumes")
            .EnumerateArray()
            .Single(volume => volume.GetProperty("target").GetString() == "/data/recipe-images");
        var expectedImageRoot = Path.GetFullPath(Path.Combine(dataRoot, "recipe-images"));
        var actualImageRoot = Path.GetFullPath(
            recipeImagesVolume.GetProperty("source").GetString()!
        );
        Assert.Equal(
            expectedImageRoot,
            actualImageRoot,
            OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal
        );
        Assert.Equal("bind", recipeImagesVolume.GetProperty("type").GetString());
        static bool? GetCreateHostPath(JsonElement volume)
        {
            var bind = volume.GetProperty("bind");
            if (!bind.TryGetProperty("create_host_path", out var value))
            {
                return null;
            }

            Assert.True(
                value.ValueKind is JsonValueKind.True or JsonValueKind.False,
                $"bind.create_host_path must render as a bool when present, not {value.ValueKind}."
            );
            return value.GetBoolean();
        }

        var bindProbeFile = Path.Combine(
            Path.GetTempPath(),
            $"wtc-bind-contract-{Guid.NewGuid():N}.yml"
        );
        try
        {
            File.WriteAllText(
                bindProbeFile,
                """
                services:
                  probe:
                    image: scratch
                    volumes:
                      - type: bind
                        source: ./explicit-false
                        target: /explicit-false
                        bind:
                          create_host_path: false
                      - ./implicit:/implicit
                      - type: bind
                        source: ./explicit-true
                        target: /explicit-true
                        bind:
                          create_host_path: true
                """
            );
            var bindProbe = await RunProcessAsync(
                "docker",
                $"compose -f \"{bindProbeFile}\" --project-name wtc-bind-contract config --format json",
                Path.GetDirectoryName(bindProbeFile)!,
                new Dictionary<string, string?>(),
                timeoutMs: 30_000
            );
            Assert.Equal(0, bindProbe.ExitCode);
            using var bindProbeDocument = JsonDocument.Parse(bindProbe.Output);
            var probeVolumes = bindProbeDocument
                .RootElement.GetProperty("services")
                .GetProperty("probe")
                .GetProperty("volumes");
            var explicitFalseVolume = probeVolumes
                .EnumerateArray()
                .Single(volume => volume.GetProperty("target").GetString() == "/explicit-false");
            var implicitVolume = probeVolumes
                .EnumerateArray()
                .Single(volume => volume.GetProperty("target").GetString() == "/implicit");
            var explicitTrueVolume = probeVolumes
                .EnumerateArray()
                .Single(volume => volume.GetProperty("target").GetString() == "/explicit-true");
            var explicitFalse = GetCreateHostPath(explicitFalseVolume);
            Assert.NotEqual(explicitFalse, GetCreateHostPath(implicitVolume));
            Assert.NotEqual(explicitFalse, GetCreateHostPath(explicitTrueVolume));
            Assert.Equal(explicitFalse, GetCreateHostPath(recipeImagesVolume));
        }
        finally
        {
            TryDeleteFile(bindProbeFile);
        }
        var postgresDataVolume = postgres
            .GetProperty("volumes")
            .EnumerateArray()
            .Single(x => x.GetProperty("target").GetString() == "/var/lib/postgresql/data");
        Assert.Equal("volume", postgresDataVolume.GetProperty("type").GetString());

        var webEnvironment = web.GetProperty("environment");
        Assert.Equal(
            WebKeyRingPath,
            webEnvironment.GetProperty("DataProtection__KeyRingPath").GetString()
        );
        Assert.Equal(
            WebCertificateTarget,
            webEnvironment.GetProperty("DataProtection__CertificatePath").GetString()
        );
        Assert.Equal(
            new[] { "DataProtection__CertificatePath", "DataProtection__KeyRingPath" },
            webEnvironment
                .EnumerateObject()
                .Select(property => property.Name)
                .Where(name => name.StartsWith("DataProtection__", StringComparison.Ordinal))
                .Order(StringComparer.Ordinal)
                .ToArray()
        );

        var webKeyRingVolume = Assert.Single(web.GetProperty("volumes").EnumerateArray().ToArray());
        Assert.Equal(WebKeyRingPath, webKeyRingVolume.GetProperty("target").GetString());
        Assert.Equal("volume", webKeyRingVolume.GetProperty("type").GetString());
        Assert.Equal(WebKeyRingVolume, webKeyRingVolume.GetProperty("source").GetString());
        Assert.True(root.GetProperty("volumes").TryGetProperty(WebKeyRingVolume, out _));

        var webSecret = Assert.Single(web.GetProperty("secrets").EnumerateArray().ToArray());
        Assert.Equal(WebCertificateSecret, webSecret.GetProperty("source").GetString());
        Assert.Equal(
            WebCertificateTarget,
            ResolveSecretTarget(webSecret.GetProperty("target").GetString()!)
        );
        var certificateSecret = root.GetProperty("secrets").GetProperty(WebCertificateSecret);
        Assert.Equal(
            Path.GetFullPath(Path.Combine(dataRoot, "secrets", WebCertificateFileName)),
            Path.GetFullPath(certificateSecret.GetProperty("file").GetString()!),
            OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal
        );

        Assert.False(
            MountsWebDataProtection(api),
            "api must not mount web Data Protection key ring or certificate."
        );
        Assert.False(
            MountsWebDataProtection(postgres),
            "postgres must not mount web Data Protection key ring or certificate."
        );

        var missingMount = new Dictionary<string, string?>(env, StringComparer.Ordinal)
        {
            ["DATA_ROOT"] = "",
        };
        var invalid = await RunProcessAsync(
            "docker",
            $"compose -f \"{composeFile}\" --project-name {projectName} config --quiet",
            deployDirectory,
            missingMount,
            timeoutMs: 30_000
        );
        Assert.NotEqual(0, invalid.ExitCode);
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

    private const string WebKeyRingPath = "/var/lib/whattocook/data-protection";
    private const string WebKeyRingVolume = "web-data-protection";
    private const string WebCertificateSecret = "web-data-protection-certificate";
    private const string WebCertificateTarget = "/run/secrets/web-data-protection.pem";
    private const string WebCertificateFileName = "web-data-protection.pem";
    private const string AntiforgeryFieldName = "__RequestVerificationToken";
    private const string RecipeEditorFormName = "recipe-editor";
    private static readonly Regex FormElementPattern = new(
        @"<form\b[^>]*>.*?</form>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline
    );
    private static readonly Regex InputElementPattern = new(
        @"<input\b[^>]*>",
        RegexOptions.IgnoreCase
    );
    private static readonly Regex AttributePattern = new(
        "\\s(name|type|value)=\"([^\"]*)\"",
        RegexOptions.IgnoreCase
    );

    private sealed class RecipeEditorForm(IReadOnlyList<KeyValuePair<string, string>> fields)
    {
        public IReadOnlyList<KeyValuePair<string, string>> Fields { get; } = fields;

        public string RequestToken =>
            Fields.Single(input => input.Key == AntiforgeryFieldName).Value;
    }

    private static void WriteWebDataProtectionCertificate(string dataRoot)
    {
        var secretsDirectory = Path.Combine(dataRoot, "secrets");
        Directory.CreateDirectory(secretsDirectory);
        var certificatePath = Path.Combine(secretsDirectory, WebCertificateFileName);

        // Combined PEM (certificate + unencrypted PKCS#8 RSA key) shaped like the production
        // file secret. It lives only in this disposable directory and is never logged.
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=WhatToCook.Web integration test",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );
        using var certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(30)
        );
        File.WriteAllText(
            certificatePath,
            certificate.ExportCertificatePem() + "\n" + rsa.ExportPkcs8PrivateKeyPem()
        );

        if (OperatingSystem.IsLinux())
        {
            // The runner cannot chown this disposable fixture to UID 1654, so grant read bits
            // only. Production provisioning MUST use owner 1654 and mode 0400.
            File.SetUnixFileMode(
                certificatePath,
                UnixFileMode.UserRead | UnixFileMode.GroupRead | UnixFileMode.OtherRead
            );
        }
    }

    private static async Task<string> GetComposeServiceContainerIdAsync(
        string composeArguments,
        string service,
        string workingDirectory,
        IReadOnlyDictionary<string, string?> env
    )
    {
        var result = await RunProcessAsync(
            "docker",
            $"{composeArguments} ps -q {service}",
            workingDirectory,
            env,
            timeoutMs: 30_000
        );
        Assert.True(
            result.ExitCode == 0,
            $"docker compose ps -q {service} failed.{Environment.NewLine}{result.Output}"
        );
        var containerId = result.Output.Trim();
        Assert.Matches("^[0-9a-f]{64}$", containerId);
        return containerId;
    }

    private static async Task<string[]> ListDataProtectionKeyFilesAsync(
        string composeArguments,
        string workingDirectory,
        IReadOnlyDictionary<string, string?> env
    )
    {
        // Names only: key XML content is never read or printed.
        var listing = await RunProcessAsync(
            "docker",
            $"{composeArguments} exec -T web ls -1 {WebKeyRingPath}",
            workingDirectory,
            env,
            timeoutMs: 60_000
        );
        Assert.True(
            listing.ExitCode == 0,
            $"Listing web Data Protection key ring failed.{Environment.NewLine}{listing.Output}"
        );
        return listing
            .Output.Split(
                '\n',
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
            )
            .Where(name =>
                name.StartsWith("key-", StringComparison.Ordinal)
                && name.EndsWith(".xml", StringComparison.Ordinal)
            )
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static HttpClient CreateAntiforgeryClient(CookieContainer cookies) =>
        new(
            new HttpClientHandler
            {
                CookieContainer = cookies,
                UseCookies = true,
                AllowAutoRedirect = false,
            },
            disposeHandler: true
        )
        {
            Timeout = TimeSpan.FromSeconds(20),
        };

    private static async Task<RecipeEditorForm> GetRecipeEditorFormAsync(
        HttpClient client,
        Uri pageUri
    )
    {
        using var response = await client.GetAsync(pageUri);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return ParseRecipeEditorForm(await response.Content.ReadAsStringAsync());
    }

    private static RecipeEditorForm ParseRecipeEditorForm(string html)
    {
        var formHtml = FormElementPattern
            .Matches(html)
            .Select(match => match.Value)
            .FirstOrDefault(form =>
                form.Contains(RecipeEditorFormName, StringComparison.Ordinal)
                && form.Contains(AntiforgeryFieldName, StringComparison.Ordinal)
            );
        Assert.True(
            formHtml is not null,
            "GET /recipes/new did not render a recipe-editor form with an antiforgery token."
        );

        var fields = new List<KeyValuePair<string, string>>();
        foreach (Match input in InputElementPattern.Matches(formHtml!))
        {
            var attributes = AttributePattern
                .Matches(input.Value)
                .ToDictionary(
                    match => match.Groups[1].Value,
                    match => WebUtility.HtmlDecode(match.Groups[2].Value),
                    StringComparer.OrdinalIgnoreCase
                );
            if (
                attributes.TryGetValue("type", out var type)
                && string.Equals(type, "hidden", StringComparison.OrdinalIgnoreCase)
                && attributes.TryGetValue("name", out var name)
            )
            {
                fields.Add(
                    new KeyValuePair<string, string>(
                        name,
                        attributes.GetValueOrDefault("value") ?? string.Empty
                    )
                );
            }
        }

        Assert.True(
            fields.Count(field => field.Key == AntiforgeryFieldName) == 1,
            "recipe-editor form must contain exactly one antiforgery request token."
        );
        return new RecipeEditorForm(fields);
    }

    private static bool HasAntiforgeryCookie(CookieContainer cookies, Uri uri) =>
        cookies
            .GetCookies(uri)
            .Any(cookie =>
                cookie.Name.StartsWith(".AspNetCore.Antiforgery", StringComparison.Ordinal)
            );

    private static async Task<HttpStatusCode> PostRecipeEditorFormAsync(
        HttpClient client,
        Uri pageUri,
        RecipeEditorForm form
    )
    {
        using var content = new FormUrlEncodedContent(form.Fields);
        using var response = await client.PostAsync(pageUri, content);
        return response.StatusCode;
    }

    private static RecipeEditorForm WithRequestToken(RecipeEditorForm form, string requestToken) =>
        new(
            form.Fields.Select(field =>
                    field.Key == AntiforgeryFieldName
                        ? new KeyValuePair<string, string>(field.Key, requestToken)
                        : field
                )
                .ToList()
        );

    private static string TamperToken(string token) => token[..^1] + (token[^1] == 'A' ? 'B' : 'A');

    private static bool MountsWebDataProtection(JsonElement service)
    {
        var mountsKeyRing =
            service.TryGetProperty("volumes", out var volumes)
            && volumes.EnumerateArray().Any(IsWebDataProtectionVolume);
        var includesCertificateSecret =
            service.TryGetProperty("secrets", out var secrets)
            && secrets
                .EnumerateArray()
                .Any(secret => secret.GetProperty("source").GetString() == WebCertificateSecret);
        return mountsKeyRing || includesCertificateSecret;
    }

    private static bool IsWebDataProtectionVolume(JsonElement volume) =>
        volume.GetProperty("target").GetString() == WebKeyRingPath
        || volume.TryGetProperty("source", out var source)
            && source.GetString() == WebKeyRingVolume;

    private static string ResolveSecretTarget(string target) =>
        target.StartsWith('/') ? target : $"/run/secrets/{target}";
}

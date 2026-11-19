using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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
        var deployDirectory = Path.Combine(ResolveRepositoryRoot(), "deploy");
        var composeFile = Path.Combine(deployDirectory, "docker-compose.yml");
        Assert.True(File.Exists(composeFile));

        var dataRoot = Path.Combine(Path.GetTempPath(), $"wtc-selfhost-{Guid.NewGuid():N}");
        var imageRoot = Path.Combine(dataRoot, "recipe-images");
        Directory.CreateDirectory(imageRoot);
        MakeDisposableImageRootWritable(imageRoot);

        var publicHttpPort = GetFreeTcpPort();
        var apiHttpPort = GetFreeTcpPort();
        var project = new ComposeProject(
            ProjectName: $"wtc{Guid.NewGuid():N}"[..15],
            DeployDirectory: deployDirectory,
            ComposeFile: composeFile,
            OverrideFile: Path.Combine(
                Path.GetTempPath(),
                $"wtc-selfhost-override-{Guid.NewGuid():N}.yml"
            ),
            Env: new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["PUBLIC_HTTP_PORT"] = publicHttpPort.ToString(),
                ["API_HTTP_PORT"] = apiHttpPort.ToString(),
                ["DATA_ROOT"] = dataRoot,
                ["POSTGRES_PASSWORD"] = "wtc-self-host",
                ["SEED_DATA_ENABLED"] = "false",
                ["DOCKER_PLATFORM"] = ResolveDockerPlatform(),
            }
        );
        var createdNetworks = Array.Empty<string>();

        try
        {
            // No certificate is supplied: web-data-protection-init must create it.
            File.WriteAllText(project.OverrideFile, PortOverrideYaml);
            createdNetworks = await EnsureDockerNetworksExistAsync(
                ["edge", "observability", "egress"],
                deployDirectory
            );

            var upResult = await RunComposeAsync(project, "up -d --build --wait", 600_000);
            if (upResult.ExitCode != 0)
            {
                var postgresLogs = await RunComposeAsync(
                    project,
                    "logs --no-color postgres",
                    60_000
                );
                Assert.Fail(
                    $"docker compose up failed.{Environment.NewLine}{upResult.Output}{Environment.NewLine}"
                        + $"PostgreSQL logs:{Environment.NewLine}{postgresLogs.Output}"
                );
            }

            await AssertPublicHomePageServesAsync(publicHttpPort);
            await AssertApiPersistsRecipeAndPhotoAsync(apiHttpPort, imageRoot);
            await AssertFreshBootstrapAsync(project);

            // Antiforgery state minted before the web-only recreation must still decrypt after it.
            using var antiforgery = await AntiforgeryProbe.IssueAsync(
                new Uri($"http://127.0.0.1:{publicHttpPort}/recipes/new")
            );

            var keyFiles = await ListDataProtectionKeyFilesAsync(project);
            Assert.NotEmpty(keyFiles);
            var apiContainer = await GetServiceContainerIdAsync(project, "api");
            var postgresContainer = await GetServiceContainerIdAsync(project, "postgres");

            await RerunCertificateInitializerAsync(project);

            var webContainerBeforeRecreate = await GetServiceContainerIdAsync(project, "web");
            var recreateWeb = await RunComposeAsync(
                project,
                "up -d --force-recreate --no-deps --wait web",
                300_000
            );
            Assert.True(
                recreateWeb.ExitCode == 0,
                $"docker compose force-recreate web failed.{Environment.NewLine}{recreateWeb.Output}"
            );
            Assert.NotEqual(
                webContainerBeforeRecreate,
                await GetServiceContainerIdAsync(project, "web")
            );
            Assert.Equal(apiContainer, await GetServiceContainerIdAsync(project, "api"));
            Assert.Equal(postgresContainer, await GetServiceContainerIdAsync(project, "postgres"));

            await AssertCertificateMountIsReadOnlyInWebAsync(project);
            await antiforgery.AssertCookieSurvivesRecreateAsync();
            Assert.Equal(keyFiles, await ListDataProtectionKeyFilesAsync(project));

            await AssertWebRefusesWrongCertificateAsync(project, keyFiles);

            // Last step: this stops the stack (volumes kept), so it runs after all live checks.
            await AssertWebRefusesStartWithoutCertificateAsync(project);
        }
        finally
        {
            await RunComposeAsync(project, "down --remove-orphans --volumes", 180_000);
            await RemoveDockerNetworksAsync(createdNetworks, deployDirectory);
            TryDeleteFile(project.OverrideFile);
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
        var services = root.GetProperty("services");
        var api = services.GetProperty("api");
        var web = services.GetProperty("web");
        var postgres = services.GetProperty("postgres");
        var bootstrap = services.GetProperty(InitServiceName);

        Assert.False(api.GetProperty("networks").TryGetProperty("edge", out _));
        Assert.True(web.GetProperty("networks").TryGetProperty("edge", out _));
        Assert.False(postgres.GetProperty("networks").TryGetProperty("edge", out _));
        Assert.False(api.TryGetProperty("ports", out _));
        Assert.False(postgres.TryGetProperty("ports", out _));

        var recipeImagesVolume = GetVolumeByTarget(api, "/data/recipe-images");
        Assert.Equal(
            Path.GetFullPath(Path.Combine(dataRoot, "recipe-images")),
            Path.GetFullPath(recipeImagesVolume.GetProperty("source").GetString()!),
            OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal
        );
        Assert.Equal("bind", recipeImagesVolume.GetProperty("type").GetString());
        await AssertRecipeImagesBindCreateHostPathContractAsync(recipeImagesVolume);

        AssertWebDataProtectionWiring(root, web);
        AssertBootstrapJobIsIsolated(bootstrap);

        Assert.False(
            MountsDataProtectionVolumes(api),
            "api must not mount the web key ring or certificate volumes."
        );
        Assert.False(
            MountsDataProtectionVolumes(postgres),
            "postgres must not mount the web key ring or certificate volumes."
        );

        var missingDataRoot = new Dictionary<string, string?>(env, StringComparer.Ordinal)
        {
            ["DATA_ROOT"] = "",
        };
        var invalid = await RunProcessAsync(
            "docker",
            $"compose -f \"{composeFile}\" --project-name {projectName} config --quiet",
            deployDirectory,
            missingDataRoot,
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

    private static async Task AssertPublicHomePageServesAsync(int publicHttpPort)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        using var response = await WaitForHealthyResponseAsync(
            client,
            new Uri($"http://127.0.0.1:{publicHttpPort}/"),
            TimeSpan.FromSeconds(90)
        );
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("WhatToCook", html);
    }

    // Temporary Compose stack only: verifies PostgreSQL-backed recipe creation and a photo
    // write/read through the API container's bind mount as the non-root UID.
    private static async Task AssertApiPersistsRecipeAndPhotoAsync(
        int apiHttpPort,
        string imageRoot
    )
    {
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
        var photoResult = await uploadResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var photoName = photoResult.GetProperty("mainPhotoPath").GetString();
        Assert.False(string.IsNullOrWhiteSpace(photoName));
        Assert.Equal(Path.GetFileName(photoName), photoName);
        Assert.True(File.Exists(Path.Combine(imageRoot, photoName!)));

        using var photoResponse = await apiClient.GetAsync($"recipe-images/{photoName}");
        photoResponse.EnsureSuccessStatusCode();
        Assert.Equal(imageBytes, await photoResponse.Content.ReadAsByteArrayAsync());
    }

    // Proves the certificate came from the real init job (no test-supplied PEM), that the init
    // container exited cleanly, and that the Docker-created directories and certificate have the
    // pinned owner and modes. Network, user, read-only root and mount access are checked in the
    // rendered config test, so they are not repeated here.
    private static async Task AssertFreshBootstrapAsync(ComposeProject project)
    {
        var init = await InspectContainerAsync(
            project,
            await GetServiceContainerIdAsync(project, InitServiceName, includeStopped: true)
        );
        Assert.Equal(0, init.GetProperty("State").GetProperty("ExitCode").GetInt32());

        Assert.Equal(
            "1654:1654:700",
            await StatInWebAsync(project, WebKeyRingPath, OwnerModeFormat)
        );
        Assert.Equal(
            "1654:1654:700",
            await StatInWebAsync(project, WebCertificatePath, OwnerModeFormat)
        );
        Assert.Equal(
            "1654:1654:400",
            await StatInWebAsync(project, WebCertificateFile, OwnerModeFormat)
        );
    }

    private static async Task RerunCertificateInitializerAsync(ComposeProject project)
    {
        var rerun = await RunComposeAsync(
            project,
            $"run --rm --no-deps -T {InitServiceName}",
            300_000
        );
        Assert.True(
            rerun.ExitCode == 0,
            $"Rerunning {InitServiceName} failed.{Environment.NewLine}{rerun.Output}"
        );
    }

    private static async Task AssertWebRefusesWrongCertificateAsync(
        ComposeProject project,
        string[] keyFiles
    )
    {
        var stop = await RunComposeAsync(project, "stop web", 60_000);
        Assert.Equal(0, stop.ExitCode);

        // Generate an unrelated but usable PEM within the disposable, networkless initializer.
        var replace = await RunComposeAsync(
            project,
            $"run --rm --no-deps -T --entrypoint /bin/sh {InitServiceName} -ec "
                + $"\"umask 077; openssl req -x509 -newkey rsa:3072 -noenc -days 1 "
                + "-subj /CN=WrongCertificate -keyout /tmp/wrong.key -out /tmp/wrong.crt 2>/dev/null; "
                + $"cat /tmp/wrong.crt /tmp/wrong.key > {WebCertificateFile}.new; "
                + $"chmod 0400 {WebCertificateFile}.new; mv -f {WebCertificateFile}.new {WebCertificateFile}\"",
            60_000
        );
        Assert.Equal(0, replace.ExitCode);
        await RerunCertificateInitializerAsync(project);

        // One-off Web uses the same image, entrypoint and volumes without the restart policy.
        var web = await RunComposeAsync(project, "run --rm --no-deps -T web", 60_000);
        Assert.NotEqual(0, web.ExitCode);
        Assert.True(
            web.Output.Contains(DecryptFailureMessage, StringComparison.Ordinal),
            "web must reject the mismatched certificate before listening."
        );
        Assert.False(web.Output.Contains("PRIVATE KEY", StringComparison.Ordinal));
        Assert.Equal(
            keyFiles,
            await ListKeyRingFilesAsync(
                project,
                $"run --rm --no-deps -T --entrypoint /bin/sh {InitServiceName} -c \"ls -1 {WebKeyRingPath}\""
            )
        );
    }

    // Behavioral check of the read-only certificate mount: a write from web must be refused.
    private static async Task AssertCertificateMountIsReadOnlyInWebAsync(ComposeProject project)
    {
        var write = await RunComposeAsync(
            project,
            $"exec -T web sh -c \"touch {WebCertificatePath}/write-probe\"",
            60_000
        );
        Assert.NotEqual(0, write.ExitCode);
    }

    // Stops web and its dependencies (volumes kept), removes only the certificate volume, then
    // asks Compose to start web. The populated key ring makes init fail with no certificate, so
    // the depends_on gate must stop web from starting. Init output is intentionally not printed:
    // it must not be needed to diagnose key material problems.
    private static async Task AssertWebRefusesStartWithoutCertificateAsync(ComposeProject project)
    {
        var down = await RunComposeAsync(project, "down --remove-orphans", 180_000);
        Assert.True(
            down.ExitCode == 0,
            $"docker compose down failed.{Environment.NewLine}{down.Output}"
        );

        var removeCertificate = await RunProcessAsync(
            "docker",
            $"volume rm {project.ProjectName}_{WebCertificateVolume}",
            project.DeployDirectory,
            project.Env,
            timeoutMs: 60_000
        );
        Assert.True(
            removeCertificate.ExitCode == 0,
            "Removing the certificate volume for the missing-certificate check failed."
        );

        var startWeb = await RunComposeAsync(project, "up -d web", 300_000);
        Assert.NotEqual(0, startWeb.ExitCode);

        var runningWeb = await RunComposeAsync(project, "ps -q --status running web", 30_000);
        Assert.Equal(0, runningWeb.ExitCode);
        Assert.True(
            string.IsNullOrWhiteSpace(runningWeb.Output),
            "web must not start when the bootstrap job refuses to run."
        );

        var init = await InspectContainerAsync(
            project,
            await GetServiceContainerIdAsync(project, InitServiceName, includeStopped: true)
        );
        Assert.NotEqual(0, init.GetProperty("State").GetProperty("ExitCode").GetInt32());
    }

    private static void AssertWebDataProtectionWiring(JsonElement root, JsonElement web)
    {
        Assert.Equal(
            WebCertificateFile,
            web.GetProperty("environment")
                .GetProperty("DataProtection__CertificatePath")
                .GetString()
        );
        Assert.False(web.TryGetProperty("secrets", out _));
        Assert.False(root.TryGetProperty("secrets", out _));
        Assert.Equal(
            "service_completed_successfully",
            web.GetProperty("depends_on")
                .GetProperty(InitServiceName)
                .GetProperty("condition")
                .GetString()
        );

        Assert.Equal(
            WebKeyRingVolume,
            GetVolumeByTarget(web, WebKeyRingPath).GetProperty("source").GetString()
        );
        Assert.False(
            IsReadOnly(GetVolumeByTarget(web, WebKeyRingPath)),
            "web must write its key ring."
        );

        Assert.Equal(
            WebCertificateVolume,
            GetVolumeByTarget(web, WebCertificatePath).GetProperty("source").GetString()
        );
        Assert.True(
            IsReadOnly(GetVolumeByTarget(web, WebCertificatePath)),
            "web must mount the certificate read-only."
        );

        var declaredVolumes = root.GetProperty("volumes");
        foreach (var volumeName in new[] { WebKeyRingVolume, WebCertificateVolume })
        {
            Assert.False(
                declaredVolumes.GetProperty(volumeName).TryGetProperty("external", out _),
                $"{volumeName} must be a project-scoped named volume."
            );
        }
    }

    private static void AssertBootstrapJobIsIsolated(JsonElement bootstrap)
    {
        Assert.Equal("none", bootstrap.GetProperty("network_mode").GetString());
        Assert.False(bootstrap.TryGetProperty("networks", out _));
        Assert.False(bootstrap.TryGetProperty("ports", out _));
        Assert.False(bootstrap.TryGetProperty("secrets", out _));
        Assert.Equal("1654:1654", bootstrap.GetProperty("user").GetString());
        Assert.True(bootstrap.GetProperty("read_only").GetBoolean());
        Assert.False(
            bootstrap.TryGetProperty("privileged", out var privileged) && privileged.GetBoolean()
        );
        Assert.Contains(
            "ALL",
            bootstrap.GetProperty("cap_drop").EnumerateArray().Select(x => x.GetString())
        );
        Assert.Contains(
            "no-new-privileges:true",
            bootstrap.GetProperty("security_opt").EnumerateArray().Select(x => x.GetString())
        );

        Assert.Equal(
            WebKeyRingVolume,
            GetVolumeByTarget(bootstrap, WebKeyRingPath).GetProperty("source").GetString()
        );
        Assert.True(
            IsReadOnly(GetVolumeByTarget(bootstrap, WebKeyRingPath)),
            "init must read the key ring read-only."
        );

        Assert.Equal(
            WebCertificateVolume,
            GetVolumeByTarget(bootstrap, WebCertificatePath).GetProperty("source").GetString()
        );
        Assert.False(
            IsReadOnly(GetVolumeByTarget(bootstrap, WebCertificatePath)),
            "init must write the certificate volume."
        );
    }

    private static async Task AssertRecipeImagesBindCreateHostPathContractAsync(
        JsonElement recipeImagesVolume
    )
    {
        var probeFile = Path.Combine(
            Path.GetTempPath(),
            $"wtc-bind-contract-{Guid.NewGuid():N}.yml"
        );
        try
        {
            File.WriteAllText(
                probeFile,
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
                $"compose -f \"{probeFile}\" --project-name wtc-bind-contract config --format json",
                Path.GetDirectoryName(probeFile)!,
                new Dictionary<string, string?>(),
                timeoutMs: 30_000
            );
            Assert.Equal(0, bindProbe.ExitCode);

            using var bindProbeDocument = JsonDocument.Parse(bindProbe.Output);
            var probeVolumes = bindProbeDocument
                .RootElement.GetProperty("services")
                .GetProperty("probe")
                .GetProperty("volumes");
            var explicitFalse = GetCreateHostPath(
                GetVolumeEntryByTarget(probeVolumes, "/explicit-false")
            );
            Assert.NotEqual(
                explicitFalse,
                GetCreateHostPath(GetVolumeEntryByTarget(probeVolumes, "/implicit"))
            );
            Assert.NotEqual(
                explicitFalse,
                GetCreateHostPath(GetVolumeEntryByTarget(probeVolumes, "/explicit-true"))
            );
            Assert.Equal(explicitFalse, GetCreateHostPath(recipeImagesVolume));
        }
        finally
        {
            TryDeleteFile(probeFile);
        }
    }

    private static bool? GetCreateHostPath(JsonElement volume)
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

    private static JsonElement GetVolumeByTarget(JsonElement service, string target) =>
        GetVolumeEntryByTarget(service.GetProperty("volumes"), target);

    private static JsonElement GetVolumeEntryByTarget(JsonElement volumes, string target) =>
        volumes
            .EnumerateArray()
            .Single(volume => volume.GetProperty("target").GetString() == target);

    private static bool IsReadOnly(JsonElement volume) =>
        volume.TryGetProperty("read_only", out var readOnly) && readOnly.GetBoolean();

    private static bool MountsDataProtectionVolumes(JsonElement service) =>
        service.TryGetProperty("volumes", out var volumes)
        && volumes
            .EnumerateArray()
            .Any(volume =>
                volume.GetProperty("target").GetString() is WebKeyRingPath or WebCertificatePath
                || volume.TryGetProperty("source", out var source)
                    && source.GetString() is WebKeyRingVolume or WebCertificateVolume
            );

    private static async Task<JsonElement> InspectContainerAsync(
        ComposeProject project,
        string containerId
    )
    {
        var result = await RunProcessAsync(
            "docker",
            $"inspect {containerId}",
            project.DeployDirectory,
            project.Env,
            timeoutMs: 30_000
        );
        Assert.True(
            result.ExitCode == 0,
            $"docker inspect failed.{Environment.NewLine}{result.Output}"
        );

        using var document = JsonDocument.Parse(result.Output);
        return document.RootElement[0].Clone();
    }

    private static async Task<string> GetServiceContainerIdAsync(
        ComposeProject project,
        string service,
        bool includeStopped = false
    )
    {
        var result = await RunComposeAsync(
            project,
            $"ps {(includeStopped ? "-a " : string.Empty)}-q {service}",
            30_000
        );
        Assert.True(
            result.ExitCode == 0,
            $"docker compose ps -q {service} failed.{Environment.NewLine}{result.Output}"
        );
        var containerId = result.Output.Trim();
        Assert.Matches("^[0-9a-f]{64}$", containerId);
        return containerId;
    }

    // Metadata only (owner, mode, inode, mtime); key or certificate contents are never read.
    private static async Task<string> StatInWebAsync(
        ComposeProject project,
        string path,
        string format
    )
    {
        var result = await RunComposeAsync(project, $"exec -T web stat -c {format} {path}", 60_000);
        Assert.True(result.ExitCode == 0, $"Inspecting {path} in web failed.");
        return result.Output.Trim();
    }

    private static Task<string[]> ListDataProtectionKeyFilesAsync(ComposeProject project) =>
        ListKeyRingFilesAsync(project, $"exec -T web ls -1 {WebKeyRingPath}");

    private static async Task<string[]> ListKeyRingFilesAsync(
        ComposeProject project,
        string subcommand
    )
    {
        // Names only: key XML content is never read or printed.
        var listing = await RunComposeAsync(project, subcommand, 60_000);
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

    private static Task<(int ExitCode, string Output)> RunComposeAsync(
        ComposeProject project,
        string subcommand,
        int timeoutMs
    ) =>
        RunProcessAsync(
            "docker",
            $"{project.Arguments} {subcommand}",
            project.DeployDirectory,
            project.Env,
            timeoutMs
        );

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

    private static string ResolveDockerPlatform() =>
        RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.Arm64 => "linux/arm64/v8",
            _ => "linux/amd64",
        };

    // CI's temporary bind mount is owned by the test runner, not the container's non-root UID
    // 1654. Allow that UID to exercise readiness in this disposable directory only. Production
    // setup MUST remain 1654:1654 with mode 0750.
    private static void MakeDisposableImageRootWritable(string imageRoot)
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

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

    private const string PortOverrideYaml = """
        services:
          web:
            ports:
              - "127.0.0.1:${PUBLIC_HTTP_PORT}:8080"
          api:
            ports:
              - "127.0.0.1:${API_HTTP_PORT}:8080"
        """;
    private const string InitServiceName = "web-data-protection-init";
    private const string WebKeyRingPath = "/var/lib/whattocook/data-protection";
    private const string WebKeyRingVolume = "web-data-protection";
    private const string WebCertificatePath = "/var/lib/whattocook/data-protection-certificate";
    private const string WebCertificateVolume = "web-data-protection-certificate";
    private const string WebCertificateFile = WebCertificatePath + "/web-data-protection.pem";
    private const string OwnerModeFormat = "%u:%g:%a";
    private const string AntiforgeryCookiePrefix = ".AspNetCore.Antiforgery";
    private const string AntiforgeryFieldName = "__RequestVerificationToken";
    private const string RecipeEditorFormName = "recipe-editor";
    private const string RecipeTitleRequiredMessage = "Tytuł jest wymagany.";
    private const string DecryptFailureMessage =
        "The existing Data Protection key ring cannot be decrypted with the configured certificate. Restore the matching certificate and key ring.";
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

    private sealed record ComposeProject(
        string ProjectName,
        string DeployDirectory,
        string ComposeFile,
        string OverrideFile,
        IReadOnlyDictionary<string, string?> Env
    )
    {
        public string Arguments =>
            $"compose -f \"{ComposeFile}\" -f \"{OverrideFile}\" --project-name {ProjectName}";
    }

    private sealed class RecipeEditorForm(IReadOnlyList<KeyValuePair<string, string>> fields)
    {
        public IReadOnlyList<KeyValuePair<string, string>> Fields { get; } = fields;

        public string RequestToken =>
            Fields.Single(input => input.Key == AntiforgeryFieldName).Value;

        public RecipeEditorForm WithRequestToken(string requestToken) =>
            new(
                Fields
                    .Select(field =>
                        field.Key == AntiforgeryFieldName
                            ? new KeyValuePair<string, string>(field.Key, requestToken)
                            : field
                    )
                    .ToList()
            );
    }

    // Holds the antiforgery cookie minted before the web recreation and replays it afterwards.
    private sealed class AntiforgeryProbe : IDisposable
    {
        private readonly Uri _pageUri;
        private readonly CookieContainer _cookies;
        private readonly HttpClient _client;
        private readonly RecipeEditorForm _issuedForm;
        private readonly string _issuedCookieValue;

        private AntiforgeryProbe(
            Uri pageUri,
            CookieContainer cookies,
            HttpClient client,
            RecipeEditorForm issuedForm
        )
        {
            _pageUri = pageUri;
            _cookies = cookies;
            _client = client;
            _issuedForm = issuedForm;
            _issuedCookieValue = ReadAntiforgeryCookieValue(cookies, pageUri);
        }

        public static async Task<AntiforgeryProbe> IssueAsync(Uri pageUri)
        {
            var cookies = new CookieContainer();
            var client = CreateAntiforgeryClient(cookies);
            try
            {
                var form = await GetRecipeEditorFormAsync(client, pageUri);
                Assert.True(
                    HasAntiforgeryCookie(cookies, pageUri),
                    "GET /recipes/new did not issue an antiforgery cookie."
                );
                return new AntiforgeryProbe(pageUri, cookies, client, form);
            }
            catch
            {
                client.Dispose();
                throw;
            }
        }

        // The recreated web must decrypt the existing cookie (no Set-Cookie replacement), keep its
        // value, accept the matching token, and still reject tampered and cookie-less posts.
        // Cookie values are compared as booleans and never printed.
        public async Task AssertCookieSurvivesRecreateAsync()
        {
            using var response = await _client.GetAsync(_pageUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(
                response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders)
                    && setCookieHeaders.Any(header =>
                        header.StartsWith(AntiforgeryCookiePrefix, StringComparison.Ordinal)
                    ),
                "Recreated web replaced the pre-recreation antiforgery cookie instead of decrypting it."
            );
            var reissuedForm = ParseRecipeEditorForm(await response.Content.ReadAsStringAsync());
            Assert.False(string.IsNullOrEmpty(reissuedForm.RequestToken));
            Assert.True(
                string.Equals(
                    _issuedCookieValue,
                    ReadAntiforgeryCookieValue(_cookies, _pageUri),
                    StringComparison.Ordinal
                ),
                "Antiforgery cookie value changed across web recreation."
            );

            // An empty title triggers the form handler's validation without writing a recipe.
            var (acceptedStatus, acceptedBody) = await PostRecipeEditorFormAsync(
                _client,
                _pageUri,
                _issuedForm
            );
            Assert.Equal(HttpStatusCode.OK, acceptedStatus);
            Assert.Contains(
                RecipeTitleRequiredMessage,
                WebUtility.HtmlDecode(acceptedBody),
                StringComparison.Ordinal
            );

            var tamperedForm = _issuedForm.WithRequestToken(TamperToken(_issuedForm.RequestToken));
            Assert.Equal(
                HttpStatusCode.BadRequest,
                (await PostRecipeEditorFormAsync(_client, _pageUri, tamperedForm)).StatusCode
            );

            using var cookielessClient = CreateAntiforgeryClient(new CookieContainer());
            Assert.Equal(
                HttpStatusCode.BadRequest,
                (
                    await PostRecipeEditorFormAsync(cookielessClient, _pageUri, _issuedForm)
                ).StatusCode
            );
        }

        public void Dispose() => _client.Dispose();
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
                cookie.Name.StartsWith(AntiforgeryCookiePrefix, StringComparison.Ordinal)
            );

    private static string ReadAntiforgeryCookieValue(CookieContainer cookies, Uri uri) =>
        cookies
            .GetCookies(uri)
            .Single(cookie =>
                cookie.Name.StartsWith(AntiforgeryCookiePrefix, StringComparison.Ordinal)
            )
            .Value;

    private static async Task<(HttpStatusCode StatusCode, string Body)> PostRecipeEditorFormAsync(
        HttpClient client,
        Uri pageUri,
        RecipeEditorForm form
    )
    {
        using var content = new FormUrlEncodedContent(form.Fields);
        using var response = await client.PostAsync(pageUri, content);
        return (response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    // Changes a middle character: the final base64 character may encode only padding bits, so
    // altering it can leave the decoded token unchanged.
    private static string TamperToken(string token)
    {
        var index = token.Length / 2;
        var replacement = token[index] == 'A' ? 'B' : 'A';
        return token[..index] + replacement + token[(index + 1)..];
    }
}

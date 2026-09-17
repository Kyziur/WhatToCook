const fs = require("fs");
const net = require("net");
const path = require("path");
const { spawn } = require("child_process");

const repoRoot = path.resolve(__dirname, "..", "..", "..");
const tempRoot = path.join(repoRoot, ".tmp", "playwright-e2e");
const runId = `${Date.now()}-${Math.floor(Math.random() * 10_000)}`;
const containerName = `wtc-playwright-pg-${runId}`;
const databaseName = `wtc_playwright_${runId.replace(/-/g, "_")}`;
const databaseUser = "wtc";
const databasePassword = "wtc";
const recipeImagesPath = path.join(tempRoot, `recipe-images-${runId}`);

let apiProcess = null;
let shuttingDown = false;

async function main() {
  fs.mkdirSync(recipeImagesPath, { recursive: true });

  const hostPort = await allocatePort();
  await runDocker([
    "run",
    "--rm",
    "--name",
    containerName,
    "-e",
    `POSTGRES_DB=${databaseName}`,
    "-e",
    `POSTGRES_USER=${databaseUser}`,
    "-e",
    `POSTGRES_PASSWORD=${databasePassword}`,
    "-p",
    `${hostPort}:5432`,
    "-d",
    "postgres:17-alpine",
  ]);

  await waitForPostgres(hostPort);

  const connectionString = `Host=127.0.0.1;Port=${hostPort};Database=${databaseName};Username=${databaseUser};Password=${databasePassword}`;
  apiProcess = spawn(
    "dotnet",
    ["run", "--project", "src/WhatToCook.Api", "--no-build", "--urls", "http://127.0.0.1:5181"],
    {
      cwd: repoRoot,
      stdio: "inherit",
      env: {
        ...process.env,
        ASPNETCORE_ENVIRONMENT: "Development",
        ASPNETCORE_DETAILEDERRORS: "true",
        "ConnectionStrings__whattocook-db": connectionString,
        "Storage__RecipeImagesPath": recipeImagesPath,
        "SeedData__Enabled": "true",
      },
    }
  );

  apiProcess.on("exit", async (code, signal) => {
    if (!shuttingDown) {
      await shutdown(code ?? (signal ? 1 : 0));
    }
  });

  await new Promise((resolve, reject) => {
    apiProcess.on("error", reject);
    apiProcess.on("exit", resolve);
  });
}

async function shutdown(exitCode) {
  if (shuttingDown) {
    return;
  }

  shuttingDown = true;

  if (apiProcess && !apiProcess.killed) {
    apiProcess.kill("SIGTERM");
  }

  await stopDockerContainer();
  process.exit(exitCode);
}

function allocatePort() {
  return new Promise((resolve, reject) => {
    const server = net.createServer();
    server.listen(0, "127.0.0.1", () => {
      const address = server.address();
      if (!address || typeof address === "string") {
        server.close();
        reject(new Error("Could not allocate a TCP port for PostgreSQL."));
        return;
      }

      const { port } = address;
      server.close((error) => {
        if (error) {
          reject(error);
          return;
        }

        resolve(port);
      });
    });
    server.on("error", reject);
  });
}

function runDocker(args) {
  return new Promise((resolve, reject) => {
    const docker = spawn("docker", args, {
      cwd: repoRoot,
      stdio: ["ignore", "pipe", "pipe"],
    });

    let stdout = "";
    let stderr = "";

    docker.stdout.on("data", (chunk) => {
      stdout += chunk.toString();
    });

    docker.stderr.on("data", (chunk) => {
      stderr += chunk.toString();
    });

    docker.on("error", reject);
    docker.on("exit", (code) => {
      if (code === 0) {
        resolve(stdout.trim());
        return;
      }

      reject(new Error(`docker ${args.join(" ")} failed with code ${code}: ${stderr.trim()}`));
    });
  });
}

async function waitForPostgres(hostPort) {
  const deadline = Date.now() + 60_000;

  while (Date.now() < deadline) {
    try {
      await runDocker([
        "exec",
        containerName,
        "pg_isready",
        "-U",
        databaseUser,
        "-d",
        databaseName,
      ]);
      return;
    } catch {
      await delay(1_000);
    }
  }

  throw new Error(`PostgreSQL container ${containerName} did not become ready on port ${hostPort}.`);
}

async function stopDockerContainer() {
  try {
    await runDocker(["rm", "-f", containerName]);
  } catch {
    // Best effort cleanup only.
  }
}

function delay(milliseconds) {
  return new Promise((resolve) => setTimeout(resolve, milliseconds));
}

process.on("SIGINT", () => {
  void shutdown(0);
});

process.on("SIGTERM", () => {
  void shutdown(0);
});

void main().catch(async (error) => {
  console.error(error);
  await stopDockerContainer();
  process.exit(1);
});

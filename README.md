# WhatToCook

WhatToCook is a self-hosted recipe catalog, meal planner, and shopping list for a household LAN.

## Local development

Requires the **.NET 10 SDK** selected by [`global.json`](global.json), **Node.js 22** for browser tests, and a running **Docker Engine** (Docker Desktop on Windows) for PostgreSQL and integration tests.

### Start the application

From the repository root:

```shell
dotnet restore WhatToCook.slnx
dotnet build WhatToCook.slnx
dotnet run --project src/WhatToCook.AppHost
```

`restore` downloads .NET packages, `build` compiles the solution, and `run` starts the Aspire-managed PostgreSQL 17, API, and Blazor Web application. Use the Web address printed by Aspire. PostgreSQL data uses the existing local Aspire volume; do not delete or replace it as a troubleshooting shortcut.

**Use the AppHost for normal development.** Running the API by itself requires a database connection string supplied through Aspire, environment variables, or user secrets; production credentials do not belong in this repository.

### Run tests

Keep Docker running; integration tests use PostgreSQL through Testcontainers.

```shell
dotnet test WhatToCook.slnx
npm ci
npx playwright install chromium
npx playwright test --config=playwright.config.ts
```

These commands run the .NET tests, install the pinned Node dependencies, install the Playwright browser, and run the desktop/mobile E2E suite. Playwright launches its own temporary PostgreSQL, API (`127.0.0.1:5181`), and Web (`127.0.0.1:5180`) servers; stop other processes occupying those ports. A normal local `dotnet build` uses Debug, which matches the Playwright default. CI builds Release and sets `PLAYWRIGHT_DOTNET_CONFIGURATION=Release`.

### When setup fails

- `docker info` checks whether the Docker daemon is reachable. Start Docker Desktop/the daemon before Aspire, Testcontainers, or Playwright E2E tests.
- `dotnet --version` checks that the .NET 10 SDK selected by `global.json` can be resolved.
- If Playwright cannot find Chromium, run `npx playwright install chromium` again. If E2E startup fails, first check whether ports 5180/5181 are already occupied.
- If direct API startup reports a missing database connection string, use the AppHost command above rather than adding credentials to source files.

Repository change conventions and verification requirements are in [`AGENTS.md`](AGENTS.md); product requirements are in [`openspec/specs`](openspec/specs).

## Architecture and trust boundary

[Trust-boundary decision](docs/architecture/trust-boundary.md) explains how browsers, Blazor Web, the internal API and PostgreSQL are separated, and which access-control decisions are required **before** enabling external users or public routes. It does not change deployment or introduce authentication.

## Production deployment

Production deployment is owned by the homelab repository and Komodo. This repository provides the application Compose contract in `deploy/docker-compose.yml`; it does not contain production secret values or the Komodo configuration.

Bitwarden Secrets Manager supplies deployment secrets through a trusted host-side wrapper or Komodo Action. Do not put secret values in GitHub or in a Komodo Stack `environment` block.

See [`docs/self-hosted-deployment.md`](docs/self-hosted-deployment.md) for the Komodo/Bitwarden contract, persistence requirements, backup and restore, rotation, and verification checklist. Raspberry Pi-specific migration notes are in [`docs/deploy-raspberry.md`](docs/deploy-raspberry.md).

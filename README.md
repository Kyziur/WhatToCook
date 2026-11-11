# WhatToCook

WhatToCook helps a household **save recipes, plan meals, and generate shopping lists** without relying on a hosted recipe service. It is designed for a trusted home network, not for publicly accessible or multi-user accounts.

The app has three main workflows: keep a recipe catalog (including URL imports and photos), assemble a meal plan, and turn planned meals into a shopping list. The Web UI is Blazor Interactive Server; an internal .NET API stores data in PostgreSQL.

## Run locally

From the repository root, install the [.NET 10 SDK](global.json) and start Docker (Docker Desktop on Windows). Then run:

```powershell
dotnet restore WhatToCook.slnx
dotnet run --project src/WhatToCook.AppHost
```

The first command restores NuGet dependencies. The second starts the Aspire-managed PostgreSQL, API, and Web app; open the Web URL reported by Aspire. **Do not delete the local PostgreSQL volume** as a troubleshooting shortcut.

## Verify a change

With Docker running, use these commands from the repository root:

```powershell
dotnet build WhatToCook.slnx
dotnet test WhatToCook.slnx
npm ci
npx playwright install chromium
npx playwright test --config=playwright.config.ts
```

They build the solution, run .NET tests (including PostgreSQL Testcontainers), install pinned Node dependencies, install Chromium, and run browser E2E tests, respectively. Browser tests require Node.js 22 and use ports **5180/5181** for their temporary Web/API processes; free those ports first. For the complete handoff/formatting policy, see [AGENTS.md](AGENTS.md).

## Find the answer

- [Architecture and access boundary](docs/architecture/trust-boundary.md) — what talks to what, and why the API must stay private.
- [Self-hosted deployment](docs/self-hosted-deployment.md) — ownership, preflight, stop conditions and verification; **no production deployment is performed from this README**.
- [Performance decisions](docs/performance.md) — what was measured and what not to optimize yet.
- [Repowise setup](docs/development/repowise-mcp.md) — optional local agent index and freshness checks.
- [OpenSpec](openspec/specs) — intended product contracts; [audit tracker](docs/audits/2026-11-02-repository-audit.md) — dated findings and outstanding operational checks.

**Important:** The [homelab repository](https://github.com/Kyziur/homelab) owns live Komodo configuration, external access, secrets, backups, and recovery. Repository Compose configuration does not prove the running host is secure or ready to migrate.

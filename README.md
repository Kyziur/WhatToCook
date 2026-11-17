# WhatToCook
### Line endings

Repository text is LF. `.gitattributes` sets `* text=auto eol=lf`, so committed and checked-out text stays LF even with `core.autocrlf=true` (the Git for Windows default). Do not change global Git config to work around it.

- Diagnose with `git ls-files --eol <path>`: `i/` is the committed index, `w/` is the working tree. Text should show `i/lf w/lf`.
- Check the repository with `python scripts/ci/check-line-endings.py`. The PR CI `line-endings` job runs it on Linux and Windows with `core.autocrlf` true and false.
- Only a byte-exact fixture may keep CRLF, via an explicit `eol=crlf` attribute listed in `.gitattributes` with its reason.
- To refresh an existing clone that has CRLF files, first confirm `git status --short` is empty, then run `git rm -r --cached -q . && git reset --hard`. If there are local changes, commit or stash them first; never reset over them.


WhatToCook helps a household **save recipes, plan meals, and generate shopping lists** without relying on a hosted recipe service. It is designed for a trusted home network, not for publicly accessible or multi-user accounts.

The app has three main workflows: keep a recipe catalog (including URL imports and photos), assemble a meal plan, and turn planned meals into a shopping list. The Web UI is Blazor Interactive Server; an internal .NET API stores data in PostgreSQL.

## Run locally

From the repository root, install the [.NET 10 SDK](global.json) and start Docker (Docker Desktop on Windows). Then run:

```powershell
dotnet restore WhatToCook.slnx
dotnet run --project src/WhatToCook.AppHost
```

The first command restores NuGet dependencies. The second starts the Aspire-managed PostgreSQL, API, and Web app; open the Web URL reported by Aspire. **Do not delete the local PostgreSQL volume** as a troubleshooting shortcut.

### When setup fails

- `docker info` checks whether the Docker daemon is reachable. Start Docker Desktop/the daemon before Aspire, Testcontainers, or Playwright E2E tests.
- `dotnet --version` checks that the .NET 10 SDK selected by `global.json` can be resolved.
- If Playwright cannot find Chromium, run `npx playwright install chromium` again. If E2E startup fails, first check whether ports 5180/5181 are already occupied.
- If direct API startup reports a missing database connection string, use the AppHost command above rather than adding credentials to source files.

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

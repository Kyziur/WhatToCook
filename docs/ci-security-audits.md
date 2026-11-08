# Dependency audit gate (SEC-08)

The PR workflow checks known dependency advisories without introducing another scanner:

- **NuGet:** `dotnet package list --vulnerable --include-transitive --no-restore --format json --output-version 1`, evaluated by `scripts/ci/check-nuget-audit.mjs`.
- **npm:** `npm audit --audit-level=high --json`.
- **Policy:** High and Critical advisories fail CI for both ecosystems; Low and Moderate NuGet findings are reported but do not block. An unreadable/unsupported NuGet report or a failing audit command also fails CI.
- **Restore behavior:** the solution uses `TreatWarningsAsErrors=true`. NuGet advisory codes `NU1901`–`NU1904` are explicitly exempted from that blanket setting so the audited JSON gate, rather than an earlier restore, determines the agreed severity threshold and retains evidence. Audit-source failures (`NU1900`, `NU1905`) remain errors.
- **Evidence:** machine-readable `nuget-audit.json` and `npm-audit.json` are attached to each PR CI run as the `dependency-audits` artifact, including when the scan step fails. Only package/advisory metadata is expected; never add tokens or private registry credentials to the report.
- **Verification:** `node --test tests/ci/nuget-audit-gate.test.mjs` tests clean, warning-only, blocking, and malformed report cases. GitHub Actions separately checks the actual NuGet/npm command execution.

Advisories depend on the configured NuGet sources and npm registry; a clean report means no *known* vulnerabilities from those sources at that run. This gate does **not** assert the presence of secret scanning, code scanning, SBOM generation, or deployment/runtime security. Review other GitHub security controls separately before adding duplicate scanners. For production host access, secrets, and backups, see [self-hosted deployment](self-hosted-deployment.md).

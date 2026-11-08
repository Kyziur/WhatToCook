# SEC-08 verification and review evidence

Verified against PR #23 head `ad8bf7785e7f0428cbf631e8ee1d65457d5880dc` on 2026-11-08.

- [GitHub Actions run 42465836173](https://github.com/Kyziur/WhatToCook/actions/runs/42465836173): `verify` and ARM64 recipe image smoke both succeeded, including .NET build/test, Playwright, dependency audits, formatting, and upload of results.
- NuGet gate tests: **5 passed** (clean, warning-only, High/Critical direct/transitive, malformed reports, actual CLI exit codes). The actual NuGet scan reported **0 advisory occurrences** from the configured sources.
- The npm audit step passed. Both reports were uploaded as the `dependency-audits` artifact (ID 13611053505).
- Review iteration: added a subprocess-level CLI exit-code regression after observing that parser-only unit tests would not prove the CI process fails. Commit: `ad8bf7785e7f0428cbf631e8ee1d65457d5880dc`.
- Scope check: no application runtime changes, external secrets, homelab host changes, package version changes or new scanning dependencies.

**Remaining boundaries:** The CI result establishes only package-advisory scan results available from the configured sources at that time. It does not validate the production environment, secret scanning, SAST, SBOM, or NuGet audit-source completeness during an upstream outage. Those remain separate checks; do not treat a clean package scan as a general security certification.

# OBS-05 application logging — verification evidence

Verified against PR #26 code head `b1f550a66f18662ab9dc36c49d2d3a7c5400636a` (2026-11-08).

- [GitHub Actions 42476464381](https://github.com/Kyziur/WhatToCook/actions/runs/42476464381): `verify` and ARM64 image smoke succeeded.
- `.NET` integration tests: 115 passed (including new `RecipeLoggingPrivacySpecificationTests`). Component tests: 53 passed. Playwright: 94 passed. CSharpier: 115 files verified. Existing NuGet/npm vulnerability gates passed.
- HTTP regression uses the real PostgreSQL Testcontainers factory and captures formatted messages plus structured fields from `EditorEndpointsLog`. A marker title is exercised through successful create, duplicate create and validation failure. It must not appear in log output or log-state values.
- Review fix 1: verify that neither recipe titles nor normalized-title values are logged, and that recipe photo replacement log messages do not include stored/host paths or exception objects.
- Review fix 2: apply exact CSharpier formatting of the new test file and remove the temporary formatter diagnostic workflow step (commit `b1f550a66f18662ab9dc36c49d2d3a7c5400636a`).

**Not verified/remaining:** actual OTLP collector payload transformation, retention limits, metric-label cardinality and redaction policy are homelab responsibilities. This app-side PR only partially addresses audit OBS-05. Do not treat the green CI as proof of collector compliance.

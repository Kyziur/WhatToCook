# WhatToCook repository audit and action plan

- **Audit snapshot:** [`4eb64d7e3de01faa7bef32db74da94e7e1c50913`](https://github.com/Kyziur/WhatToCook/tree/4eb64d7e3de01faa7bef32db74da94e7e1c50913), audited on 2026-11-02.
- **Assessment:** recommendations reviewed against the repository on 2026-11-06. A finding's priority below is the assessment's recommendation, not necessarily the original audit's priority.
- **Status:** backlog, not evidence that a fix, deployment, backup, or mitigation has been completed. Recheck current code and operational state before implementing an item.

## Decision

The application does not need a wholesale rewrite. First make CI trustworthy, fix concrete data-validation and shopping-list problems, and verify the network and recovery boundaries before rollout. Follow with end-to-end catalog pagination and focused accessibility, diagnostics, and maintainability work. Performance changes and additional operational or agent tooling should be driven by a demonstrated need, an exposure change, or measurements.

The audit contains 69 original finding IDs. Their assessments, including optional opportunities, live in these chapters:

1. [Correctness, CI, and user experience](2026-11-02-correctness-ci-ux.md): `COR`, `CI`, and `UX` findings.
2. [Security, deployment, and observability](2026-11-02-security-operations.md): `SEC`, `OPS`, and `OBS` findings.
3. [Performance, architecture, documentation, and agent tooling](2026-11-02-performance-maintainability.md): `PERF`, `ARC`, `DOC`, and `AGENT` findings.

Each chapter identifies a recommended action, evidence or the limit of available evidence, and a verification approach. Optional items include the condition under which the work is worthwhile; an unmeasured cost or an unverified exposure is not recorded as a production incident.

## Priority and ownership

- **P1:** address first. For rollout-gated items, the prerequisite is a verified operational outcome, not necessarily an application-code change.
- **P2:** worthwhile after P1 or when the stated trigger applies.
- **P3 / optional:** do alongside affected work or after a measurement demonstrates value. An optional item is not a commitment to implement it regardless of cost.
- **Verify first:** a plausible risk or behavior requiring an HTTP, browser, CI, or homelab check before choosing a fix.

The application repository owns application code, images, and the passive Compose contract. The homelab owns host preparation, Komodo/Periphery, secret injection, shared networks, routing, and backups. Preserve the existing BWS workload scope and approved wrapper; the application must not require `BWS_ACCESS_TOKEN`. See the [self-hosted deployment contract](../self-hosted-deployment.md).

## Recommended sequence

1. **CI:** align Release builds with both Playwright server processes while preserving local Debug use; propagate the exit code of each PowerShell command. Validate on a clean runner. `CI-01`, `CI-02`.
2. **Input correctness:** handle normalized-key collisions, draft limits, plan and manual-item limits, and explicit nulls with controlled API behavior. Keep regression coverage at the relevant HTTP boundary. `COR-01`, `COR-02`, `COR-05`, `COR-06`, `COR-08`, `CI-05`.
3. **Shopping semantics:** decide how descriptive quantities and multipliers are represented, and when regenerated items retain checklist state. Update the specification if its stable-key contract changes. `COR-03`, `COR-04`.
4. **Before rollout:** verify actual Web/API reachability and access controls, the existing Compose project and PostgreSQL volume, backup and isolated restore of database and photos, and DB-backed plus image write/read smoke checks. Homelab configuration and live results are outside this repository's static evidence. `SEC-01`, `SEC-02`, `OPS-02`, `OPS-03`, `OPS-04`, `OPS-06`, `OPS-07`, `OBS-02`, `CI-04`.
5. **Catalog correctness and bounded work:** design pagination and filtering across the whole library and search. The library currently takes at most 500 recipes before the UI filters them; search API tag matching and UI tag matching differ. Preserve filtering and sorting semantics rather than silently truncating results. `PERF-01`.
6. **Conditional improvements:** investigate importer egress/SSRF protections, accessibility and reconnect, telemetry, privacy, immutable images, and maintainability where the chapters' triggers apply. Benchmark SQL, image transfer, rendering, and capacity before optimizing.

Keep changes small and independently reviewable. Avoid combining the application and homelab changes into one oversized PR; each side should have an owner and a verifiable contract. Existing tests and safeguards should be extended where they are insufficient, not described as absent simply because a particular edge case is missing.

## Evidence and limitations

The original audit described a static review of one snapshot, not a production incident investigation. The subsequent assessment read current source, tests, configuration, documentation, and relevant OpenSpec requirements. It did not run the complete CI pipeline, application test suite, browser accessibility checks, dependency/security scans, production containers, or homelab backup/restore.

Two narrow local checks were performed during the assessment: the Web project's default MSBuild `Configuration` evaluated to `Debug`, and Windows PowerShell demonstrated that a succeeding native command can replace an earlier nonzero `$LASTEXITCODE`. Those observations do not establish the result of a GitHub-hosted CI run. Actual network reachability, credentials, image permissions on the target host, alert delivery, and backup/restore remain operational checks, not facts inferred from Compose or documentation.

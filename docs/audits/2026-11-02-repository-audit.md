# WhatToCook repository audit and action plan

- **Audit snapshot:** [`4eb64d7e3de01faa7bef32db74da94e7e1c50913`](https://github.com/Kyziur/WhatToCook/tree/4eb64d7e3de01faa7bef32db74da94e7e1c50913), audited on 2026-11-02.
- **Assessment:** recommendations reviewed against the repository on 2026-11-06. A finding's priority below is the assessment's recommendation, not necessarily the original audit's priority.
- **Status:** historical findings and priorities; see the implementation tracker below for merged work, open work and required operational verification.

## Decision

The application does not need a wholesale rewrite. First make CI trustworthy, fix concrete data-validation and shopping-list problems, and verify the network and recovery boundaries before rollout. Follow with end-to-end catalog pagination and focused accessibility, diagnostics, and maintainability work. Performance changes and additional operational or agent tooling should be driven by a demonstrated need, an exposure change, or measurements.

The audit contains 69 original finding IDs. Their assessments, including optional opportunities, live in these chapters:

1. [Correctness, CI, and user experience](2026-11-02-correctness-ci-ux.md): `COR`, `CI`, and `UX` findings.
2. [Security, deployment, and observability](2026-11-02-security-operations.md): `SEC`, `OPS`, and `OBS` findings.
3. [Performance, architecture, documentation, and agent tooling](2026-11-02-performance-maintainability.md): `PERF`, `ARC`, `DOC`, and `AGENT` findings.

Each chapter identifies a recommended action, evidence or the limit of available evidence, and a verification approach. Optional items include the condition under which the work is worthwhile; an unmeasured cost or an unverified exposure is not recorded as a production incident.

## Implementation tracker (updated 2026-11-08)

This is the **live implementation tracker** for the 69 original findings. It is separate from the historical 2026-11-02 static audit and 2026-11-06 assessment.

- **DONE · CI** means source/tests were merged to `main` and relevant GitHub PR CI passed. It does **not** establish production/homelab readiness.
- **PARTIAL** means only a clearly scoped piece was implemented.
- **IN REVIEW** means an unmerged PR exists; CI or actual host verification remains outstanding.
- **OPEN / CONDITIONAL** means no verified closure; optional work should not be implemented without its stated trigger.

**Current count:** 25 DONE · CI, 6 PARTIAL, 1 IN REVIEW, 37 open/conditional. The original audit's 69 findings remain unchanged.

| Finding | Status | Merged evidence or outstanding work |
| --- | --- | --- |
| COR-01 | **DONE · CI** | [#4](https://github.com/Kyziur/WhatToCook/pull/4) — Lossless normalized ingredient/tag collisions |
| COR-02 | **DONE · CI** | [#7](https://github.com/Kyziur/WhatToCook/pull/7), [#12](https://github.com/Kyziur/WhatToCook/pull/12) — Bound import drafts and Unicode scalar length |
| COR-03 | **DONE · CI** | [#15](https://github.com/Kyziur/WhatToCook/pull/15) — Display descriptive quantities, repetitions and factors |
| COR-04 | **DONE · CI** | [#16](https://github.com/Kyziur/WhatToCook/pull/16) — Reset checklist state when amount semantics change |
| COR-05 | **DONE · CI** | [#2](https://github.com/Kyziur/WhatToCook/pull/2) — Validate plan name length on create and update |
| COR-06 | **DONE · CI** | [#3](https://github.com/Kyziur/WhatToCook/pull/3) — Validate manual shopping item length |
| COR-07 | **DONE · CI** | [#13](https://github.com/Kyziur/WhatToCook/pull/13) — Atomic concurrent draft finalization |
| COR-08 | **DONE · CI** | [#5](https://github.com/Kyziur/WhatToCook/pull/5) — Controlled HTTP validation of explicit nulls |
| COR-09 | OPEN / CONDITIONAL | Optional: document catalog-level normalized-title uniqueness |
| CI-01 | **DONE · CI** | [#1](https://github.com/Kyziur/WhatToCook/pull/1) — Release E2E processes match the CI build |
| CI-02 | **DONE · CI** | [#1](https://github.com/Kyziur/WhatToCook/pull/1) — Native-command exit codes no longer masked |
| CI-03 | OPEN / CONDITIONAL | Optional: split CI only after duration evidence |
| CI-04 | OPEN / CONDITIONAL | Pre-rollout: deployed PostgreSQL and recipe-photo write/read proof |
| CI-05 | **DONE · CI** | [#2](https://github.com/Kyziur/WhatToCook/pull/2), [#3](https://github.com/Kyziur/WhatToCook/pull/3), [#4](https://github.com/Kyziur/WhatToCook/pull/4), [#5](https://github.com/Kyziur/WhatToCook/pull/5), [#7](https://github.com/Kyziur/WhatToCook/pull/7), [#12](https://github.com/Kyziur/WhatToCook/pull/12) — Targeted HTTP input-boundary regressions for correctness changes |
| CI-06 | OPEN / CONDITIONAL | Optional: coverage reporting after baseline |
| CI-07 | **DONE · CI** | [#14](https://github.com/Kyziur/WhatToCook/pull/14) — Compose contract assertions instead of documentation phrasing |
| UX-01 | OPEN / CONDITIONAL | Optional: split large UI state only for concrete maintenance cost |
| UX-02 | **PARTIAL** | [#20](https://github.com/Kyziur/WhatToCook/pull/20) — Keyboard/accessibility smoke delivered; axe-based and broader focus tests outstanding |
| UX-03 | **DONE · CI** | [#25](https://github.com/Kyziur/WhatToCook/pull/25) — Desktop/mobile Playwright covers a 10-second SignalR reconnect, unsaved editor/planner drafts and exactly-one save. Long circuit eviction/offline editing remains outside this finding's tested scope |
| UX-04 | OPEN / CONDITIONAL | Optional: establish mobile performance budget from measurements |
| UX-05 | OPEN / CONDITIONAL | Conditional: actionable import errors if generic recovery messages prove insufficient |
| SEC-01 | OPEN / CONDITIONAL | Pre-rollout: real API authorization and access boundary verification |
| SEC-02 | OPEN / CONDITIONAL | Pre-rollout: actual network reachability/isolation |
| SEC-03 | OPEN / CONDITIONAL | Conditional: resolve effective DNS/proxy egress path before importer exposure |
| SEC-04 | OPEN / CONDITIONAL | Before browser-auth/exposure change: CSRF review |
| SEC-05 | OPEN / CONDITIONAL | Conditional: photo EXIF policy based on exposure/privacy |
| SEC-06 | OPEN / CONDITIONAL | Conditional: API abuse limits if untrusted clients can reach it |
| SEC-07 | OPEN / CONDITIONAL | Optional: OpenAPI exposure policy |
| SEC-08 | **PARTIAL** | [#23](https://github.com/Kyziur/WhatToCook/pull/23) — NuGet/npm severity gates and retained reports delivered; inventory of other GitHub security controls unverified |
| OPS-01 | OPEN / CONDITIONAL | Before schema release: single migrator/rollback proof |
| OPS-02 | OPEN / CONDITIONAL | Pre-rollout: isolated PostgreSQL + image restore drill |
| OPS-03 | OPEN / CONDITIONAL | Pre-rollout: preserve Compose project and PostgreSQL volume identity |
| OPS-04 | OPEN / CONDITIONAL | Pre-rollout: verify UID/GID 1654 image mount write/read |
| OPS-05 | OPEN / CONDITIONAL | Conditional: immutable images for reproducible release/rollback |
| OPS-06 | OPEN / CONDITIONAL | Pre-rollout: external Docker networks and allowed directions |
| OPS-07 | OPEN / CONDITIONAL | Pre-rollout: verified backup, rollback and release gate |
| OPS-08 | OPEN / CONDITIONAL | Optional: tune host resources only after profiling |
| OBS-01 | OPEN / CONDITIONAL | Optional: extra workflow spans only if standard traces inadequate |
| OBS-02 | OPEN / CONDITIONAL | Pre-rollout: DB and image storage readiness against deployment |
| OBS-03 | OPEN / CONDITIONAL | Operational: SLI/SLO and alert ownership if monitored |
| OBS-04 | **DONE · CI** | [#21](https://github.com/Kyziur/WhatToCook/pull/21) — Bounded import-failure categories without source-content logging |
| OBS-05 | **PARTIAL** | [#26](https://github.com/Kyziur/WhatToCook/pull/26) — Recipe write logs no longer include user-controlled titles or internal photo paths; homelab collector redaction, retention and cardinality verification outstanding |
| OBS-06 | OPEN / CONDITIONAL | Operational: verify OTLP export-to-backend if telemetry enabled |
| PERF-01 | **DONE · CI** | [#17](https://github.com/Kyziur/WhatToCook/pull/17) — Full library/search paging; follow-up [#37](https://github.com/Kyziur/WhatToCook/pull/37) removed remaining 500-item planner truncation using bounded 50-item pages and optional no-facets query (final-head [CI 42513645703](https://github.com/Kyziur/WhatToCook/actions/runs/42513645703) green: 121 API, 56 component, 98 browser tests). Planner still materializes summaries in its Blazor circuit; profile capacity before further refactoring |
| PERF-02 | **DONE · CI** | [#6](https://github.com/Kyziur/WhatToCook/pull/6) — Removed inactive output-cache annotation |
| PERF-03 | **DONE · CI** | [#32](https://github.com/Kyziur/WhatToCook/pull/32) — PostgreSQL 1k/10k `EXPLAIN (ANALYZE, BUFFERS)` comparisons show no justified `pg_trgm` migration at household scale; synthetic index and matching regression tests only, production latency remains unmeasured |
| PERF-04 | **DONE · CI** | [#33](https://github.com/Kyziur/WhatToCook/pull/33) — PostgreSQL 100/1k/10k candidate benchmark and ranking parity; SQL `EXISTS` prefilter avoids materializing unmatched recipes (10k synthetic: 9,999→499), with no pre-rank limit; full CI [42491096130](https://github.com/Kyziur/WhatToCook/actions/runs/42491096130) green. Production p95 unmeasured |
| PERF-05 | **DONE · CI** | [#34](https://github.com/Kyziur/WhatToCook/pull/34) — Development-only Blazor circuit/render metrics and 5/20-tab diagnostics merged, final-head [CI 42506756813](https://github.com/Kyziur/WhatToCook/actions/runs/42506756813) green. Single-run server render p95 51.66/77.16 ms on shared runner; no production p95 or demonstrated need for UI refactor. Optional host repeats remain |
| PERF-06 | OPEN / CONDITIONAL | Optional: image variants after network measurements |
| PERF-07 | **DONE · CI** | [#30](https://github.com/Kyziur/WhatToCook/pull/30) — Endpoint now uses `AsSplitQuery()` after repeated synthetic PostgreSQL 100×100×2 stress measurements; HTTP completeness and full CI passed ([run](https://github.com/Kyziur/WhatToCook/actions/runs/42481352433)). Production p95 remains unmeasured |
| ARC-01 | OPEN / CONDITIONAL | Conditional: split complex page only when changing affected flow |
| ARC-02 | **DONE · CI** | [#35](https://github.com/Kyziur/WhatToCook/pull/35) — Pure shopping aggregation and shared numeric semantics extracted, DB-free tests added; final-head [CI 42508132586](https://github.com/Kyziur/WhatToCook/actions/runs/42508132586) green (120 API, 53 component, 98 browser). Transactions and checklist reconciliation remain unchanged |
| ARC-03 | OPEN / CONDITIONAL | Optional: use-case extraction only where boundary improves |
| ARC-04 | **PARTIAL** | [#29](https://github.com/Kyziur/WhatToCook/pull/29) — Current trusted-LAN/Web/API/DB and identity ownership decision documented; actual homelab access matrix and pre-exposure user authorization decision still required |
| ARC-05 | OPEN / CONDITIONAL | Optional: API contract tests without central DTO rewrite |
| DOC-01 | **DONE · CI** | [#24](https://github.com/Kyziur/WhatToCook/pull/24) — Local developer start/test/troubleshooting quickstart |
| DOC-02 | OPEN / CONDITIONAL | Conditional: short architecture map if repeated review confusion |
| DOC-03 | OPEN / CONDITIONAL | Conditional: product status note only if owners need it |
| DOC-04 | **DONE · CI** | [#8](https://github.com/Kyziur/WhatToCook/pull/8) — Corrected product-specific documentation context |
| DOC-05 | OPEN / CONDITIONAL | Optional: preserve deployment note, dedupe only if necessary |
| DOC-06 | OPEN / CONDITIONAL | Ongoing practice: docs updated with behavior changes |
| AGENT-01 | **PARTIAL** | [#28](https://github.com/Kyziur/WhatToCook/pull/28) — Manual instructions now prioritize live source/freshness over older generated Repowise section; each task must still verify its live index status |
| AGENT-02 | OPEN / CONDITIONAL | Ongoing practice: right-size agent review and evidence |
| AGENT-03 | **DONE · CI** | [#36](https://github.com/Kyziur/WhatToCook/pull/36) — Five dead skill references and incorrect SDK10 VSTest/MTP guidance fixed with Node contract checks; final-head [CI 42508393599](https://github.com/Kyziur/WhatToCook/actions/runs/42508393599) green. No blanket deduplication of otherwise consistent harness instructions |
| AGENT-04 | **PARTIAL** | [#28](https://github.com/Kyziur/WhatToCook/pull/28) — Machine-specific Windows path removed and CI contract test passed; real MCP client smoke from second checkout/WSL still required |
| AGENT-05 | **DONE · CI** | [#8](https://github.com/Kyziur/WhatToCook/pull/8), [#28](https://github.com/Kyziur/WhatToCook/pull/28) — Aligned agent/OpenSpec product assumptions and corrected stale MAUI implementation example |
| AGENT-06 | **IN REVIEW** | Shared OpenSpec apply/archive safety invariants verified across OMP commands and `.agents` skills in a focused Node PR CI contract; harness-specific invocation syntax deliberately remains separate; final CI and review pending |
| AGENT-07 | OPEN / CONDITIONAL | Optional: add deployment-specific skill only for demonstrated gap |
| AGENT-08 | OPEN / CONDITIONAL | Optional: reduce skill selection cost when measurable |

### Immediate operational prerequisites (not proven by PR CI)

Before claiming WhatToCook production-ready, capture evidence from the **homelab** for `SEC-01`, `SEC-02`, `CI-04`, `OPS-02`, `OPS-03`, `OPS-04`, `OPS-06`, `OPS-07`, and `OBS-02`. These require actual effective access controls, deployed storage, backup/restore, volume identity, network reachability and database+image write/read. GitHub-hosted Testcontainers/ARM64 checks cannot substitute for the live environment.

### Maintenance rule

After every merged audit PR, update the corresponding row, keep its linked evidence, and distinguish **application implementation** from **operational verification**. Do not mark conditional observations fixed solely because documentation changed. Re-review old snapshot evidence against current code before implementing another item.

## Priority and ownership

- **P1:** address first. For rollout-gated items, the prerequisite is a verified operational outcome, not necessarily an application-code change.
- **P2:** worthwhile after P1 or when the stated trigger applies.
- **P3 / optional:** do alongside affected work or after a measurement demonstrates value. An optional item is not a commitment to implement it regardless of cost.
- **Verify first:** a plausible risk or behavior requiring an HTTP, browser, CI, or homelab check before choosing a fix.

The application repository owns application code, images, and the passive Compose contract. The homelab owns host preparation, Komodo/Periphery, secret injection, shared networks, routing, and backups. Preserve the existing BWS workload scope and approved wrapper; the application must not require `BWS_ACCESS_TOKEN`. See the [self-hosted deployment contract](../self-hosted-deployment.md).

## Originally recommended sequence (2026-11-06)

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

# PERF-03 — evidence and decision

## Verification

- Initial CI: [42483387937](https://github.com/Kyziur/WhatToCook/actions/runs/42483387937) completed PostgreSQL integration tests including PERF-03 (117 passed) and component tests (53 passed); Playwright and ARM64 also completed. The run failed **only** the CSharpier formatting gate (first review issue).
- PostgreSQL Testcontainers 17, 1,000 and 10,000 synthetic recipes. Baseline and disposable GIN states were compared on the same dataset. Count and deterministic page IDs matched, and HTTP search remained bounded and correct.
- `kurkuma` (1% selectivity) used sequential scans for count with and without GIN; the page used the pre-existing normalized title B-tree.
- `domowe` (75% selectivity) caused PostgreSQL to use bitmap GIN; count became slower in this run.
- `zu` (two characters) stayed on sequential/B-tree scans.
- Search SQL translation was explicitly verified to contain `LIKE`; the testing extension and index were never added to migrations.
- Timing numbers and planner choices are captured in [the performance note](../../../docs/performance/recipe-title-substring-search.md). The dataset is synthetic and warmed; no production latency guarantee is claimed.

## Decision

Do **not** deploy `pg_trgm` now. Revisit PERF-03 only after real catalog size/latency and query-plan evidence indicate a material slowdown. This closes the audit's investigation, not any future performance backlog triggered by real workload.

## Review

- Iteration 1: CSharpier required `CaptureAsync` to be on one line. Fixed in a separate commit.
- Final handoff depends on an entirely green PR CI run; GitHub check history provides its link.

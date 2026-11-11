# Performance decisions

**Purpose:** avoid re-running settled experiments or adding optimizations without a measurable problem. Results below come from synthetic CI fixtures, **not** production SLAs. Source code, tests and OpenSpec evidence remain authoritative.

| Area | Current decision | Revisit when |
| --- | --- | --- |
| Library and planner paging | The library uses a paged catalog; the planner fetches all pages rather than silently stopping at 500 recipes. | Planner latency or circuit memory becomes significant on real data; consider server-side picker search. |
| Title substring search | Keep the existing B-tree strategy; do **not** add `pg_trgm` solely from synthetic query plans. | Real catalog plans and p95 show a selective search bottleneck. |
| Recipe details | Use EF Core `AsSplitQuery()` for multiple collections. | Real data shows extra round trips outweigh reduced join multiplication. |
| Shopping recommendations | Filter candidates in SQL by possessed ingredients, then compute full scores **before** applying the result limit. | Candidate distribution or query latency changes. |
| Blazor circuits | Keep UI structure unless real render/circuit measurements justify a change. | Repeated representative load measurements identify a specific costly component. |
| Recipe images | Conditional ETag/304 is implemented; **no production thumbnail strategy** is committed by the synthetic tests. | Real photos and device measurements justify responsive variants with quality, deletion and storage checks. |

## Reproduce a specific question

With Docker running, use the named integration tests in `tests/WhatToCook.Api.IntegrationTests/Specs/`, e.g.:

```powershell
dotnet test tests/WhatToCook.Api.IntegrationTests/WhatToCook.Api.IntegrationTests.csproj --filter "FullyQualifiedName~RecipeDetailsQueryDiagnosticsTests"
```

This executes the PostgreSQL-backed recipe-detail query comparison, not a production benchmark. Other focused fixtures: `RecipeSubstringSearchDiagnosticsTests`, `RecommendationCandidateDiagnosticsTests`, and `RecipeDetailsQueryDiagnosticsTests`. Blazor circuit and image transfer probes live under `tests/e2e/` and `scripts/perf/`; opt in only when investigating the corresponding issue, and do not put real recipe photo data into public CI artifacts.

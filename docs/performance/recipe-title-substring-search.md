# PERF-03 — PostgreSQL substring-search query plans

## Question and production scope

The API uses `NormalizedTitle.Contains(normalizedPhrase)` in search and the paged recipe library. Npgsql translates `string.Contains` to `LIKE '%phrase%'` (escaping wildcards when needed), and the PostgreSQL `pg_trgm` GIN operator class can accelerate suitable `LIKE` predicates. This **does not** establish that an index is needed for a small household catalog.

The diagnostic compares the **existing** query semantics at 1,000 and 10,000 synthetic active recipes before and after a **test-database-only** `pg_trgm` GIN index. No migration, extension, endpoint, response shape or persistent index is introduced in this PR.

## Reproduce

Ensure Docker is running; execute from the repository root:

```powershell
dotnet test tests/WhatToCook.Api.IntegrationTests/WhatToCook.Api.IntegrationTests.csproj --filter "FullyQualifiedName~RecipeSubstringSearchDiagnosticsTests" --logger "console;verbosity=detailed"
```

This spins up PostgreSQL 17 through Testcontainers, runs migrations, loads synthetic data with `generate_series` (1k then 10k), analyzes the table, and compares indexed/non-indexed states on the **same catalog**. At 10k it tests a selective substring (`kurkuma`), a common substring (`domowe`), and a two-character substring (`zu`). The test reports generated EF SQL and `EXPLAIN (ANALYZE, BUFFERS, FORMAT JSON)` summaries for count and bounded page queries, including scan types, index names, elapsed time and buffer hits/reads.

**Correctness:** query count and stable ordered page IDs must agree with/without the test index, and the actual search HTTP endpoint still returns the expected bounded page. The test does **not** fail based on timings, whether a particular scan is chosen or whether GIN is faster.

## Measured plans (first CI run)

[GitHub Actions run 42483387937](https://github.com/Kyziur/WhatToCook/actions/runs/42483387937) executed the real EF query and the matching count/page `EXPLAIN (ANALYZE, BUFFERS)` probes. The application integration suite passed (117 tests) and 53 component tests passed. The first run was **not fully green** because CSharpier required one method signature to be collapsed; the formatting correction is in the next review commit.

| Catalog size | Phrase / matches | COUNT no GIN | COUNT with GIN | Planner's chosen scan |
| --- | --- | --- | --- | --- |
| 1,000 | `kurkuma` / 10 | 0.17 ms | 0.15 ms | Sequential in both cases |
| 10,000 | `kurkuma` / 100 | 2.84 ms | 1.35 ms | Sequential in both cases |
| 10,000 | `domowe` / 7,500 | 3.48 ms | 5.58 ms | Sequential without; bitmap GIN with |
| 10,000 | `zu` / 2,500 | 1.92 ms | 1.88 ms | Sequential in both cases |

At 10,000 recipes, the bounded 50-item `kurkuma` page used the **existing unique B-tree normalized-title index**, not the disposable GIN index (1.04 vs 1.00 ms). The 75%-selective `domowe` query did use the disposable GIN index, but its count became slower in this run. The two-character term did not benefit from GIN. All scans above had zero shared read blocks in this warm CI measurement, so these values must not be extrapolated to cold production I/O.

**Decision: do not add a production `pg_trgm` migration now.** The synthetic measurement does not demonstrate a stable indexing win at a likely household catalog size. Reopen the schema decision if actual query p95 or PostgreSQL plans from the deployed catalog identify a real bottleneck. No change to matching behavior is justified.

## Decision gate

The test intentionally allows small catalogs, frequent phrases, and short inputs to use sequential scans. PostgreSQL's `pg_trgm` needs extractable trigrams to filter efficiently; a short phrase can degrade to a full-index scan. Review repeated plans and sizes, plus realistic recipe counts and request p95 from the target homelab, before adding an index.

Only if index benefit is repeatable **and** the deployment catalog has a real latency issue, propose a separate migration with `pg_trgm` extension + GIN index and evaluate storage/write amplification, small-search behavior and rollback. Otherwise retain the existing unique B-tree normalized-title index and avoid speculative schema complexity.

References: [Npgsql string translations](https://www.npgsql.org/efcore/mapping/translations.html); [PostgreSQL 17 pg_trgm](https://www.postgresql.org/docs/17/pgtrgm.html).

## Evidence and limitations

CI logs carry the plans and query counts on a throwaway PostgreSQL container. An observed speedup on the CI runner is **not** production p95 or a universal speedup. The test's fixture creates empty ingredient/tag collections, so it focuses on the title predicate rather than claiming to represent entire recipe-fetch costs or production data distribution.

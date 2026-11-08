# PERF-07 — EF Core recipe details query-shape measurement

The endpoint `GET /api/recipes/{recipeId}` originally included tags, ingredients and steps in one no-tracking EF Core query. Multiple collection joins can multiply result rows; the original audit **did not measure** a production regression. The endpoint has since been switched to a split query after the synthetic measurements below.

## Reproducible PostgreSQL fixture

`RecipeDetailsQueryDiagnosticsTests` creates a recipe with 100 distinct ingredients, 100 ordered steps and 2 tags through the HTTP API. It checks the ordinary details response for completeness, and compares the same include shape in explicit `AsSingleQuery()` and `AsSplitQuery()` modes via the Testcontainers-backed PostgreSQL fixture. Both queries must return all the same collections.

Run from the repository root after starting Docker:

```shell
dotnet test tests/WhatToCook.Api.IntegrationTests/WhatToCook.Api.IntegrationTests.csproj --filter "FullyQualifiedName~RecipeDetailsQueryDiagnosticsTests" --logger "console;verbosity=detailed"
```

This runs the focused integration test and prints the two observed elapsed times. The repository CI also runs it with detailed output and **does not fail based on timing**. Its theoretical single-query cartesian upper bound is 100 × 100 × 2 = 20,000 joined rows for this fixture. That number is not measured bytes, latency, or a production result.

## How to interpret further measurements

The comparison exercises the same logical include graph, not production workload. For additional optimization, capture representative SQL plus `EXPLAIN (ANALYZE, BUFFERS)` on realistic data. Compare query count, rows/bytes transferred, allocations and p50/p95 over several runs. A split query reduces collection-row multiplication but adds round trips and consistency considerations; a projection may help small responses. Do not use these synthetic elapsed times to promise a production SLA.

## Targeted query change

The initial GitHub PR run [42479513091](https://github.com/Kyziur/WhatToCook/actions/runs/42479513091) measured warmed synthetic queries at **301.2 ms** for `AsSingleQuery()` and **9.5 ms** for `AsSplitQuery()`. A second run [42480178557](https://github.com/Kyziur/WhatToCook/actions/runs/42480178557) reported **211.5 ms** and **5.2 ms** for the same 100-ingredient/100-step/2-tag fixture. Both runs reproduced a large difference, but these are shared-runner diagnostics, **not** production p95 or a promised speedup. The original endpoint was still using the single-query include shape in the first run.

Because this fixture is at the documented maximum collection size and the join upper bound is about 20,000 rows, the endpoint now explicitly uses `AsSplitQuery()`. The HTTP regression tests ensure all 100 ingredients and steps remain in the response, and the diagnostic comparison keeps both variants visible. Split mode can perform more DB round trips and may observe non-atomic child collection changes if concurrent writers modify the recipe; the existing API never promised a snapshot across multiple statements. Keep the single-query baseline diagnostic for future decisions.

**Status:** targeted worst-case query-shape improvement implemented and covered by HTTP correctness tests plus a non-gating query comparison. Merge readiness depends on the final PR CI result. The timings establish only this synthetic fixture; further production-like SQL `EXPLAIN`, allocations and load measurements remain useful when performance matters.

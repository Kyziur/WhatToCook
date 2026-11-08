# PERF-07 — EF Core recipe details query-shape measurement

The endpoint `GET /api/recipes/{recipeId}` currently includes tags, ingredients and steps in one no-tracking EF Core query. Multiple collection joins can multiply result rows; the original audit **did not measure** a production regression.

## Reproducible PostgreSQL fixture

`RecipeDetailsQueryDiagnosticsTests` creates a recipe with 100 distinct ingredients, 100 ordered steps and 2 tags through the HTTP API. It checks the ordinary details response for completeness, and compares the same include shape in explicit `AsSingleQuery()` and `AsSplitQuery()` modes via the Testcontainers-backed PostgreSQL fixture. Both queries must return all the same collections.

Run from the repository root after starting Docker:

```shell
dotnet test tests/WhatToCook.Api.IntegrationTests/WhatToCook.Api.IntegrationTests.csproj --filter "FullyQualifiedName~RecipeDetailsQueryDiagnosticsTests" --logger "console;verbosity=detailed"
```

This runs the focused integration test and prints the two observed elapsed times. The repository CI also runs it with detailed output and **does not fail based on timing**. Its theoretical single-query cartesian upper bound is 100 × 100 × 2 = 20,000 joined rows for this fixture. That number is not measured bytes, latency, or a production result.

## Decision after the first measurement

Review CI output and, if latency is material, capture representative SQL plus `EXPLAIN (ANALYZE, BUFFERS)` on expected production-like data. Compare *query count*, rows/bytes transferred, allocations and p50/p95, ideally with more than one recipe size and repeated runs. A split query may reduce row multiplication but adds round trips and consistency considerations; a projection can also reduce columns. Do not switch to `AsSplitQuery()` based solely on this synthetic fixture or a single CI time.

**Status:** measurement harness established. No endpoint optimization claimed until evidence supports a specific change; baseline results are not a performance gate.

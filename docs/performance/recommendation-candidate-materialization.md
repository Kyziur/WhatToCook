# PERF-04 — shortlist recommendation candidates before materialization

## Existing behavior

`ShoppingListService.GetRecipeRecommendationsAsync` reads ingredients for every active recipe not already in a meal plan, even though recipes sharing **no** possessed ingredients can never be recommended. It then distincts ingredient names per recipe, checks coverage (at least 30%), ranks by matched ingredient count, coverage, and case-insensitive title, and **only then** limits results to 1–20.

This is an observed query shape, not measured evidence of a real production incident.

## Controlled PostgreSQL diagnostic

`RecommendationCandidateDiagnosticsTests` builds a separate PostgreSQL 17 Testcontainers catalog at **100, 1,000 and 10,000 recipes**, with three ingredient links per recipe and 5% shared-ingredient candidates. It compares the existing unrestricted recipe query to a server-side `EXISTS` predicate retaining recipes with at least one owned normalized ingredient. The baseline includes a deliberately inactive matching recipe and duplicate normalized ingredient names, so eligibility and coverage matter.

The test reports **materialized parent rows and ingredient links**, wall-clock milliseconds, and process-wide allocated bytes (approximate/noisy on a shared CI runner). It checks identical eligible candidate IDs, equivalent ranked top 20, and actual HTTP recommendations. It never asserts a hard time/byte threshold.

Run from the repository root with Docker available:

```sh
dotnet test tests/WhatToCook.Api.IntegrationTests/WhatToCook.Api.IntegrationTests.csproj --filter "FullyQualifiedName~RecommendationCandidateDiagnosticsTests" --logger "console;verbosity=detailed"
```

The dedicated CI diagnostic runs after the ordinary .NET test suite and prints each catalog size's numbers. Its 10k fixture is deliberately sparse, not evidence of current household catalog size or end-user latency.

## Decision guardrails

- SQL may discard recipes with **no** matching ingredient before loading their details, because the existing scorer excludes them anyway.
- Do **not** add `Take(limit)` or another arbitrary row limit **before** scoring; that can silently remove the best-scoring recipe.
- Preserve the 30% threshold, distinct-by-normalized-name semantics, planned/inactive exclusions, and ranking (matched count DESC, coverage DESC, title ordinal ignore-case ASC).
- If the query optimization is adopted, preserve exact response content in HTTP tests and document query/round-trip behavior. Large owned-ingredient sets and a catalog where nearly everything matches may reduce gains.

## Observed baseline and targeted change

The first green [CI run 42490440131](https://github.com/Kyziur/WhatToCook/actions/runs/42490440131) reported:

| Synthetic recipes | Loaded recipes / ingredient links, old | SQL-filtered | Old time | Filtered time | Old / filtered allocation |
| --- | --- | --- | --- | --- | --- |
| 100 | 99 / 297 | 4 / 12 | 167.01 ms | 56.56 ms | 2.22 / 1.64 MB |
| 1,000 | 999 / 2,997 | 49 / 147 | 13.93 ms | 2.21 ms | 2.01 / 0.13 MB |
| 10,000 | 9,999 / 29,997 | 499 / 1,497 | 109.75 ms | 9.14 ms | 19.91 / 1.02 MB |

The 100-row timings include query-startup overhead and should not be compared to the warmed larger-catalog timings. Process-wide allocation counters can include other thread activity. This is one shared-runner measurement with 5% matching candidates and does **not** imply a universal speedup.

**Decision:** apply the safe SQL candidate predicate to `ShoppingListService.GetRecipeRecommendationsAsync`. The predicate is only `EXISTS` with a possessed normalized ingredient name; it cannot discard a recipe that the existing scorer could accept, because that scorer requires at least one match. It preserves all ingredients for every retained recipe, the existing 30% coverage computation, full ranking and the post-ranking `Take(limit)` cutoff. Planned and inactive exclusions remain unchanged.

**Boundary:** this optimization does not reduce work when most recipes share a possessed ingredient and does not push coverage/rank to SQL. Production p50/p95 latency and PostgreSQL transfer size remain unmeasured. Avoid interpreting the synthetic allocation numbers as network bytes.

## Final CI verification

The optimized production service passed [GitHub Actions run 42491096130](https://github.com/Kyziur/WhatToCook/actions/runs/42491096130), including PostgreSQL integration tests, ranking regression, the diagnostic benchmark, Playwright, ARM64, dependency policy and CSharpier. This verifies application behavior on test fixtures, **not** production latency.

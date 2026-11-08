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

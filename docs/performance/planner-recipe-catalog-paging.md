# PERF-01 follow-up — Planner recipe catalog paging

The Blazor recipe **library** already uses `GET /api/recipes/page` (PERF-01). The meal **planner** continued to call legacy `GET /api/recipes`, silently capped at 500 results. The planner now traverses 50-summary pages until the complete reported `TotalCount` is collected.

To prevent each successive page from triggering expensive whole-catalog facet queries, the existing page API adds an **optional** `includeFacets=false` query parameter. Its default is `true`, so the library continues to receive tag usage and ingredient facets. On opt-out the page still returns the same JSON contract with `tags: []`, `ingredients: []`, `tagUsage: {}`.

The planner maintains its existing in-memory recipe picker, title sorting and selection logic. This is a correctness and bounded-response fix, **not** a measured speedup; multiple requests replace one incomplete request. At household scale this avoids unseen recipes. If catalog growth leads to high planner load time or Blazor circuit memory, profile those numbers and move the **picker** to server-side search/pagination rather than materializing all recipes at once.

Verification: `ApiRecipeCatalogServiceTests` models 551 active recipes and an incomplete second page; API HTTP integration asserts the facets contract and unchanged default. Final CI and host-specific measurements are tracked separately.

## Verification on main

[PR #37](https://github.com/Kyziur/WhatToCook/pull/37) was squash-merged after [final-head CI 42513645703](https://github.com/Kyziur/WhatToCook/actions/runs/42513645703) passed: 121 API, 56 component, 98 Playwright browser tests and ARM64 image smoke. The implementation guards against both empty and short nonempty page results before the declared total. This is application correctness evidence, **not** a benchmark of planner memory under large real household catalogs.

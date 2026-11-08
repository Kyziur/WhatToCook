# PERF-01 planner follow-up evidence — 2026-11-08

- [PR #37](https://github.com/Kyziur/WhatToCook/pull/37) — merged to main as `010e2c66f137673426baf562ff3d8784256f569c`.
- [Final-head CI 42513645703](https://github.com/Kyziur/WhatToCook/actions/runs/42513645703): **success** for `verify` and ARM64 jobs.
- `dotnet csharpier check .`, Release build, **121 API integration tests**, **56 Web component tests**, **98 Playwright E2E tests** and existing CI contracts passed.
- Added tests demonstrate 551 recipes across 12 pages, verify opt-out facets/default full facets, and fail visibly if a later page is empty **or short but nonempty** before the declared total.
- CI formatter failures were corrected in separate commits; the author-side review found the short nonempty-page case and added its regression.

## Preserved boundaries

The legacy `GET /api/recipes` endpoint is retained for existing consumers, but the planner now uses bounded paged `GET /api/recipes/page`. The optional `includeFacets=false` flag reduces repeated facet queries without changing default API behavior. No schema change, UI redesign or claim of measured throughput improvement.

## Outstanding

Independent PR review was not recorded. Planner continues to keep all recipe summaries in its server-side circuit; large-catalog load time/memory require measurement before migrating the picker to server-driven search.

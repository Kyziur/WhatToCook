# PERF-01 follow-up — Planner catalog beyond 500 recipes

## Why

PERF-01 made the library and search paged, but the independently maintained planner still calls `ApiRecipeCatalogService.GetActiveRecipesAsync`, which uses the **legacy** `GET /api/recipes` endpoint capped at 500. A household with 501+ active recipes may be unable to select later recipes in the weekly plan.

## What changes

- Traverse stable bounded `GET /api/recipes/page` results (50 items/request) for the planner's existing in-memory picker. Preserve its existing title ordering and selection behavior.
- Add optional `includeFacets=false` to the library page API to avoid repeating full-catalog tag/ingredient facet queries for each planner page. The default remains **true** for existing library and API consumers; response JSON shape stays unchanged with empty facet collections on opt-out.
- Add focused Web HTTP-client tests for 551 results and truncated/incomplete server responses; add HTTP API test for opt-out and backward-compatible default facets.

## Non-goals and trade-offs

No removal of legacy `/api/recipes`, no unbounded single API response, no planner picker UI refactor, no schema/index changes, and no new service abstractions. The planner still materializes all active recipe summaries in one circuit; if large-catalog memory or load latency becomes measurable, a separate, server-filtered recipe picker is warranted rather than another larger cap.

## Verification

Run full CI after formatter check. Test the actual API facet semantics at the HTTP boundary and the Web consumer's complete page traversal, including an explicit failure if a page unexpectedly ends early.

## Why

PERF-01: the current library silently caps its unfiltered result at 500 and filters only client-side; the search endpoint materializes an unbounded list. Users cannot find later catalog entries and resource cost grows with the catalog.

## What Changes

- Add stable, bounded SQL pagination and full-catalog filtering for the library, including tag ALL and ingredient ALL/ANY semantics.
- Bound legacy search responses while allowing page traversal; preserve list JSON shape for existing consumers.
- Return whole-catalog facet values and counts independently from the displayed page.
- Update Blazor library with server-backed page navigation and filters (included in this change).

## Non-Goals

No new search engine, no unrelated API contract removal and no speculative indexes without query-plan evidence.

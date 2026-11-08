# ARC-02 verification and review evidence — 2026-11-08

- [PR #35](https://github.com/Kyziur/WhatToCook/pull/35), verified implementation SHA `377b4acfe489c745b98e51322f9492a4b3a8b2b3`.
- [PR CI run 42507447247](https://github.com/Kyziur/WhatToCook/actions/runs/42507447247): **success** for `verify` and ARM64 upload-smoke jobs.
- Formatter `dotnet csharpier check .` passed after applying CI-derived exact formatting diff. xUnit analyzer issue xUnit2031 fixed using the `Assert.Single(collection, predicate)` overload.
- API test project **120 passed** (including two new in-memory `ShoppingListAggregationTests`); Web component tests **53 passed**; full desktop/mobile Playwright smoke **98 passed** (4 intentionally skipped).
- Existing HTTP shopping-list regeneration and checklist-state behavior tests exercised; no new migrations or API endpoint changes.

## Boundary review

`ShoppingListAggregation` builds transient `ShoppingListItem` values without DB access. Common numeric parsing, normalized units and numeric formatting are reused via `ShoppingQuantity`. `ShoppingListService` still owns EF Core queries, advisory locks, transactions, stable row matching, state retention and manual-item persistence.

The extraction moves existing calculation with no intended behavior change; numeric, descriptive, name-only and distinct-unit cases are covered by new focused tests. The test project still includes Testcontainers-based HTTP scenarios, so full CI proves DB integration, while the new tests do not acquire a database fixture.

## Outstanding

The above is author-side verification, **not** an independent PR review. Final-head documentation-only CI must pass, and actual merge/audit reconciliation must be handled explicitly. No production performance or homelab readiness claim follows from this refactor.

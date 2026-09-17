## Why

The recipe library currently supports title, tag, and ingredient discovery but has no defined metadata contract for filters such as preparation time. The foundation explicitly deferred this behavior, so the filter semantics should be designed before any data-model or UI implementation is started.

## What Changes

- Define which recipe metadata fields are eligible for filtering and how missing values behave.
- Define normalized query, range, URL-state, result-count, and clear-filter behavior for metadata filters.
- Record the implementation and migration scope for a future recipe-library package.
- Add scenario-level test-traceability placeholders for the not-yet-implemented behavior.
- No application, persistence, API, or UI implementation in this design-only change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `recipe-management-core`: define the deferred metadata-filter requirements for the recipe library.

## Impact

This design-only change affects the future recipe-library contract in a reusable application template. It does not change runtime code, database schema, API routes, or current user-visible behavior.

## Non-Goals

- Implementing metadata fields, filtering, or new recipe editor controls.
- Choosing an external search engine or introducing a new query service.
- Changing existing title, tag, or ingredient filtering behavior.
- Retrofitting historical recipes before an implementation package defines the migration strategy.

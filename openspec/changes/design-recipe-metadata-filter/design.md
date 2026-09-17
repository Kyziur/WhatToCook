## Context

See `proposal.md` for the motivation. The current recipe contract supports title, tag, and ingredient discovery but has no persisted preparation-time metadata. The foundation deliberately deferred metadata filtering, so this change defines the contract without changing the current application.

The future implementation must fit the existing recipe library, recipe editor, API query model, and URL-backed filter state. It must also remain compatible with records that do not have the new metadata populated.

## Goals / Non-Goals

**Goals:**

- Define a small first metadata-filter slice that can be implemented without introducing a separate search subsystem.
- Specify the value, validation, missing-value, query, URL-state, and clear-filter semantics.
- Keep the contract compatible with existing title, tag, and ingredient filtering.
- Define the test and migration boundaries for a later implementation change.

**Non-Goals:**

- No runtime, API, persistence, or UI implementation in this change.
- No difficulty, cost, nutrition, category, or free-form metadata taxonomy.
- No external search engine or client-side filtering of the full catalog.
- No automatic backfill of historical recipes without an implementation-specific migration decision.

## Decisions

### Use optional preparation time as the first metadata field

The first supported metadata field will be an optional strictly positive integer (> 0) representing preparation time in minutes. A nullable value preserves recipes for which the user does not know or want to record preparation time.

A broader metadata registry was rejected because it would create an abstraction before the product has a second stable metadata use case.

### Use inclusive minimum and maximum bounds

The future filter contract will support an optional minimum and maximum preparation-time bound. A recipe matches when its value is present and falls within both supplied inclusive bounds. When no metadata filter is active, recipes with and without preparation time remain visible under the existing title, tag, and ingredient rules.

Recipes with a missing preparation time do not match an active preparation-time filter; the first implementation does not add a separate “unknown” bucket.

### Keep the filter URL-backed and server-evaluated

The selected bounds will be represented in the library query and URL state so links can be refreshed and shared. The API remains authoritative for filtering and pagination; the UI only presents the selected values and result summary.

Invalid values, zero or negative minutes, or a minimum greater than the maximum produce a user-visible validation error and do not silently change the active filter.

### Treat implementation as a separate follow-up change

This change remains design-only. A future implementation change must update the recipe editor, persistence model, API contracts, library UI, migration strategy, tests, and traceability before this contract is synced into the canonical spec.

## Risks / Trade-offs

- Optional preparation time makes filtering precise only for completed metadata; the UI must make missing values understandable instead of implying that no match exists.
- Adding a persisted field requires a migration and compatibility with existing recipes; the implementation change must choose a safe nullable default.
- URL-backed numeric bounds require consistent parsing and formatting across browser, API, and component tests.

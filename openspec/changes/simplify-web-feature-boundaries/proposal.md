# Simplify Web feature boundaries without changing behavior

## Why
The editor and import review repeat the same recipe unit list, while the planner uses a service locator despite a required DI registration. Library pagination also left unused client-side predicates.

## Changes
- Reuse one feature-local unit catalog in the recipe editor and imported-draft review.
- Inject the planner shopping service directly, dropping unreachable optional-service fallbacks.
- Remove unused recipe-library client match helpers after server pagination.

## Non-goals
No generic form framework, state store, backend, data migration, CSS or deployment change. Preserve rendered options, API contracts and planner behavior. This alone does not complete ARC-01 or UX-01.

## Verification
Existing bUnit editor/import/planner tests and Playwright browser regression; full CI and independent review before merge.

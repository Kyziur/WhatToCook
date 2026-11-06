## Why

The existing shopping flow is useful for generating ingredient needs, but it does not model the act of **buying** an item, categorize items for a store trip, or guarantee safe shared use on multiple phones. Users need a trustworthy list they can use while physically shopping.

## What Changes

- Add a separate **Bought** state to the current `MAM` / `NIE MAM` model, with undo, persistence, and shopping progress.
- Add a focused mobile shopping view, store departments, and editable categories for generated and manual items.
- Define shared-household synchronization and conflict handling, including authorization requirements.
- Preserve manual items and correctly handle quantity changes when a shopping list is regenerated.
- Define a separately gated offline experience; explicitly avoid assuming Blazor Interactive Server works offline.
- Record detailed user stories and pending test-traceability for future implementation.

## Capabilities

### New Capabilities
None.

### Modified Capabilities
- `shopping-list-management`: proposed shopping-trip, category, collaboration, and optional offline requirements.

## Impact

Future work may affect shopping domain and persistence, API item state/merge operations, Blazor shopping screens, identity and synchronization, deployment access, and the relevant PostgreSQL integration and browser tests.

## Non-Goals

- No application code, schema migrations, runtime behavior, or tests are implemented by this change.
- No full inventory tracking, store pricing, receipts, barcode scanning, or automated order placement.
- No implicit internet exposure or unauthenticated sharing of household lists.
- No declaration of offline support until a feasible architecture and release decision are approved.

## Product outcome

A household can use a single accurate list during a shopping trip, see what remains, and avoid repeated purchases. See [user-stories.md](user-stories.md) for full user narratives and acceptance criteria.

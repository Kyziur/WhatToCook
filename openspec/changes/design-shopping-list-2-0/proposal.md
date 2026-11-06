## Why

The existing shopping list shows what is at home or still needed, but not what is already bought in the store. It lacks mobile-first purchase progress, useful product grouping and automatic updates between connected devices.

## What Changes

- Add a persisted Bought indicator distinct from `MAM`/`NIE MAM`, undo and `Finish shopping` to make purchased items available to recommendations.
- Add mobile views with accurate progress, fixed store departments and editing of manual products on existing plan-linked lists.
- Add server-authoritative automatic syncing for one connected, trusted household deployment, using per-item version checks to reject stale edits.
- Reconcile list regeneration atomically, resetting `MAM`/Bought when a matched item's amount changes.
- Record offline shopping only as a **separate potential future change**, not a normative requirement here.

## Capabilities

### New Capabilities
None.

### Modified Capabilities
- `shopping-list-management`: buying lifecycle, connected shopping usability, categories, safe synchronization and regeneration.

## Impact

Later changes touch shopping item persistence/versioning, API, recommendation semantics, list regeneration, mobile Blazor components, PostgreSQL and multi-device tests. Access outside LAN requires authenticated private network connectivity, never public exposure of the unauthenticated API.

## Non-Goals

- **No implementation yet**, schema changes or test execution in this design-only change.
- No offline mode, custom category administration, shopping history or standalone lists.
- No user accounts, multiple households or unauthenticated public access.

## Product outcome

Two connected household devices can coordinate a real shopping trip without lost progress or duplicate purchases. Detailed acceptance criteria live in [user-stories.md](user-stories.md).

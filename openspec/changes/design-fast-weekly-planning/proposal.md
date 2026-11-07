## Why

Recreating next week's meal plan one day at a time is unnecessary work. The current planner already supports multiple meals per slot, ordering and multipliers, but does not offer a reliable copy-and-adjust workflow.

## What Changes

- Copy any previous plan to new dates as an independent plan, with preview, explicit resolution of archived recipes and duplicate-request protection.
- Replace, move/append and explicitly swap individual recipe entries, preserving the existing **multiple-recipes-per-slot** model and multipliers.
- Represent no-cooking and leftovers without fake recipes, with safe ingredient and source-reference semantics.
- Preserve shopping-list regeneration choice and atomic persistence during edits.
- Keep reusable saved combinations as a **deferred idea**, not an unconditional requirement in this delta.

## Capabilities

### New Capabilities
None.

### Modified Capabilities
- `meal-planning`: plan reuse, explicit entry movement, leftovers, no-cooking and save invariants.

## Impact

Later work touches planner models/API, `SortOrder`/slot handling, copy idempotency, Blazor planner UI, shopping regeneration and PostgreSQL/E2E tests.

## Non-Goals

- **No implementation yet** or runtime/migration/test changes in this PR.
- No AI-generated plan, external calendar, pantry deductions or recipe replacement by implicit occupied-slot overwrite.
- No WP-07 saved combinations in this change.

## Product outcome

A planner can copy and adjust a familiar week in a few focused actions instead of building it again. See [user-stories.md](user-stories.md).

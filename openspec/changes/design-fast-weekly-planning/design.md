## Context

Meal plans are separate named `DateOnly` ranges, can overlap, and have multiple planned recipes per day. Each planned recipe stores `PlannedDate`, `Multiplier` and `SortOrder`; the Blazor planner **encodes meal slot and intra-slot position in SortOrder**. A slot may contain multiple recipes. A recipe may occur on different dates but not twice on the same date. Inactive recipes remain readable in historical plans but cannot be added to new ones.

## Goals / Non-Goals

**Goals:** copy an existing week safely, preview before confirmation, quick replacement/move/explicit swap, real no-cooking and leftover entries, safe regeneration.

**Non-Goals:** no implementation yet, no redesign to single-entry slots, no generative meal plans, no automatic leftover portion forecasting, no saved combination templates (WP-07 deferred).

## Decisions

### Copy produces an independent target plan

Copy preserves relative `DateOnly` offsets, encoded meal slots/positions, positive multipliers and all supported entry types, but not the source shopping list or its `MAM`/Bought status. When No cooking / Leftovers entries are implemented, new target entries receive fresh IDs, and each leftover reference must be remapped to the corresponding copied cooked entry within the target. If archive resolution omits or replaces a cooked source, affected leftovers must be explicitly rebound or removed before copy confirmation; original-plan IDs must never leak into target references. The new plan defaults to the source's duration; source remains unchanged. Archived/inactive recipe entries appear in preview as unresolved. The user must replace or omit each archived recipe before committing a valid new plan. A new user command may create another target, but retrying the same accepted command (via stable operation identity/idempotency key) may not create duplicates.

### Multiple entries per slot remain valid

Moving an entry to a nonempty slot **appends at a valid position**; it never overwrites an existing meal. Swapping requires selection of **two specific entries** and exchanges their complete placements (date, slot and within-slot position), retaining each entry's multiplier and avoiding same-day recipe duplicates. Other entries retain their relative ordering, while slot sort indices may be normalized. Replacing one specific entry preserves its slot, relative position and multiplier by default. UI must be keyboard/touch accessible and must not require drag-and-drop.

### Leftovers and no-cooking need genuine plan-entry semantics

Current storage only models `PlannedRecipe`. Future implementation will introduce distinguishable plan entries/annotations without fake recipe IDs. `No cooking` is exclusive in a date/slot, so an occupied slot must be explicitly cleared before applying it. A leftover references a **stable cooked entry ID**, from an earlier date or strictly earlier meal slot on that date. It adds no ingredients; only the source recipe's multiplier generates shopping requirements. Missing, later or non-cooking sources block save until resolved; no guarantee is made that enough portions exist.

### Keep existing regeneration and concurrency invariants

New plans require explicit shopping generation and never inherit source purchase states. Editing an already generated plan preserves regenerate-or-cancel and transactional list updates. The shopping-list-management canonical contract is authoritative for item state reconciliation: if the Shopping List 2.0 delta is implemented, quantity changes reset `MAM`/Bought as defined there, rather than unconditionally preserving checked state. Concurrent plan saves need a version/conditional update rather than a silent last-write-wins overwrite.

## Risks and trade-offs

- `SortOrder` encoding must survive copies and moves; normalize ordering per slot rather than assuming one recipe per slot.
- Copying archived recipes contradicts canonical rules unless resolved before new plan persistence.
- A retry after timeout must not create another plan; a separate intentional copy must remain possible.
- Leftover references can break after moves/deletes; perform server-side validation.
- Versioning and changed-quantity shopping reconciliation must align with Shopping List 2.0 if both features are implemented.

## Future delivery sequence

1. Confirm copy + archived preview + idempotency contract.
2. Implement copy/preview, replace, move/append, explicit pairwise swap with PostgreSQL and mobile tests.
3. Add no-cooking/leftover entry model, validation and shopping calculations.
4. Verify regeneration, concurrency and existing invariants, then sync this delta when complete.

## Deferred product opportunity

WP-07 saved combinations remain in `user-stories.md` as **non-normative** backlog only. If prioritized, create a separate design-only OpenSpec change.

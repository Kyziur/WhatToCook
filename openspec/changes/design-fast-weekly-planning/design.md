## Context

WhatToCook can already create overlapping named date-range plans, assign recipes to days, preserve entry order and positive decimal multipliers, and generate shopping lists explicitly. The same recipe may be used on different days but not twice on one day. Archived recipes remain in old plans but are excluded from new selection.

## Goals / Non-Goals

**Goals**
- Make repeat planning fast without weakening current plan invariants.
- Keep copied plans independent and shopping progress fresh.
- Provide small edits through accessible controls rather than drag-and-drop as the only input path.
- Define clear semantics for leftover and no-cooking slots and their impact on ingredients.
- Prevent silent shopping-list regeneration.

**Non-Goals**
- No implementation now or new nutrition, calendar or inventory subsystem.
- Do not impose saved-combination functionality as a blocker for the first release.

## Decisions for this proposal

### 1. Copy as new plan with relative date offsets
A copy creates a new plan and does not modify its source. It preserves slot, ordering, recipe references, and multipliers. Default destination duration equals the source duration; arbitrary duration changes are deferred until truncation semantics are explicitly agreed. Mapping should use `DateOnly` calendar arithmetic to avoid timezone errors.

### 2. Shopping states are not copied
Shopping-list generation remains explicit. A new plan must not inherit old checked or bought states, even if recipes are identical. Existing regeneration rules still apply to later edits.

### 3. Accessible move/replace first; drag-and-drop optional
Use focused pickers and explicit move/swap actions, with source-state retention on cancel. Do not rely on a complex drag-and-drop calendar for primary mobile interaction. All writes must be server-validated.

### 4. Non-recipe planner entries as a real domain concept
`Eating out` and `Leftovers` are plan entries with distinct semantics, not fake `Recipe` entities. Leftovers must reference a valid earlier cooked entry; they contribute no extra ingredient requirement. The original cooked meal multiplier is the only direct quantity signal in MVP.

### 5. Handle concurrent plan editing deliberately
If multiple household members can edit a plan, use versioning or conditional updates so the final save does not silently erase another person's work.

## Risks / Trade-offs

- Relative date mapping may break around month boundaries: test `DateOnly` offsets and leap days.
- Invalid leftovers after moves or deletion: maintain referential validation with helpful remediation.
- Shopping-list changes during copy or edit: preserve atomic regeneration, explicitly gated by the planner.
- Larger planner UI complexity: prioritize a small mobile-first workflow rather than a complete calendar redesign.
- Saved reusable groups may become a second plan model: defer until after actual reuse behavior is observed.

## Future delivery sequence

1. Approve copy semantics, date mapping, meal-slot preservation and constraints.
2. Implement copy, preview, and focused replacement/move/swap in one end-to-end vertical slice with Testcontainers and UI tests.
3. Introduce explicit non-recipe entry semantics and shopping-list calculation.
4. Verify regeneration, rollback/failure cases, concurrency, and mobile accessibility.
5. Implement saved combinations only after a separate prioritization decision.

## Open Questions

- Should copy default to the latest plan by calendar date or last edited plan?
- Should "leftovers" reference a specific earlier entry and serving intention, or only mark a slot as leftovers?
- When a move hits an occupied destination, is swapping the default offered action or should it always require an extra confirmation?
- Are saved meal combinations needed in MVP, or only later?

## Context

See `proposal.md` for the direct-assignment motivation. The application already has meal-plan summaries, full plan save semantics, and recipe library and details routes. The direct assignment interaction needs to reuse those contracts instead of introducing a second persistence path.

The implementation was intentionally transferred to `improve-core-ui-ux`, which owns the shared picker, responsive presentation, duplicate handling, generated-shopping-list behavior, and verification. This change remains the design record for that capability boundary.

## Goals / Non-Goals

**Goals:**

- Define a focused direct-assignment interaction from recipe library and details contexts.
- Preserve existing meal-plan validation, one-recipe-per-day uniqueness, multiplier, and regeneration behavior.
- Keep the design compatible with the current API and persistence model.
- Record the implementation ownership transfer and its verification evidence.

**Non-Goals:**

- No implementation changes in this change.
- No new direct-assignment API endpoint or database migration.
- No replacement of the planner's existing save contract.
- No automatic shopping-list regeneration that bypasses the existing user decision.

## Decisions

### Reuse the existing meal-plan save contract

The picker adds an entry to a short-lived plan projection and saves through the existing meal-plan service. This keeps duplicate validation, multiplier defaults, and shopping-list regeneration semantics in one place.

A separate single-entry endpoint was rejected because it would duplicate plan invariants and create a second write path for the same aggregate.

### Use one shared picker for library and details

Recipe library cards and recipe details invoke the same picker component. The caller supplies the recipe context; the picker loads eligible plans and days, preserves the selected context, and reports success or validation failure without forcing planner-first navigation.

### Keep duplicate prevention at the selected-day boundary

The picker checks the selected day before saving and presents a corrective message when the recipe is already assigned there. The existing backend constraint remains authoritative for concurrent or stale clients.

### Transfer implementation ownership to `improve-core-ui-ux`

The implementation and verification are owned by `improve-core-ui-ux` tasks 4.1-4.5. This change contributes the design contract and is archived as superseded design history after the implementation owner's evidence is complete.

## Risks / Trade-offs

- Reusing full-plan save can expose the picker to concurrent edits; reload the selected plan immediately before saving and surface conflicts without losing the browsing context.
- A shared picker must support both library and details callers; keep caller-specific styling outside the picker and preserve a single behavior contract.
- Generated shopping lists require an explicit regeneration decision when the assignment changes a saved plan; reuse the existing decision rather than adding a picker-specific confirmation flow.

## Context

The current planner renders one editable day beneath several setup cards. The supplied references use the same underlying concepts but make the week, meal rhythm, progress, recipes, and shopping state visible together. Current persistence stores date, multiplier, and sort order but not a meal-slot field.

## Goals / Non-Goals

**Goals:**

- Match the reference hierarchy and density on desktop and mobile while retaining the current palette and typography.
- Preserve all current plan creation, editing, regeneration, duplicate prevention, direct assignment, shopping, and recipe-detail behavior.
- Avoid a database migration by encoding slot and within-slot order in the existing non-negative `SortOrder`.
- Keep phone screens focused, touch-friendly, safe-area-aware, and free from horizontal overflow.

**Non-Goals:**

- No month planner, drag-and-drop, authentication/avatar, product catalog, favorite recipes, recipe ratings, preparation-time metadata, or invented shopping categories.
- No fake metrics or controls unsupported by current application data.
- No redesign of the established warm color palette.

## Decisions

### Encode meal slots in sort-order ranges

Breakfast uses `1000-1999`, second breakfast `2000-2999`, dinner `3000-3999`, and supper `4000-4999`. Relative order remains the offset inside the range. Legacy entries with sort order below `1000` are interpreted as dinner until the next save normalizes them. A single helper owns encoding and decoding to avoid duplicated magic numbers.

### Render one semantic model in two responsive projections

Desktop renders seven day columns crossed with four meal rows. Mobile renders the same slot data for one selected day. CSS controls projection visibility, while shared methods supply entries and actions so behavior cannot diverge.

### Use a contextual picker rather than repeated controls

The current search/select controls move into a conditional picker associated with `SelectedDay` and `SelectedSlot`. It is an inline side drawer on desktop and a fixed bottom sheet on mobile. Existing recipe data and duplicate checks are reused.

### Reuse the shopping service in the planner

The planner loads the selected plan's generated shopping list after plan selection and after generation/regeneration. The embedded panel is a read-focused summary with state toggles delegated to the existing service; complex list management remains on `/shopping`.

### Preserve global navigation

The current desktop header and mobile bottom navigation remain the global shell. Planner-specific period controls and actions live inside the route workspace rather than creating a second application shell.

## Risks / Trade-offs

- [Risk] Sort-order encoding is implicit persistence. -> Mitigation: one `MealSlotOrdering` helper, explicit tests, and normalization on save.
- [Risk] Seven desktop columns become cramped below wide breakpoints. -> Mitigation: switch to selected-day projection below `1100px`.
- [Risk] Embedded shopping data adds an extra request. -> Mitigation: load only for the selected plan and keep the panel read-focused.
- [Risk] Existing tests target repeated day editors. -> Mitigation: preserve day and add-target data attributes and update interaction helpers after UI wiring is complete.

## Migration Plan

1. Add static workspace markup and responsive CSS around current state.
2. Add slot ordering and contextual picker behavior.
3. Integrate shopping summary and aligned standalone screens.
4. Verify visually through agent-browser at 375, 768, 1440, and 1920 pixels.
5. Update component and E2E tests, then run the full handoff sequence.

Rollback is file-level because no database schema or API contract changes are introduced.

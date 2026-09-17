## Summary

Design a new planner shortcut that lets the user add a recipe directly from the library screen to an existing meal plan without leaving the browsing context.

## Problem

The current flow requires switching to the planner first and then selecting the recipe again. With larger catalogs this adds friction and extra context switching.

## Goals

- Define a fast, low-friction interaction from recipe card/details to plan assignment.
- Keep the interaction mobile-friendly and non-blocking for browsing.
- Reuse existing meal-planning constraints (date range, per-day uniqueness, inactive recipe behavior).

## Non-Goals

- No implementation in this change.
- No backend contract changes yet (unless design proves they are required).

## Proposed UX Direction

- Add a secondary action on recipe card/details: `Dodaj do planu`.
- Open a compact picker (sheet/modal) with:
  - existing plans search/select,
  - day selection in chosen plan range,
  - multiplier input (default `1`).
- Confirm action with explicit success/failure toast.

## Risks / Resolved Decisions

- The picker does not create a new plan inline; it assigns only to an existing plan day.
- Duplicate handling remains enforced at the selected-day boundary and by the existing backend plan constraints.
- The picker reuses the existing plan projection and save contract instead of adding a second write path.

## Implementation Ownership

`improve-core-ui-ux` owns implementation and verification in tasks 4.1-4.5. This change remains the design record and will be archived as superseded design history after its capability is synchronized into the canonical `meal-planning` specification.

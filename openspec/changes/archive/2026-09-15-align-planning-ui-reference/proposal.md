## Why

The planning and shopping workflows are functionally complete but still read as stacked administration forms rather than a cooking workspace. The supplied desktop and mobile references demonstrate a clearer weekly hierarchy, stronger at-a-glance progress, and faster access to recipes and shopping state.

## What Changes

- Recompose the desktop planner as a weekly dashboard with period controls, summary metrics, a seven-day meal grid, and a contextual shopping panel.
- Recompose the mobile planner around one selected day, compact meal-slot sections, and thumb-reachable add and navigation actions.
- Represent breakfast, second breakfast, dinner, and supper as stable UI slots encoded through the existing entry ordering contract.
- Replace the full-page recipe selector with a responsive contextual drawer while preserving search, duplicate prevention, multiplier editing, reorder, removal, save, and regeneration behavior.
- Restyle the standalone shopping list and recipe details to match the same compact mobile and desktop hierarchy.
- Preserve the existing warm color palette, typography, API surface, persistence schema, and three primary destinations.

## Capabilities

### New Capabilities

- `planning-workspace-ui`: Responsive weekly planning dashboard, meal-slot interactions, contextual recipe picker, embedded shopping summary, and aligned shopping/detail screens.

### Modified Capabilities

None. The affected foundational capabilities have not yet been archived into `openspec/specs`.

## Impact

- Blazor layout and route components under `src/WhatToCook.Web/Components`.
- Planner UI state and entry ordering semantics; no database migration or API contract change.
- Shared CSS and responsive breakpoints.
- Component and Playwright scenarios for planning, shopping, details, navigation, and direct assignment.

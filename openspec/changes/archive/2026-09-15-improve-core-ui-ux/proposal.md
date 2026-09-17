## Why

The current visual language is coherent, but the core workflows do not consistently meet the repository's own mobile-first and safety principles. Recipe discovery is hidden below filters, meal planning does not scale beyond short pages, asynchronous states are ambiguous, and regenerating an existing shopping list can silently discard checklist and manual-item work.

## What Changes

- Make shopping-list regeneration explicit at the save boundary and preserve matching checklist state and all manual items through an atomic backend diff.
- Introduce distinct loading, loaded, empty, error, and busy states across recipe, planning, and shopping workflows.
- Compact the mobile application shell, remove horizontal overflow, guarantee thumb-sized controls, and preserve safe-area spacing.
- Lead the recipe library with title search, move advanced filters into responsive disclosure UI, and keep results visible near the top of the screen.
- Implement the existing `Dodaj do planu` shortcut from recipe browsing without forcing planner-first navigation.
- Rework meal planning around the selected plan and selected day, collapse plan creation when not needed, and keep save/generate actions reachable.
- Improve editor form semantics, keyboard submission, field-level validation relationships, page titles, focus behavior, and reduced-motion support.
- Add component and browser coverage for destructive confirmation, responsive overflow, loading/error states, keyboard use, and critical mobile layouts.
- Preserve the current typography, warm visual identity, three-destination navigation model, and simple day-based planner mental model.

## Capabilities

### New Capabilities

- `responsive-workflow-usability`: responsive shell, search-first library, direct add-to-plan, compact planner, and reachable primary actions.
- `accessible-async-feedback`: loading, error, busy, form, focus, and assistive-technology behavior shared by core UI workflows.

### Modified Capabilities

- `shopping-list-management`: replace destructive regeneration with an atomic non-destructive diff that preserves matching generated state and manual items.

## Impact

- Primary UI files under `src/WhatToCook.Web/Components`, layout styles, and `wwwroot/app.css`.
- Existing recipe catalog, meal planning, and shopping service contracts; no persistence schema change is expected.
- Component tests under `tests/WhatToCook.Web.ComponentTests` and Playwright scenarios under `tests/e2e`.
- The earlier design-only quick-add proposal is superseded by the direct-assignment implementation in this change and is retained only until its design history is archived.

## Context

See `proposal.md` for motivation. The Blazor Web App uses Interactive Server rendering, route-level feature components, shared service contracts, global CSS, and scoped layout CSS. The current services already expose recipe summaries, meal-plan details, plan save semantics, and shopping-list generation; the change should reuse those boundaries.

The existing foundation requires mobile-first layouts, sticky primary actions, search-led discovery, simple day lists, and explicit save states. The implementation currently diverges from those constraints, while duplicated global and scoped shell rules make responsive behavior difficult to reason about.

## Goals / Non-Goals

**Goals:**

- Keep each route's load and mutation state explicit and locally understandable.
- Make responsive layout behavior deterministic at phone, tablet, and desktop widths.
- Reuse current API contracts for direct plan assignment and regeneration.
- Keep browser and component tests aligned with every scenario in this change.

**Non-Goals:**

- No visual rebrand, design-system replacement, persistence migration, or API redesign.
- No dense seven-column calendar, drag-and-drop dependency, or client-side state framework.
- No client-side shopping-list restoration workflow; regeneration preserves matching generated state and manual items in the backend transaction.

## Decisions

### Use explicit local page state instead of a shared state framework

Each route will model loading, error, and mutation state with small enums or booleans owned by the component. This keeps failure semantics close to the UI and avoids introducing an application-wide state abstraction for a small route set.

Alternative considered: a generic asynchronous component wrapper. Rejected because each route has different empty, retry, and mutation semantics and a generic wrapper would hide important behavior.

### Consolidate shell ownership in scoped layout CSS

Global CSS will retain tokens and reusable primitives. Shell positioning, sizing, safe-area, and responsive behavior will live in the layout's scoped CSS. Grid tracks and flex children will explicitly allow shrinking with `minmax(0, 1fr)` and `min-width: 0`.

Alternative considered: move all styles to global CSS. Rejected because shell rules have one owner and scoped CSS prevents unrelated feature selectors from changing them.

### Use responsive disclosure for library filters

Title search and result summary remain visible. Advanced filters use native disclosure semantics on compact layouts and a persistent side panel on wide layouts. Selected-filter chips remain outside the disclosure so active constraints are never hidden.

Alternative considered: always-expanded horizontal filters. Rejected because measured phone and desktop layouts place results below the initial viewport.

### Normalize ingredient names at write time and filter time

Recipe writes will split recognized leading quantities and units from an ingredient name. The library applies the same display normalization when building its index so existing imports with legacy combined text remain searchable under the clean product name. The normalized filter key is URL-backed, while recipe details preserve their own quantity and unit fields.

### Keep global navigation and discovery available on long pages

Desktop primary navigation will render in the shell header while phone navigation remains thumb-reachable at the bottom. The library's search surface is a sticky control below the header. This avoids making users traverse hundreds of cards just to search again or reach a primary destination.

### Provide a large opt-in development seed profile

`SeedData:Profile=large` will generate deterministic recipe and plan records only when the database is empty. Default development and E2E seed data stay compact to preserve fast tests. The large profile is intended for manual visual inspection, not production or ordinary test execution.

### Use selected-day editing with compact day summaries

The planner will select one plan and one day for editing. On phones, a horizontal day picker exposes one full day editor. On larger screens, compact day summary cards remain visible and select the editing context. New-plan creation is a disclosure when plans exist. This preserves day-list semantics without repeating full controls seven times.

Alternative considered: a traditional calendar grid. Rejected because it compresses recipe names and conflicts with the existing simple-list requirement.

### Reuse meal-plan save for direct recipe assignment

The add-to-plan control loads existing plan details, appends one entry when the day is eligible, and saves through the current plan service. If the plan already has a generated shopping list, the control requires the same explicit regeneration decision used by planner editing.

Alternative considered: a new single-entry API endpoint. Deferred because the current contract is sufficient and adding an endpoint would expand backend scope without demonstrated latency need.

### Keep regeneration safety at the backend boundary

Direct plan-save requires an explicit regeneration decision when an existing shopping list is affected. After confirmation, the backend performs the atomic generated-item diff. Standalone shopping-list regeneration uses the same non-destructive merge and does not show a destructive-replacement confirmation because matching checklist state and manual items are preserved.
## Risks / Trade-offs

- [Risk] Selected-day planner editing hides other days on phones. -> Mitigation: keep a compact, horizontally scrollable day picker with recipe counts and preserve selection while editing.
- [Risk] Saving a whole plan for direct assignment can conflict with concurrent edits. -> Mitigation: keep the picker short-lived, reload details immediately before save, and surface save conflicts without leaving browsing context.
- [Risk] Consolidating shell CSS can regress intermediate widths. -> Mitigation: add overflow assertions and screenshots at 375, 768, and 1440 CSS pixels.
- [Risk] Sticky actions can overlap bottom navigation or software-keyboard content. -> Mitigation: share navigation height and safe-area variables and reserve matching content padding.
- [Risk] Added state flags can make large components harder to read. -> Mitigation: keep operation state cohesive, derive labels from state, and extract only reusable UI such as the add-to-plan picker.
- [Risk] Normalizing a legacy ingredient can merge distinct names. -> Mitigation: remove only a leading numeric quantity and recognized unit, leaving unrecognized prefixes untouched.

## Migration Plan

1. Add specs and tests around current safety and responsive failures.
2. Implement shopping safety and asynchronous state first.
3. Consolidate shell and responsive primitives before changing feature layouts.
4. Migrate library, planner, and editor incrementally with scoped verification.
5. Run the complete component, E2E, formatting, and repository validation sequence.

Rollback is file-level because no database or external contract migration is introduced.

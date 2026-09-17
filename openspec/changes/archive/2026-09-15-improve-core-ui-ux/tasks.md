## 1. Shopping Safety and Async State

- [x] 1.1 Add component tests for initial generation, regeneration cancellation, and confirmed replacement
- [x] 1.2 Implement explicit shopping load, error, retry, and mutation-busy states
- [x] 1.3 Add an accessible regeneration confirmation that preserves data on cancel
- [x] 1.4 Add E2E coverage proving repeated generation cannot silently discard shopping work
- [x] 1.5 Run scoped shopping component and Playwright verification

## 2. Responsive Shell and Shared Accessibility

- [x] 2.1 Consolidate shell ownership in scoped layout styles and remove competing global shell rules
- [x] 2.2 Implement a compact mobile action menu, safe-area spacing, non-overflowing grid tracks, and 44-pixel touch targets
- [x] 2.3 Add page titles, primary headings, route-focus styling, primary-button contrast, and reduced-motion behavior
- [x] 2.4 Add responsive E2E assertions for 375, 768, and 1440 CSS-pixel viewports
- [x] 2.5 Run scoped shell component and Playwright verification

## 3. Search-First Recipe Library

- [x] 3.1 Add component tests for title-search priority, filter disclosure, selected-filter summary, and result count
- [x] 3.2 Recompose the library into a search-first results layout with responsive advanced filters
- [x] 3.3 Add truthful loading, error, retry, empty, and not-found states to recipe library and details
- [x] 3.4 Simplify recipe-card navigation while preserving edit and filter-return context
- [x] 3.5 Run scoped recipe library component and Playwright verification

## 4. Direct Add to Plan

- [x] 4.1 Add a reusable accessible add-to-plan picker for recipe library and detail contexts
- [x] 4.2 Reuse meal-plan loading and save contracts with duplicate-day and regeneration handling
- [x] 4.3 Add component and E2E coverage for successful assignment, duplicate prevention, and generated-list confirmation
- [x] 4.4 Update the design-only quick-add traceability tasks to reference the completed implementation owner
- [x] 4.5 Run scoped direct-assignment verification

## 5. Compact Meal Planner

- [x] 5.1 Add component tests for automatic plan selection, collapsed creation, selected-day editing, and sticky actions
- [x] 5.2 Recompose planner loading and plan selection around a compact selected-plan summary
- [x] 5.3 Implement phone day-picker editing and desktop compact day summaries without a dense calendar
- [x] 5.4 Replace repeated recipe controls with one selected-day searchable add control
- [x] 5.5 Add busy, dirty, retry, save, and regeneration feedback states
- [x] 5.6 Extend E2E coverage for mobile day navigation, desktop summaries, and reachable save actions
- [x] 5.7 Run scoped planner component and Playwright verification

## 6. Editor and Form Semantics

- [x] 6.1 Add keyboard and validation component tests for import, recipe editor, plan creation, and manual shopping items
- [x] 6.2 Convert single-purpose actions to form submission semantics with repeat-submit protection
- [x] 6.3 Relate dynamic field labels and validation messages and focus the first invalid recipe field
- [x] 6.4 Replace the redundant mobile workflow card with compact progress navigation and keep editor save reachable
- [x] 6.5 Run scoped editor and form verification

## 7. Traceability and Handoff

- [x] 7.1 Add a scenario-to-test traceability matrix for all three capabilities
- [x] 7.2 Run `dotnet build WhatToCook.slnx`
- [x] 7.3 Run `dotnet test WhatToCook.slnx`
- [x] 7.4 Run the full Playwright suite for desktop and mobile projects
- [x] 7.5 Run `dotnet husky run` and CSharpier fallback when no files are staged
- [x] 7.6 Run strict OpenSpec validation and complete an independent quality review

## 8. Large-Scale Discovery and Preview

- [x] 8.1 Normalize ingredient names at recipe write time and while indexing legacy library entries
- [x] 8.2 Add component and integration coverage for quantity-and-unit ingredient normalization
- [x] 8.3 Move desktop primary navigation into the persistent shell header and keep phone navigation thumb-reachable
- [x] 8.4 Make library search reachable during long scrolling
- [x] 8.5 Add deterministic opt-in large development seed data with 100 recipes and 20 plans
- [x] 8.6 Verify the large development profile visually at phone, tablet, and desktop widths

## 9. Safe Backend Regeneration Diff

- [x] 9.1 Replace client-side shopping-list restoration with an atomic backend diff
- [x] 9.2 Preserve matching checklist state and all manual duplicates during regeneration
- [x] 9.3 Update shopping tests and OpenSpec traceability for the non-destructive regeneration contract

## 10. Remaining UI Polish

- [x] 10.1 Keep the desktop direct-add panel visible beyond the card media without clipping or hover loss
- [x] 10.2 Use human-readable Polish date labels and keep native date inputs free of format literals
- [x] 10.3 Keep the active-plan label and selector visible on mobile and close the action menu after route changes
- [x] 10.4 Consolidate the recipe editor navigation into Podstawy, Składniki, Przygotowanie, and Dodatkowe
- [x] 10.5 Hide planner micro-actions until hover or keyboard focus while preserving accessible controls
- [x] 10.6 Align remaining UI typography, borders, focus states, and desktop shopping-list spacing

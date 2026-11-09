# ARC-01 (targeted) — Extract the reusable planner meal card

## Why
Planner markup repeats the same compact/expanded meal card across desktop and mobile. Rendering and action controls live in an inline RenderFragment on an already large page.

## Changes
Move meal-card presentation to a feature-local Blazor component with explicit display data and action callbacks. Keep mutable planner state and all plan operations in the page. Add bUnit tests for compact rendering, disabled state and event forwarding.

## Boundaries
No new state store/coordinator or generic component system; no changes to meal-plan API, persistence, shopping regeneration, accessibility labels, CSS selectors or deployment. This is a targeted ARC-01 improvement, not full-page decomposition.

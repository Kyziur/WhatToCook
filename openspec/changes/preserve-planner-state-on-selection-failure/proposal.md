# Preserve planner state when plan loading fails

## Why
Plan selection cleared CurrentPlan before confirming that the target loaded. A transient request error could remove an edited unsaved draft from the UI and leave the selected ID inconsistent.

## Changes
Keep the previous plan and draft during a pending switch, commit new plan state only after its details load, and restore the selection ID on missing/error/cancelled result. Remove old, unreferenced handlers from the planner and the now-unreachable optional shopping dependency guard.

## Boundaries
No new state layer or generic store, API, data schema, layout, or deployment change. This is not a new navigation-confirmation feature; successful switching keeps its existing semantics.

## Verification
bUnit regression for failed selection with dirty draft. Existing planner, API and Playwright suites cover unchanged flows.

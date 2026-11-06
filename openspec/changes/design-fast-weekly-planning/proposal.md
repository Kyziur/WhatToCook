## Why

Household meal planning is repetitive: users already have successful weeks and favourite combinations but must recreate arrangements manually. The current planner supports date-range plans, recipe assignment, ordering and multipliers, yet lacks a focused copy-and-adjust flow.

## What Changes

- Create a new plan from an existing plan, preserving relative day assignments, meal slots, order and multipliers without mutating the source.
- Preview and edit the target week immediately, including simple replacement, moving and swapping meals without mandatory drag-and-drop.
- Represent `Eating out / No cooking` and linked leftovers without inventing fake recipes or duplicating shopping ingredients.
- Preserve the current shopping-list regeneration decision while saving edits.
- Define saved combinations as stretch scope rather than a gate for an initial copy-and-edit release.
- Add product-level user stories and PENDING test-traceability for eventual implementation.

## Capabilities

### New Capabilities
None.

### Modified Capabilities
- `meal-planning`: extend the existing canonical capability with planning reuse, flexible slot handling and safe changes.

## Impact

Future implementation touches meal plan persistence/validation, plan duplication and edit endpoints, Blazor planner UI, shopping-list regeneration, and integration/component/E2E tests.

## Non-Goals

- No implementation, migrations, application changes or automated tests in this design-only package.
- No generative AI weekly-menu planner, nutrition optimization, calorie counting or external calendar synchronization.
- No automatic pantry deduction or guarantees about leftover portion quantities.

## Product outcome

A planner can reuse a previously successful week and adjust five dinners in approximately three minutes in a measured usability session. See [user-stories.md](user-stories.md) for detailed narratives and acceptance criteria.

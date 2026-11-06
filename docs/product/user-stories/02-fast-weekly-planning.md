# Epic WP — Plan a Week in Two Minutes

**Priority:** P1 — second proposed product epic  
**Status:** Proposed / not implemented  
**Users:** Household meal planner, cook, secondary household member  
**Primary outcome:** Reuse a previous weekly plan with minimal manual repetition.

## Problem and job to be done

WhatToCook already supports named date-range meal plans, assignment of recipes to days, ordered entries, meal slots, multipliers, quick-add from the library, and shopping-list regeneration. However, repeating the same planning decisions each week is still a manual task.

**Job story:** *When I need to plan next week's meals and already know what our household likes, I want to reuse a previous plan and make a few substitutions so that I can finish planning in minutes instead of rebuilding every day from scratch.*

**Product success hypothesis:** A user can prepare a realistic plan for five dinners in under three minutes, starting from an existing plan.

## MVP user stories and acceptance criteria

### WP-01 — Start a new plan by copying an existing plan

**As a** planner, **I want** to copy a previous plan into new dates **so that** I can reuse a successful week.

**Acceptance criteria**

- **AC1:** From the planner, I can choose **Copy an existing plan** and select a source plan; the most recent plan may be preselected but I can choose another.
- **AC2:** I can select a new start date, preview the resulting date range, name the new plan, and confirm.
- **AC3:** Copying creates a **new plan with a new ID**; it never mutates the source plan.
- **AC4:** Planned recipes map to the corresponding day offset in the target date range; their meal slot, order, and multiplier are preserved.
- **AC5:** The copy operation detects duplicate recipe/day conflicts and does not produce an invalid plan.
- **AC6:** The new plan is not silently considered shopped-for: existing purchase or checklist states from the source are not copied into a fresh shopping list.
- **AC7:** Copying the source again produces a separate new plan unless the user explicitly chooses to edit an existing target.

**Scope decision:** For MVP, default to copying the source plan's entire date range length. Changing the duration is a separate refinement and must define how out-of-range entries are handled.

### WP-02 — Review the copied week at a glance

**As a** planner, **I want** to see the copied meals before saving **so that** I can quickly spot days I need to change.

**Acceptance criteria**

- **AC1:** A preview identifies the target dates and recipe assignments grouped by day and meal slot.
- **AC2:** Empty days and unassigned meals are visually distinguishable from filled slots.
- **AC3:** I can open the target plan for adjustment immediately after copying, without having to navigate back to the plan selector.
- **AC4:** On mobile, I can move between days with accessible controls and understand which date is selected.
- **AC5:** A failure to copy leaves the source plan unchanged, with no partially created target visible.

### WP-03 — Swap a planned meal for another recipe

**As a** planner, **I want** to replace a recipe in a specific slot **so that** I can adapt the week without deleting and rebuilding surrounding meals.

**Acceptance criteria**

- **AC1:** A planned recipe has a clear **Replace** action.
- **AC2:** Choosing a replacement preserves the original day and meal slot unless I explicitly change them.
- **AC3:** The picker searches active recipes and supports existing library search/filter behavior where available.
- **AC4:** Selecting a recipe already scheduled on that same day is rejected with an understandable message; a recipe planned on another day is allowed.
- **AC5:** On cancel, the original assignment is preserved.
- **AC6:** After successful replacement, the displayed plan and dirty/saved state reflect the change, and any existing shopping list triggers the current regeneration decision process.

### WP-04 — Move or swap meals between days

**As a** planner, **I want** to move a planned meal to another day **so that** I can react to schedule changes without recreating the recipe assignment.

**Acceptance criteria**

- **AC1:** I can choose **Move to another day/slot** using a tap- and keyboard-friendly control; drag-and-drop is optional, not required.
- **AC2:** When moving into an empty destination, the entry's recipe and multiplier are preserved.
- **AC3:** When the destination contains another meal, the UI explicitly offers a **Swap** option rather than overwriting it unexpectedly.
- **AC4:** Moving or swapping cannot create a duplicate recipe on the same day.
- **AC5:** Canceling a move or swap leaves both assignments unchanged.
- **AC6:** Saving uses the existing plan validation and does not bypass shopping-list regeneration rules.

### WP-05 — Mark a meal as leftovers or eating out

**As a** planner, **I want** to represent days when I will not cook a new dish **so that** the plan matches our real schedule.

**Acceptance criteria**

- **AC1:** A slot can be marked as **Eating out / No cooking** without creating a fake recipe in the recipe library.
- **AC2:** A slot can be marked as **Leftovers from an earlier meal** and linked to the source meal in the same plan.
- **AC3:** The plan visually distinguishes a recipe cooked from scratch from leftovers and eating out.
- **AC4:** **Eating out / No cooking** does not add ingredients to a shopping list.
- **AC5:** **Leftovers** do not create a second set of ingredients; the source meal's intended cooked quantity remains the input to shopping-list generation.
- **AC6:** If the source meal is moved, removed, or changed, the leftover reference is validated and the user is prompted to resolve it instead of keeping a broken link.
- **AC7:** If the source meal is planned at a multiplier insufficient for desired leftovers, the UI does not imply a sufficient quantity; the user can adjust the multiplier.

**Product note:** Leftovers are a planning annotation, not inventory tracking or automatic portion prediction.

### WP-06 — Save, cancel, or regenerate shopping requirements safely

**As a** planner, **I want** the application to clearly explain how changes affect the shopping list **so that** I do not unexpectedly lose a useful list.

**Acceptance criteria**

- **AC1:** If there is no generated shopping list, saving a revised plan succeeds without forcing generation.
- **AC2:** If an existing shopping list would change, the user sees the established choice to regenerate or cancel before the plan and list are changed.
- **AC3:** Cancel leaves the persisted plan and shopping list unchanged.
- **AC4:** Confirming regeneration updates plan and list together and preserves applicable manual items and checklist states under existing list reconciliation rules.
- **AC5:** The UI exposes pending, success, and failure states; repeated clicks do not save duplicate changes.

### WP-07 — Quickly repeat favourite meal combinations

**As a** planner, **I want** a saved group of meals for common occasions **so that** I can fill the next plan without repeatedly searching for each recipe.

**Acceptance criteria**

- **AC1:** I can save a reusable, named meal combination from selected plan entries; it does not overwrite the source plan.
- **AC2:** I can insert the saved combination starting at a chosen date in a target plan, with a preview showing the inserted meals and any conflicts.
- **AC3:** Recipe IDs, meal slots, relative order, and multipliers are carried over.
- **AC4:** Archived/inactive recipes in a combination are shown as unavailable for new assignment and are excluded or require an explicit replacement.
- **AC5:** The user confirms replacement when a destination slot already contains a meal.
- **AC6:** I can rename and delete a saved combination without deleting recipes or existing plans.

**Release strategy:** WP-07 is a **stretch story** for the epic. Do not hold the first copy-and-edit release for this feature unless validation shows it is essential.

## Out of scope for first release

- Generative AI creating a full weekly menu.
- Automatic nutrition optimization and budgeting.
- Household calendar integration.
- Predicting future grocery prices or portions.
- Automatic pantry deduction for leftovers.

## Cross-cutting cases and test expectations

- Copy and move operations honor the existing rule: the same recipe can appear on different days but **not twice on one day**.
- Archived recipes stay visible in historical plans but cannot be newly scheduled.
- A plan may include multiple entries/slots per day and positive decimal multipliers; no data is dropped when copying.
- Date mapping works across month/year boundaries and daylight saving transitions; use calendar dates rather than naive timestamp arithmetic.
- Saving updates uses concurrency protection so one member's changes do not silently destroy another member's changes. Conflict UX must be explicitly designed if multiple editors are supported.
- Integration and component tests cover copying, source immutability, slot conflicts, leftover shopping semantics, cancelled regeneration, and mobile navigation.
- Measure both successful time-to-plan and number of interactions; do not treat “two minutes” as a hard-coded timeout.

## Success measures

- Median time from choosing **Copy plan** to a usable plan containing five dinners.
- Share of new weekly plans created by copying rather than from an empty calendar.
- Number of manual edits required after copying.
- User-reported difficulty resolving changes that impact a shopping list.

## Dependencies and implementation handoff

- Existing [meal-planning](../../../openspec/specs/meal-planning/spec.md) requirements, planner UI, and shopping reconciliation.
- WP-05 requires explicit model changes to represent non-recipe entries and leftovers safely.
- WP-01 through WP-04 can be delivered before leftovers or saved combinations.
- Agree OpenSpec delta, migration strategy, and test traceability before implementation.

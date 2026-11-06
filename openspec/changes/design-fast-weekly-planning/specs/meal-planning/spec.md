## ADDED Requirements

### Requirement: WP-01 — User can copy an existing meal plan into new dates
The system SHALL allow creating a separate plan from an existing plan while preserving its recipe assignments, dates as relative offsets, meal slots, order, and positive multipliers.

#### Scenario: WP-01 — Choose a source plan and target dates
- **WHEN** a planner chooses `Copy existing plan`, selects any available source, and enters a new start date and name
- **THEN** the system previews the target date range using the source plan's existing duration by default

#### Scenario: WP-01 — Create an independent copy
- **WHEN** the planner confirms a valid copy of a meal plan
- **THEN** the system creates a new plan ID, maps planned recipes to corresponding target-day offsets, and preserves each entry's slot, order, and multiplier without changing the source

#### Scenario: WP-01 — Preserve uniqueness and independence
- **WHEN** copying would produce an invalid same-day duplicate or the user copies the same source repeatedly
- **THEN** the system rejects invalid assignments and creates independent copies for valid repeated requests rather than modifying existing plans

#### Scenario: WP-01 — Keep shopping progress out of a fresh plan
- **WHEN** a previous plan with a generated shopping list is copied
- **THEN** the new plan does not inherit the source's bought or `MAM` checklist states or claim that a new list was already generated

### Requirement: WP-02 — User can inspect a copied plan at a glance
The system SHALL make the new target plan immediately reviewable on desktop and mobile before further changes.

#### Scenario: WP-02 — Preview target assignments
- **WHEN** a planner previews or opens a copied plan
- **THEN** recipes are grouped by target date and meal slot, with empty slots visibly distinguishable from filled ones

#### Scenario: WP-02 — Continue editing the new plan
- **WHEN** copy creation succeeds
- **THEN** the planner can adjust the new plan directly, without returning to the plan list to find it

#### Scenario: WP-02 — Navigate dates on mobile
- **WHEN** the preview is opened on a narrow screen
- **THEN** the planner can access every target date using readable, keyboard- and touch-accessible controls

#### Scenario: WP-02 — Fail without creating a partial plan
- **WHEN** copying fails due to validation or persistence error
- **THEN** the source remains intact and no partially created target is exposed as a valid plan

### Requirement: WP-03 — User can replace a planned recipe in place
The system SHALL provide a focused way to replace one recipe assignment without deleting unrelated meals.

#### Scenario: WP-03 — Replace an assigned recipe
- **WHEN** a planner chooses `Replace` for a planned recipe and selects another active recipe
- **THEN** the assignment changes to the chosen recipe while retaining its target date and meal slot

#### Scenario: WP-03 — Enforce same-day uniqueness
- **WHEN** the selected replacement recipe is already present on that date
- **THEN** the system rejects the duplicate and explains the conflict; the same recipe on another date remains allowed

#### Scenario: WP-03 — Preserve assignment when cancelled
- **WHEN** the planner cancels the replacement picker
- **THEN** the original recipe and its saved assignment remain unchanged

#### Scenario: WP-03 — Keep shopping-list changes explicit
- **WHEN** a replacement is saved for a plan with an existing generated shopping list
- **THEN** the established regenerate-or-cancel decision flow applies before changes are persisted

### Requirement: WP-04 — User can move or swap meals between dates and slots
The system SHALL support rearranging meal assignments without requiring drag-and-drop.

#### Scenario: WP-04 — Move into an empty destination
- **WHEN** a planner moves an entry to an empty day or meal slot
- **THEN** its recipe and positive multiplier are retained and the source assignment is removed

#### Scenario: WP-04 — Swap occupied assignments explicitly
- **WHEN** the destination already has an assigned meal
- **THEN** the planner can explicitly choose a swap or cancel rather than silently overwriting the destination

#### Scenario: WP-04 — Reject duplicate recipes after a move
- **WHEN** moving or swapping would result in the same recipe appearing twice on the same day
- **THEN** the operation is rejected without changing either assignment

#### Scenario: WP-04 — Save move consistently
- **WHEN** a planner confirms a valid move or swap
- **THEN** the normal plan validation, dirty/saved feedback, and existing shopping-regeneration contract remain in effect

### Requirement: WP-05 — User can plan leftovers or a no-cooking meal
The system SHALL support non-recipe planning annotations without requiring fake recipes in the trusted catalog.

#### Scenario: WP-05 — Mark an eating-out slot
- **WHEN** a planner marks a meal slot as `Eating out / No cooking`
- **THEN** the plan displays that state clearly and no recipe-derived ingredients are added for it

#### Scenario: WP-05 — Mark leftovers from an earlier meal
- **WHEN** the planner links a target meal slot to leftovers from an earlier recipe assignment in the same plan
- **THEN** the target is identified as leftovers and no duplicate ingredient quantity is added to the shopping list for the leftover slot

#### Scenario: WP-05 — Explain how many portions must be cooked
- **WHEN** a leftover slot references a meal whose multiplier may not produce enough food
- **THEN** the system does not pretend to calculate guaranteed remaining servings and allows the planner to adjust the source multiplier

#### Scenario: WP-05 — Validate leftover dependencies
- **WHEN** the original cooked meal is deleted, moved to a later time, or replaced with an incompatible assignment
- **THEN** the planner is prompted to resolve the leftover link rather than retaining an invalid reference

### Requirement: WP-06 — User can save planning changes without losing shopping-list integrity
The system SHALL preserve the existing explicit regeneration decision and prevent partial or accidental list changes during rapid editing.

#### Scenario: WP-06 — Save when there is no existing list
- **WHEN** a planner saves an edited plan that has no generated shopping list
- **THEN** the plan is saved without automatically generating a list

#### Scenario: WP-06 — Confirm or cancel regeneration
- **WHEN** a planner saves changes that affect a plan with an existing generated shopping list
- **THEN** the planner must confirm regeneration or cancel the save, and cancellation leaves plan and list unchanged

#### Scenario: WP-06 — Reconcile generated requirements safely
- **WHEN** the planner confirms a plan edit and regeneration
- **THEN** the plan and generated list are updated atomically while applicable manual items and checklist states are preserved

#### Scenario: WP-06 — Report save state and avoid duplicate requests
- **WHEN** a save is pending or fails and the planner retries or navigates
- **THEN** the UI accurately indicates the result and avoids duplicate mutations from repeated actions

### Requirement: WP-07 — User can reuse a saved combination of meals
The system SHALL support named reusable sets of planned meals as a **stretch scope**, independently of the primary copy-and-edit release.

#### Scenario: WP-07 — Save and reuse a named combination
- **WHEN** a planner saves selected meal entries as a named combination and inserts them into another plan
- **THEN** recipe references, relative offsets, meal slots, ordering, and multipliers are retained without changing the original source plan

#### Scenario: WP-07 — Preview destination conflicts
- **WHEN** inserting a saved combination overlaps existing assignments or creates same-day recipe duplicates
- **THEN** the planner sees the conflicts and confirms replacements or cancels; nothing is silently overwritten

#### Scenario: WP-07 — Handle archived recipe references
- **WHEN** a saved combination references a recipe that has become inactive
- **THEN** the unavailable recipe is identified and cannot be added as a new assignment until replaced or excluded

#### Scenario: WP-07 — Manage combination lifecycle
- **WHEN** a planner renames or deletes a saved combination
- **THEN** existing meal plans and recipes remain unaffected

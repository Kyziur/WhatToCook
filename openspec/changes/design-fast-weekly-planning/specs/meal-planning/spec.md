## ADDED Requirements

### Requirement: WP-01 — User can copy an existing meal plan into new dates
The system SHALL allow creating a separate plan from an existing plan while preserving all supported entry types, relative calendar dates, meal slots, order, and positive recipe multipliers. If the source contains leftovers, their links SHALL be remapped to the corresponding entries in the new plan.

#### Scenario: WP-01 — Choose a source plan and target dates
- **WHEN** a planner chooses `Copy existing plan`, selects any available source, and enters a new start date and name
- **THEN** the system previews the target date range using the source plan's existing duration by default

#### Scenario: WP-01 — Create an independent copy
- **WHEN** the planner confirms a valid copy of a meal plan
- **THEN** the system creates a new plan ID, maps scheduled entries by day offset, preserves each active recipe's slot (encoded in SortOrder), relative position and multiplier, and never mutates the source

#### Scenario: WP-01 — Copy no-cooking entries and rebind leftovers
- **WHEN** a source plan includes No cooking and Leftovers entries, and the user copies it after those entry types are supported
- **THEN** the new plan retains their day/slot placement, allocates fresh entry IDs and maps each leftover reference to its corresponding copied cooked entry, never to an entry ID in the source plan; if its source was omitted or replaced during archive resolution, the copy is not committed until the leftover is explicitly rebound or removed

#### Scenario: WP-01 — Resolve archived source recipes
- **WHEN** the selected source plan contains recipes that have since been archived or are otherwise inactive
- **THEN** the preview identifies the unavailable entries and requires the planner to replace or omit each one before the new plan can be saved; the source remains unchanged

#### Scenario: WP-01 — Handle a deliberate second copy and a network retry
- **WHEN** the user deliberately starts a second copy of the same source
- **THEN** a distinct valid plan may be created; retrying an already accepted confirmation with the same operation identity does not create another plan

#### Scenario: WP-01 — Reject invalid assignments
- **WHEN** the copied target would violate same-day recipe uniqueness or other current plan validation
- **THEN** the copy is rejected with an actionable explanation and no partial target is persisted

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
The system SHALL let users replace one specific planned recipe, retaining its date, encoded meal slot, intra-slot position and existing positive multiplier by default, without changing other entries.

#### Scenario: WP-03 — Replace an assigned recipe
- **WHEN** a planner chooses `Replace` for a planned recipe and selects another active recipe
- **THEN** the selected assignment changes to the chosen recipe while retaining its date, meal slot, relative intra-slot order and positive multiplier; the planner may explicitly change the multiplier afterwards

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
The system SHALL move individual entries between dates and meal slots. Multiple recipes **may coexist within the same slot**, so moving into an occupied slot does not imply replacement. Drag-and-drop is optional.

#### Scenario: WP-04 — Move into an empty destination
- **WHEN** a planner moves an entry to an empty day or slot
- **THEN** its recipe, multiplier, and relative ordering are retained where possible, and the source assignment is removed

#### Scenario: WP-04 — Append to a slot containing other meals
- **WHEN** a planner moves a recipe into a slot that already contains one or more different recipes
- **THEN** the moved recipe is appended at a valid position without deleting or replacing other entries, provided same-day recipe uniqueness is preserved

#### Scenario: WP-04 — Swap two explicitly selected entries
- **WHEN** a planner chooses Swap and explicitly selects a source entry and one destination entry
- **THEN** the two entries exchange their complete placements (date, meal slot and relative position within that slot), preserving each entry's own multiplier; unrelated entries remain in their original relative order (even if sort indices are renumbered), and both resulting days satisfy recipe uniqueness

#### Scenario: WP-04 — Reject duplicate recipes after a move
- **WHEN** moving, appending or swapping results in the same recipe appearing twice on one date
- **THEN** the operation is rejected without changing the source or destination assignments

#### Scenario: WP-04 — Save move consistently
- **WHEN** a planner confirms a valid move or swap
- **THEN** the normal plan validation, dirty/saved feedback, and existing shopping-regeneration contract remain in effect

### Requirement: WP-05 — User can plan leftovers or a no-cooking meal
The system SHALL support no-cooking and leftover **planning entries** distinct from saved recipes. A No cooking entry excludes other recipe/leftover entries in the same date-and-slot; a leftover references an earlier cooked recipe entry by stable entry ID.

#### Scenario: WP-05 — Mark an eating-out slot
- **WHEN** a planner marks an empty date-and-slot as `Eating out / No cooking`
- **THEN** the plan displays that annotation, adds no recipe ingredients, and prevents other meal entries from being silently retained in that slot; the planner must explicitly clear/replace an occupied slot

#### Scenario: WP-05 — Mark leftovers from an earlier meal
- **WHEN** the planner links a leftover entry to a cooked recipe entry from an earlier date or strictly earlier meal slot in the same plan
- **THEN** the leftover is linked by stable entry ID and adds no extra ingredient quantities beyond the source recipe's multiplier

#### Scenario: WP-05 — Explain how many portions must be cooked
- **WHEN** a leftover slot references a meal whose multiplier may not produce enough food
- **THEN** the system does not pretend to calculate guaranteed remaining servings and allows the planner to adjust the source multiplier

#### Scenario: WP-05 — Validate leftover dependencies
- **WHEN** the source cooked entry is deleted, archived from a new copy, moved after its leftover, or changed into a non-cooking entry
- **THEN** the save is blocked until the leftover reference is removed or reassigned to a valid earlier cooked recipe entry

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
- **THEN** the plan and list are updated atomically, manual items remain intact, and generated state follows the current canonical shopping-list-management reconciliation rules (including any later adopted quantity-change reset)

#### Scenario: WP-06 — Report save state and avoid duplicate requests
- **WHEN** a save is pending or fails and the planner retries or navigates
- **THEN** the UI accurately indicates the result and avoids duplicate mutations from repeated actions


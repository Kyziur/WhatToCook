## ADDED Requirements

### Requirement: SL2-01 — User can mark shopping-list items as bought
The system SHALL support a persistent `Bought` state distinct from the existing `MAM` (already available at home) and `NIE MAM` (need to buy) semantics.

#### Scenario: SL2-01 — Mark a required item bought
- **WHEN** a user marks an outstanding `NIE MAM` item as bought
- **THEN** the list persists its bought state, shows immediate visible feedback, and removes the item from the outstanding count

#### Scenario: SL2-01 — Undo buying an item
- **WHEN** a user undoes a bought item
- **THEN** the item becomes outstanding again without a duplicate or loss of item metadata

#### Scenario: SL2-01 — Distinguish stocked and purchased items
- **WHEN** the list contains an item marked `MAM` and another marked `Bought`
- **THEN** the UI and persistence retain different meanings for "already at home" and "purchased during this trip"

#### Scenario: SL2-01 — Recover from an unsuccessful update
- **WHEN** saving a purchase-state change fails or the user reopens a saved list
- **THEN** the UI reports a failed save accurately or restores the persisted state, respectively, without falsely claiming synchronization

#### Scenario: SL2-01 — Accessible purchase control
- **WHEN** a shopper uses touch, keyboard, or assistive technology to change item state
- **THEN** the action is reachable, its current state is announced, and the target is usable on a phone

### Requirement: SL2-02 — User can shop from a focused mobile checklist
The system SHALL expose a mobile-friendly shopping view prioritizing outstanding items and accurate trip progress.

#### Scenario: SL2-02 — Show outstanding items first
- **WHEN** a shopper opens a list with a mix of `NIE MAM`, `Bought`, and `MAM` items
- **THEN** outstanding items are shown first, with explicit controls to inspect bought and already-at-home items

#### Scenario: SL2-02 — Calculate trip progress
- **WHEN** the shopping list contains bought, outstanding, and already-at-home items
- **THEN** progress uses bought and outstanding items as its denominator and excludes items already available at home

#### Scenario: SL2-02 — Make the list usable on a phone
- **WHEN** the shopper views the active list in a narrow viewport or returns from another screen
- **THEN** item names, quantities, units, categories, and controls are readable without horizontal scrolling and the active list is preserved

#### Scenario: SL2-02 — Explain empty and failed states
- **WHEN** a list is empty, all buying is complete, the list is loading, or an operation fails
- **THEN** the screen explains the state and offers an appropriate next action

### Requirement: SL2-03 — User can group purchases by store department
The system SHALL let the household classify shopping-list items using editable store departments without changing recipe ingredient definitions.

#### Scenario: SL2-03 — Group items by department
- **WHEN** a shopper views the checklist with categories assigned
- **THEN** items appear under stable department headings and uncategorized items appear under `Other / Uncategorised`

#### Scenario: SL2-03 — Reassign a category
- **WHEN** a shopper changes the department of a generated or manual item
- **THEN** its name, quantity, unit, checklist state, and identity remain unchanged

#### Scenario: SL2-03 — Keep category choices through list regeneration
- **WHEN** a plan-derived list is regenerated and an item can be matched to the previous list
- **THEN** its applicable category assignment is retained according to the documented ingredient-level inheritance rule

#### Scenario: SL2-03 — Allow shopping without categorization
- **WHEN** no department has been configured or the feature is disabled
- **THEN** the shopping list remains usable with all items visible

### Requirement: SL2-04 — Household members can collaborate on a shopping list
The system SHALL synchronize authorized household members' changes and apply explicit conflict rules rather than silently losing updates.

#### Scenario: SL2-04 — Reflect changes on another device
- **WHEN** an authorized member modifies an item and another member has the same household list open
- **THEN** the other member sees the saved change without manual export or import, either live or after reconnection/refresh

#### Scenario: SL2-04 — Preserve nonconflicting updates
- **WHEN** two members concurrently change different list items
- **THEN** both changes persist and neither overwrites the other

#### Scenario: SL2-04 — Resolve conflicting updates
- **WHEN** two devices concurrently change the same item to incompatible states
- **THEN** the system applies a documented, testable conflict-resolution rule and never deletes unrelated fields

#### Scenario: SL2-04 — Show synchronization outcome
- **WHEN** a list update is pending, succeeds, fails, or cannot reach the server
- **THEN** the UI clearly distinguishes those states and does not claim that unsent changes are synchronized

#### Scenario: SL2-04 — Authorize household data
- **WHEN** a member requests a list for a household to which they have no access
- **THEN** the request is denied without exposing that household's list contents

### Requirement: SL2-05 — User can include everyday manually entered products
The system SHALL let users add, change, remove, and purchase manual items independently of recipe-derived items.

#### Scenario: SL2-05 — Create and validate a manual item
- **WHEN** a shopper adds a valid free-text product with optional category and amount
- **THEN** the item is stored in the same checklist; blank or oversized input receives an actionable validation error

#### Scenario: SL2-05 — Edit or remove a manual item
- **WHEN** a shopper edits or deletes an existing manual entry
- **THEN** the change persists and accidental deletion can be cancelled or undone

#### Scenario: SL2-05 — Preserve duplicate manual entries
- **WHEN** a shopper deliberately creates two identical free-text items
- **THEN** both keep distinct identities until explicitly removed or merged

#### Scenario: SL2-05 — Retain manually entered products after regeneration
- **WHEN** a meal plan and its generated list are updated
- **THEN** manual items, their categories and their purchasing states remain intact

### Requirement: SL2-06 — Regeneration preserves trustworthy shopping progress
The system SHALL reconcile changes to generated requirements without destroying previously saved household shopping decisions.

#### Scenario: SL2-06 — Reconcile unchanged and new items
- **WHEN** a shopping list is regenerated after meal-plan edits
- **THEN** matched items retain applicable `MAM`, `Bought`, and category states while new generated items start as `NIE MAM`

#### Scenario: SL2-06 — Remove obsolete generated needs
- **WHEN** a recipe is removed from a plan and the user confirms regeneration
- **THEN** its now-obsolete generated items are removed when no longer needed while manual items remain

#### Scenario: SL2-06 — Handle a changed quantity safely
- **WHEN** regeneration increases or otherwise changes the amount of an item previously marked bought
- **THEN** the user can see that the quantity changed and the system does not misleadingly claim the new total has already been bought

#### Scenario: SL2-06 — Keep regeneration atomic
- **WHEN** regeneration fails during direct regeneration or a plan-save workflow
- **THEN** the previously committed plan, list, manual items, and purchase states remain consistent, without partial changes

### Requirement: SL2-07 — User can continue a loaded shopping trip through temporary connectivity loss
When the offline capability is approved for a release, the system SHALL provide readable cached lists, queued offline changes, and safe resynchronization. This is a **conditional release gate**, not functionality promised by the current Blazor Interactive Server deployment.

#### Scenario: SL2-07 — Read an already loaded list while disconnected
- **WHEN** a previously opened list loses connectivity
- **THEN** the shopper can still read its cached items and sees an explicit offline state; an uncached list is not presented as available offline

#### Scenario: SL2-07 — Queue an offline purchase
- **WHEN** offline mutation support is enabled and a shopper marks an item bought while disconnected
- **THEN** the mutation is stored durably on the device and visibly marked as pending synchronization

#### Scenario: SL2-07 — Reconcile changes after reconnecting
- **WHEN** the device reconnects after one or more queued actions
- **THEN** changes are replayed without duplicate manual entries or lost unrelated updates, and unresolved conflicts are visible

#### Scenario: SL2-07 — Survive mobile lifecycle events
- **WHEN** a shopper refreshes, suspends, or reopens the browser during offline operation
- **THEN** the previously cached list and pending operations remain available to the extent explicitly promised by the selected offline architecture

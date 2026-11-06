## ADDED Requirements

### Requirement: SL2-01 — User can mark shopping-list items as bought
The system SHALL maintain a persistent purchase indicator **separate from** the existing `MAM` (at home) / `NIE MAM` (need to buy) state. Bought is valid only when `NIE MAM`; switching to `MAM` clears Bought.

#### Scenario: SL2-01 — Mark a required item bought
- **WHEN** a user marks an outstanding `NIE MAM` item as bought
- **THEN** the list persists its bought state, shows immediate visible feedback, and removes the item from the outstanding count

#### Scenario: SL2-01 — Undo buying an item
- **WHEN** a user undoes a bought item
- **THEN** the same item becomes `NIE MAM` and not Bought without a duplicate or lost item metadata

#### Scenario: SL2-01 — Distinguish stocked and purchased items
- **WHEN** the list contains an item marked `MAM` and another marked `Bought`
- **THEN** the UI and persistence retain different meanings for "already at home" and "purchased during this trip"

#### Scenario: SL2-01 — Finish shopping and mark purchases as owned
- **WHEN** a shopper confirms Finish shopping on the active plan-linked list
- **THEN** all Bought items atomically become `MAM` with their purchase indicator cleared, while unpurchased `NIE MAM` items remain outstanding

#### Scenario: SL2-01 — Make completed purchases available to recommendations
- **WHEN** a user requests ingredient-based recommendations after Finish shopping
- **THEN** items converted to `MAM` are treated as owned by the existing recommendation logic, while not-yet-finished Bought items are not

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
- **THEN** progress counts Bought out of Bought-plus-outstanding `NIE MAM` items and excludes `MAM`; after Finish shopping the counter reflects remaining needs, not a historical completed-trip percentage

#### Scenario: SL2-02 — Make the list usable on a phone
- **WHEN** the shopper views the active list in a narrow viewport or returns from another screen
- **THEN** item names, quantities, units, categories, and controls are readable without horizontal scrolling and the active list is preserved

#### Scenario: SL2-02 — Explain empty and failed states
- **WHEN** no generated list exists, all buying is complete, the list is loading, connectivity is lost, or an operation fails
- **THEN** the screen explains the state and offers an appropriate next action; connected-only MVP never claims to support offline cached reading or queued writes

### Requirement: SL2-03 — User can group purchases by store department
The system SHALL offer item-level assignment to a fixed MVP department list: Produce, Dairy, Bakery, Meat & Fish, Pantry, Frozen, Household, Other. Users cannot create, rename, delete, or reorder departments in MVP.

#### Scenario: SL2-03 — Group items by department
- **WHEN** a shopper views the checklist with categories assigned
- **THEN** items appear under stable fixed-order department headings and uncategorized items appear under `Other`

#### Scenario: SL2-03 — Reassign a category
- **WHEN** a shopper changes the department of a generated or manual item
- **THEN** its name, quantity, unit, checklist state, and identity remain unchanged

#### Scenario: SL2-03 — Keep category choices through list regeneration
- **WHEN** a plan-derived list is regenerated and an item can be matched to the previous list
- **THEN** its applicable category assignment is retained for the same matched normalized ingredient and unit, independent of quantity changes

#### Scenario: SL2-03 — Show an ungrouped list
- **WHEN** a shopper chooses the ungrouped view
- **THEN** all items remain usable, irrespective of their department assignment

### Requirement: SL2-04 — Connected devices can collaborate on one trusted household list
The system SHALL automatically synchronize list mutations between devices using **one single-household deployment**. MVP has no app accounts or multi-household isolation. Access is restricted to a trusted LAN or authenticated private tunnel; public unauthenticated endpoints are unsupported.

#### Scenario: SL2-04 — Automatically synchronize connected devices
- **WHEN** device A saves an item mutation and connected device B displays the same list
- **THEN** B receives the confirmed new state without manual refresh, targeting no more than five seconds on a healthy connection

#### Scenario: SL2-04 — Preserve updates to different items
- **WHEN** devices concurrently change different items
- **THEN** both changes persist independently without overwriting each other

#### Scenario: SL2-04 — Reject stale changes to the same item
- **WHEN** a device attempts an update using an obsolete item version
- **THEN** the server rejects the write, returns the current state, and the user is prompted to review and deliberately retry; unrelated data is not overwritten

#### Scenario: SL2-04 — Report disconnected or rejected synchronization
- **WHEN** a connected-only client loses the server or receives a stale-write rejection
- **THEN** it clearly reports disconnection or conflict and never claims offline edits were queued or synchronized

#### Scenario: SL2-04 — Enforce trusted network access
- **WHEN** a device is outside the deployment's trusted LAN or authenticated private network
- **THEN** it cannot reach the shopping list; the unauthenticated application/API is never publicly exposed

### Requirement: SL2-05 — User can include everyday manually entered products
The system SHALL let users add, change, remove, and purchase manual items **on an already generated list associated with a meal plan**. Standalone manual-only lists are deferred.

#### Scenario: SL2-05 — Create and validate a manual item
- **WHEN** a shopper adds a valid free-text product with optional amount and a department from the fixed list to an existing generated list
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

### Requirement: SL2-06 — Regeneration preserves only still-valid shopping decisions
The system SHALL reconcile generated items by normalized ingredient and unit, preserving identities, categories, and unchanged quantities without falsely carrying buying/owning state across quantity changes.

#### Scenario: SL2-06 — Preserve state for unchanged ingredient and quantity
- **WHEN** a matched generated item has the same normalized ingredient, unit, and normalized amount after regeneration
- **THEN** its `MAM`/`NIE MAM`, Bought indicator, and category persist

#### Scenario: SL2-06 — Reset home/purchase state when amount changes
- **WHEN** a matched generated item has an increased, decreased, or changed descriptive amount after regeneration
- **THEN** it resets to `NIE MAM` with Bought cleared, its category remains, and a visible explanation identifies the amount change

#### Scenario: SL2-06 — Add and remove generated needs
- **WHEN** meal-plan ingredients change and regeneration is confirmed
- **THEN** new generated items start as `NIE MAM`, obsolete generated items are removed, and all manual entries remain

#### Scenario: SL2-06 — Do not carry buying state across different units
- **WHEN** the same ingredient appears with a different normalized unit
- **THEN** the new unit entry does not inherit `MAM` or Bought from the former unit

#### Scenario: SL2-06 — Regeneration is atomic in both update paths
- **WHEN** direct regeneration or plan-save-with-regeneration fails
- **THEN** the last committed plan, generated list, manual items and states remain consistent without partial changes


## MODIFIED Requirements

### Requirement: Regenerating a shopping list preserves matching generated state and manual items
The system SHALL require an explicit regeneration decision before saving meal-plan changes that affect an existing shopping list and SHALL atomically update generated items by stable ingredient-and-unit keys without removing manual items, including duplicates, or checklist state for matching generated items.

#### Scenario: Attempt to save changed plan with existing shopping list
- **WHEN** the user edits a meal plan that already has a generated shopping list and tries to save the plan
- **THEN** the system receives an explicit choice to confirm regeneration or cancel, and does not save the plan until that choice is made

#### Scenario: Cancel shopping list regeneration
- **WHEN** the user cancels the regeneration choice while saving meal plan changes
- **THEN** the plan changes remain unsaved and the existing shopping list remains unchanged

#### Scenario: Confirm shopping list regeneration
- **WHEN** the user confirms regeneration while saving meal plan changes
- **THEN** the backend atomically merges generated items, preserves matching `MAM` or `NIE MAM` states and every manual item, and removes stale generated items

#### Scenario: Regenerate a shopping list directly
- **WHEN** the user explicitly regenerates an existing shopping list
- **THEN** the system applies the same non-destructive reconciliation without requiring a client-side restore request

## ADDED Requirements

### Requirement: Shopping mutations expose operation progress
The system SHALL prevent repeated shopping mutations while a generation, checklist update, manual-item addition, or copy operation is already running.

#### Scenario: Repeat a shopping action while it is running
- **WHEN** the user activates the same shopping mutation more than once before the first request completes
- **THEN** the system executes one operation and exposes a visible busy state until completion

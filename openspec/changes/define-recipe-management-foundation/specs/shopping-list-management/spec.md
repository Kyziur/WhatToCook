## ADDED Requirements

### Requirement: User can generate a shopping list from a meal plan on demand
The system SHALL generate a shopping list only when the user explicitly requests it from a meal plan.

#### Scenario: Generate shopping list for a plan
- **WHEN** the user clicks the generate shopping list action for a meal plan
- **THEN** the system creates a shopping list for that plan from the currently planned recipes

#### Scenario: Do not generate list before explicit action
- **WHEN** the user creates or edits a meal plan without requesting a shopping list
- **THEN** the system does not create a shopping list automatically

### Requirement: Shopping list aggregates planned recipe ingredients by ingredient and unit
The system SHALL combine planned recipe ingredients into shopping list items by normalized ingredient and unit, while keeping different units as separate items.

#### Scenario: Merge matching ingredient and unit
- **WHEN** two planned recipes contribute the same normalized ingredient with the same unit
- **THEN** the system creates one shopping list item representing the aggregated quantity for that ingredient and unit

#### Scenario: Keep separate items for different units
- **WHEN** planned recipes contribute the same normalized ingredient with different units such as `szt` and `kg`
- **THEN** the system creates separate shopping list items for each unit

#### Scenario: Include ingredient without quantity
- **WHEN** a planned recipe contains an ingredient with no quantity or unit such as `sól do smaku`
- **THEN** the system adds a shopping list item showing only the ingredient name

### Requirement: Shopping list supports household checklist state and manual items
The system SHALL allow each shopping list item to be marked as `MAM` or `NIE MAM`, and SHALL allow the user to add manual text items to the same list.

#### Scenario: Mark item as already available
- **WHEN** the user toggles a shopping list item to `MAM`
- **THEN** the system stores that checklist state for the generated list

#### Scenario: Add manual text item
- **WHEN** the user adds a custom shopping list entry not derived from a recipe
- **THEN** the system stores the entry as a plain-text item in the shopping list

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
### Requirement: Shopping list can be copied as plain text
The system SHALL provide a plain-text copy action that includes the meal plan name followed by one shopping list item per line.

#### Scenario: Copy shopping list text
- **WHEN** the user activates the copy action for a shopping list
- **THEN** the system produces plain text beginning with the meal plan name and then one shopping list item per line

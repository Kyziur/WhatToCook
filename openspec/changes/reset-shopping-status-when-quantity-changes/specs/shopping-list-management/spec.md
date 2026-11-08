## MODIFIED Requirements

### Requirement: Regenerating a shopping list preserves matching generated state and manual items
The system SHALL require an explicit regeneration decision before saving plan changes that affect an existing list and SHALL atomically reconcile generated rows by normalized ingredient-and-unit keys. Checklist state SHALL be retained only for unambiguously matched generated items with unchanged normalized amount, including descriptive contribution context. Changed or ambiguous amounts SHALL reset to NIE MAM with a visible reason. All manual rows, including duplicates, SHALL remain intact.

#### Scenario: Confirm shopping list regeneration
- **WHEN** the user confirms regeneration while saving meal-plan changes
- **THEN** the backend atomically matches generated rows, preserves unambiguous unchanged MAM/NIE MAM, resets changed or ambiguous amounts, retains all manual entries and removes stale generated rows

#### Scenario: Regenerate a shopping list directly
- **WHEN** the user explicitly regenerates an existing list
- **THEN** the same quantity-aware atomic matching rules apply

#### Scenario: Cancel shopping list regeneration
- **WHEN** the user cancels the regeneration choice
- **THEN** the plan and shopping list remain unchanged

#### Scenario: Preserve unambiguous matching amounts
- **WHEN** normalized ingredient, unit, amount and descriptive contribution context remain unchanged
- **THEN** existing item identity and checklist state are preserved

#### Scenario: Reset changed or ambiguous amounts
- **WHEN** a matched amount changes or several generated rows cannot be paired unambiguously
- **THEN** affected rows reset to NIE MAM with a visible reason and cannot inherit an unrelated item's previous MAM state

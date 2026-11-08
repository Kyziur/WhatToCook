## ADDED Requirements

### Requirement: Archived recipes may only remain in their existing historical plan slot
The plan-update API SHALL allow an inactive recipe to remain assigned only when the submitted entry has the same persisted entry ID, recipe ID and planned day. Reusing an entry ID does not authorize assigning an archived recipe to a new day or replacing a different recipe.

#### Scenario: Keep archived historical plan entry
- **WHEN** an archived recipe's existing plan entry is saved with the same recipe and date
- **THEN** the API accepts retaining that entry, including safe multiplier or ordering changes

#### Scenario: Move archived recipe while reusing its entry ID
- **WHEN** a caller retains a historical row ID but changes the date of an archived recipe
- **THEN** validation rejects the new inactive assignment without mutating the persisted plan

#### Scenario: Substitute a different archived recipe under another entry's ID
- **WHEN** a caller supplies an existing row ID but swaps its recipe ID to an archived recipe
- **THEN** validation rejects the update without mutating the persisted plan

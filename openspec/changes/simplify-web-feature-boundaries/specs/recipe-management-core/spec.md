## ADDED Requirements

### Requirement: Recipe editor and import review present consistent ingredient units
The recipe editor and imported-draft review SHALL offer the same ordered ingredient unit values and labels, including an empty unit. Saving SHALL retain the selected existing unit token.

#### Scenario: Editing units in either workflow
- **WHEN** a user chooses an ingredient unit in the editor or the import-review form
- **THEN** both forms present the same choices and order
- **AND** the selected unit value is submitted unchanged

## ADDED Requirements

### Requirement: Save attempts release the editor saving state
The editor SHALL permit retry after any completed validation, API or optional image-upload failure without allowing duplicate simultaneous saves.

#### Scenario: API failure followed by retry
- **WHEN** the API save fails
- **THEN** the editor displays the failure and unlocks saving
- **AND** the next successful attempt follows the existing save navigation

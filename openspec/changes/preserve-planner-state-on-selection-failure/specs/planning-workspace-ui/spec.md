## ADDED Requirements

### Requirement: Failed plan selection preserves the current editable plan
The planner SHALL keep its current plan, unsaved changes and current selected plan if loading another plan fails.

#### Scenario: Another plan fails to load during an unsaved edit
- **WHEN** the user has an unsaved plan draft and selecting another plan fails
- **THEN** the planner keeps the draft and selected plan ID unchanged
- **AND** the user sees a recoverable error and can continue editing

## ADDED Requirements

### Requirement: Planner meal cards keep one presentation contract on desktop and mobile
The planner SHALL present the same meal title, multiplier and action semantics in compact and expanded cards while preserving page ownership of plan mutations.

#### Scenario: Meal card actions
- **WHEN** a user opens a meal card's actions and changes its multiplier or removes the entry
- **THEN** the planner handles the action through its existing state and persistence flow
- **AND** the card retains its accessible action labels and disabled state

## ADDED Requirements

### Requirement: User can add a recipe to an existing plan directly from library context
The system SHALL provide a direct planner shortcut from recipe browsing so the user can assign a recipe to an existing plan day without manually switching to planner-first flow.

#### Scenario: Open quick add-to-plan action from recipe library
- **WHEN** the user triggers `Dodaj do planu` on a recipe card
- **THEN** the system opens a focused add-to-plan picker with available plans and day selection

#### Scenario: Confirm assignment from quick picker
- **WHEN** the user selects plan, day, and confirms assignment
- **THEN** the system adds the recipe entry to that day using planner constraints and shows explicit success feedback

#### Scenario: Duplicate recipe in selected day
- **WHEN** the chosen day already contains the same recipe
- **THEN** the system blocks duplicate insertion and shows a clear corrective message

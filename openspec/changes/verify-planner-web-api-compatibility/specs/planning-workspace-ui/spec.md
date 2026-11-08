## ADDED Requirements

### Requirement: Planner Web HTTP client remains compatible with the live planning API
The Blazor planner client SHALL be able to exchange plan requests and responses with the deployed API contract, including regeneration-required conflicts, without requiring a shared runtime DTO assembly.

#### Scenario: Create, list and read a plan through the Web client
- **WHEN** the Blazor HTTP client creates a meal plan through the API
- **THEN** the same client can list and retrieve its ID, name, date range, generated-shopping-list flag and entries accurately

#### Scenario: Update requires regeneration and can be saved after explicit choice
- **WHEN** a generated shopping list exists and the client submits a changed plan without regeneration
- **THEN** the client recognizes the API's regeneration-required conflict and a subsequent confirmed regeneration save succeeds with the expected persisted plan

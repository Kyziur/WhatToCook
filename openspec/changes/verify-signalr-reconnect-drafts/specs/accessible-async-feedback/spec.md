## ADDED Requirements

### Requirement: Brief network interruptions preserve in-progress forms on reconnect

The Web application SHALL preserve entered, unsaved form values when an existing Blazor circuit successfully reconnects after a brief loss of browser connectivity; it SHALL allow an intended save only once after reconnection.

#### Scenario: Editor reconnects with an unsaved recipe
- **WHEN** the user has entered a recipe title, servings, ingredient and step, the connection drops for ten seconds, and the circuit reconnects
- **THEN** the reconnect overlay clears, entered fields are still present, and one user save creates exactly one recipe

#### Scenario: Planner reconnects with an unsaved plan
- **WHEN** the user has entered a name and date range for a new plan, the connection drops for ten seconds, and the circuit reconnects
- **THEN** the reconnect overlay clears, entered fields are still present, and one user save creates exactly one plan

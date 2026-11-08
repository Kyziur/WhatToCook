## ADDED Requirements

### Requirement: Recipe write telemetry minimizes household content

The API SHALL log useful bounded write/validation/conflict events without emitting user-supplied recipe titles, normalized titles, or internal photo paths as log fields or messages.

#### Scenario: Create recipe and receive validation or conflict responses
- **WHEN** a recipe title is valid, conflicts with another recipe or fails validation
- **THEN** HTTP response behavior remains unchanged and API write logs contain no user-supplied title or normalized-title value

#### Scenario: Replace a recipe photo
- **WHEN** the main photo is uploaded or replacement cleanup fails
- **THEN** operational logging identifies the recipe and event without emitting stored photo paths or exception messages that disclose host paths

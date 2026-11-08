## ADDED Requirements

### Requirement: Recipe import finalization is safe under concurrent requests
The system SHALL finalize a given import draft at most once across concurrent API requests, storing one finalized recipe ID and returning controlled responses.

#### Scenario: Ten requests finalize one ready draft concurrently
- **WHEN** ten clients concurrently finalize the same ready import draft
- **THEN** exactly one recipe is created, one request reports creation, all others report conflict with the same existing recipe ID, and the draft retains that final recipe ID without HTTP 500 responses

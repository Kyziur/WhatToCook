## ADDED Requirements

### Requirement: Deployment contract is mechanically verifiable
The repository SHALL verify the rendered Docker Compose contract without coupling deployment test success to unrelated documentation phrasing.

#### Scenario: Render the valid deployment
- **WHEN** Compose is rendered with the required nonproduction test environment
- **THEN** the expected services, storage bindings, internal Web-to-API routing and edge isolation are present

#### Scenario: Reject missing required data root
- **WHEN** the required DATA_ROOT is empty
- **THEN** Compose configuration validation fails rather than silently targeting an unexpected host path

## ADDED Requirements

### Requirement: Import failures are diagnosable without leaking source content

The system SHALL classify URL import fetch/extraction failures using low-cardinality internal categories and SHALL NOT log source HTML, full URLs or query strings as diagnostic properties.

#### Scenario: Expected import failure
- **WHEN** a supported source fails due to timeout, HTTP status, blocked destination, DNS or invalid response
- **THEN** an internal category, adapter and duration are available while the client receives a controlled existing error response

#### Scenario: Caller cancellation
- **WHEN** the HTTP caller cancels import
- **THEN** the cancellation is propagated rather than reported as a normal unsupported-source failure

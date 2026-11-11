# Image storage readiness

## ADDED Requirements

### Requirement: Readiness reflects image storage write/read availability
The API readiness endpoint SHALL be unhealthy when the configured recipe image directory does not exist, cannot be opened for creation of a new file, or cannot round-trip a marker byte.

#### Scenario: Writable mounted storage
- **WHEN** the directory supports file creation, writing and reading
- **THEN** the storage readiness check reports healthy
- **AND** the check leaves no probe file after completion

#### Scenario: Missing or unwritable mounted storage
- **WHEN** the configured directory is missing or rejects file creation
- **THEN** the storage readiness check reports unhealthy
- **AND** the check does not create the directory or alter recipe photos

### Requirement: Process liveness remains independent
The API `/alive` endpoint SHALL NOT depend on recipe image storage readiness.

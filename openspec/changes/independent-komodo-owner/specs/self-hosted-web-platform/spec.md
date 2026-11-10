# GitOps ownership

## Requirement: Independent WhatToCook Stack owner
The application repository SHALL provide a single declarative Komodo Stack named `what-to-cook` with `project_name = "whattocook"`. The homelab Resource Sync SHALL cease ownership only after a reviewed no-delete handoff.

### Scenario: App-only source change
- **WHEN** an application-only commit is proposed
- **THEN** it can be reviewed without changing the platform Resource Sync
- **AND** deployment remains manual without a webhook

### Scenario: Database preservation
- **WHEN** the Stack is adopted by its independent Resource Sync
- **THEN** the Compose project remains `whattocook` and existing state is not replaced with an empty `postgres-data` volume
- **AND** a prior independent backup and scratch restore has been verified

### Scenario: Conflicting owners
- **WHEN** the old and new Syncs both propose changes for the Stack
- **THEN** the operator must stop before applying any deletion or conflicting update

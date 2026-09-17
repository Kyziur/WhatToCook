## Purpose

Provides truthful asynchronous feedback and keyboard or assistive-technology semantics so users can understand and complete core workflows under normal, slow, and failing conditions.

## ADDED Requirements

### Requirement: Data screens distinguish loading, empty, missing, and failed states
The system SHALL render loading, empty, not-found, and failed states as mutually exclusive outcomes and SHALL offer retry for recoverable load failures.

#### Scenario: Load data over a delayed connection
- **WHEN** a recipe, planner, or shopping request has not completed
- **THEN** the system exposes a loading state and does not claim that data is empty or missing

#### Scenario: Fail to load a data screen
- **WHEN** a recoverable load request fails
- **THEN** the system presents an error with retry and does not present first-use actions based on an empty collection

### Requirement: Mutating actions communicate busy and result state
The system SHALL disable conflicting actions while a mutation is running and SHALL expose success or failure using semantics appropriate to the result.

#### Scenario: Save or generate over a delayed connection
- **WHEN** a save, generation, import, or assignment request is running
- **THEN** the initiating action communicates progress and cannot be submitted repeatedly

### Requirement: Core forms support keyboard submission and field relationships
The system SHALL use form submission semantics for primary form actions and SHALL provide programmatic labels and error relationships for dynamic fields.

#### Scenario: Submit a focused single-purpose form with Enter
- **WHEN** the user presses Enter in import, plan creation, or manual shopping item input
- **THEN** the system submits that form once when its data is valid

#### Scenario: Correct an invalid recipe editor submission
- **WHEN** recipe validation fails
- **THEN** the system identifies invalid fields, relates messages to those fields, and moves focus to the first invalid field

### Requirement: Navigation and visual preferences remain accessible
The system SHALL provide one primary page heading, a meaningful page title, visible but non-field-like navigation focus, sufficient control contrast, and reduced motion when requested.

#### Scenario: Navigate between primary screens with assistive technology
- **WHEN** route navigation completes
- **THEN** focus moves to the page's primary heading and the document title identifies the current screen

#### Scenario: Request reduced motion
- **WHEN** the user's platform requests reduced motion
- **THEN** non-essential transitions and animations are disabled

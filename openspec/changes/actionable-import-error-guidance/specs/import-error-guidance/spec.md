# Recipe import error guidance

## ADDED Requirements

### Requirement: Stable error categories for create-import failures
The API SHALL return a bounded machine-readable failure code with the existing safe message for validation, unsupported sources, fetch failure and extraction failure, without including source URLs, HTML or exception details.

#### Scenario: Timed-out source fetch
- **WHEN** page fetching times out without caller cancellation
- **THEN** API returns a timeout code and safe failure status, without including the URL.

### Requirement: Localized actionable UI guidance
The Web client SHALL map known codes to Polish messages with a next action (retry, correct URL or choose another source); unknown codes SHALL not expose arbitrary server text.

#### Scenario: Unsupported source
- **WHEN** a URL belongs to an unsupported domain
- **THEN** the user receives guidance to choose a supported source.

### Requirement: Cancellation remains cancellation
The import service SHALL NOT turn caller cancellation into a normal import failure.

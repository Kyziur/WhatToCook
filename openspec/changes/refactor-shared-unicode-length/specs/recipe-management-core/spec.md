## MODIFIED Requirements

### Requirement: Recipe text limits match PostgreSQL Unicode character limits

The system SHALL enforce configured length limits on trimmed recipe text by Unicode scalar values, not UTF-16 code units, with the same accepted maximum as PostgreSQL varchar columns.

#### Scenario: Accept supplementary-plane characters within the limit
- **WHEN** an otherwise valid recipe contains a 200-scalar title with supplementary-plane characters
- **THEN** the recipe can be persisted successfully

#### Scenario: Reject a title beyond the limit
- **WHEN** an otherwise valid recipe title contains 201 Unicode scalar values after trimming
- **THEN** the API returns a controlled validation error without a partial write

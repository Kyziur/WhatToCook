## ADDED Requirements

### Requirement: Recipe input length validation matches database character boundaries
The system SHALL validate recipe title, ingredient names, units, quantity text, source, steps and tags by Unicode scalar values after trimming, consistent with their PostgreSQL character-length limits.

#### Scenario: Accept supplementary characters at maximum database-supported length
- **WHEN** a user creates a valid recipe whose title and ingredient name contain 200 Unicode scalar values including supplementary characters
- **THEN** the API stores and returns the recipe successfully

#### Scenario: Reject overlong supplementary text before persistence
- **WHEN** a user creates a recipe with a title or ingredient name containing 201 Unicode scalar values
- **THEN** the API returns a controlled validation response and does not persist that recipe

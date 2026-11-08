## ADDED Requirements

### Requirement: Repository audit tracker counts remain verifiable during agent changes
The repository SHALL fail its standard pull request CI if the live audit summary does not match the 69 original, uniquely identified finding rows, using the established DONE, PARTIAL, IN REVIEW and OPEN / CONDITIONAL classifications.

#### Scenario: A status is changed without updating totals
- **WHEN** a contributor changes an audit status but leaves the old counts in the header
- **THEN** the fast audit contract test fails before expensive application tests

#### Scenario: Partial item is under review
- **WHEN** a PARTIAL finding is marked PARTIAL · IN REVIEW
- **THEN** the finding remains counted as partial and is not double-counted as an additional review item

#### Scenario: Audit row is duplicated or removed
- **WHEN** the tracker contains more or fewer than its original 69 finding rows, or the same finding ID twice
- **THEN** CI fails

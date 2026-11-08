## ADDED Requirements

### Requirement: Substring title query performance can be evaluated without changing production search behavior
Maintainers SHALL be able to compare representative PostgreSQL count and bounded-page title search query plans at 1,000 and 10,000 synthetic recipes, with the current database indexes and with a temporary test-only trigram index.

#### Scenario: A rare title substring has measurable plans at both catalog sizes
- **WHEN** the diagnostics run on a PostgreSQL Testcontainers catalog of 1,000 and then 10,000 active recipes
- **THEN** they report PostgreSQL execution time, buffer usage and scan strategy for the selective substring before and after creating a disposable trigram index

#### Scenario: Search semantics are not changed by the diagnostic index
- **WHEN** the same catalog is queried with and without the disposable trigram index
- **THEN** the match count and ordered first page IDs remain equal, and the HTTP search endpoint still returns a bounded set of matching recipes

#### Scenario: Short or common phrases do not force an index preference
- **WHEN** diagnostics search for common and two-character substrings
- **THEN** their plans are reported without requiring a specific index/scan choice or hard-coded latency threshold

#### Scenario: Production schema remains unchanged
- **WHEN** the diagnostics complete or fail
- **THEN** no extension or index is added to the application's production migrations; all benchmark DDL stays in the disposable test database

## ADDED Requirements

### Requirement: Shopping list recommendations remain rank-equivalent under candidate selection
The system SHALL retain the existing recommendation eligibility and ranking rules when candidate loading is optimized.

#### Scenario: Exclude recipes without any possessed ingredient
- **WHEN** a meal plan has at least one non-manual shopping item in state MAM
- **THEN** an optimized candidate query may discard unplanned active recipes with no ingredient whose normalized name matches an owned item, without changing the recommended items

#### Scenario: Preserve score, coverage, and cutoff after filtering candidates
- **WHEN** qualifying recipes differ in matched-ingredient count, duplicate ingredient names, total distinct ingredient count and title
- **THEN** recommendations still use the original 30% coverage threshold and matched-count/coverage/title ranking, with the limit applied after ranking all eligible candidates

#### Scenario: Preserve planned and inactive exclusions
- **WHEN** a candidate recipe is inactive or already planned
- **THEN** it cannot be recommended even when it shares a possessed ingredient

### Requirement: Candidate materialization diagnostics are repeatable
Maintainers SHALL be able to compare the original and SQL-filtered recommendation candidate materialization at synthetic catalog sizes of 100, 1,000 and 10,000 recipes using PostgreSQL.

#### Scenario: Compare candidate transfer sizes
- **WHEN** the benchmark runs on the same catalog with and without candidate filtering
- **THEN** it records the recipe count, ingredient-link count, elapsed time and approximate allocation and verifies the ranked top recommendation IDs agree

#### Scenario: Avoid unreliable latency gates
- **WHEN** the diagnostics run in shared CI
- **THEN** the test does not fail based on raw elapsed time or noisy process-wide allocation deltas

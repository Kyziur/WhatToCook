## ADDED Requirements

### Requirement: Shopping-list aggregation preserves deterministic calculations independently of persistence

The system SHALL compute generated ingredient rows from a meal plan using the same normalized ingredient, unit, amount and multiplier rules before transaction-protected reconciliation with existing checklist items.

#### Scenario: Numeric and descriptive contributions are combined without database access
- **WHEN** recipe ingredients include numeric amounts, descriptive amounts and name-only contributions across entries with different plan multipliers
- **THEN** the generated shopping rows preserve the calculated numeric totals, descriptive occurrence and multiplier context, and name-only context without requiring persistent storage

#### Scenario: Unit boundaries remain stable
- **WHEN** a normalized ingredient is supplied in distinct units, including comma-decimal numeric amounts
- **THEN** the system keeps each normalized unit separate and computes numeric values consistently

#### Scenario: Regeneration stays transactional and non-destructive
- **WHEN** a plan regenerates a previously saved shopping list
- **THEN** the service continues to coordinate the transaction, locking, original row matching, retained checklist state and manual items independently of pure aggregation

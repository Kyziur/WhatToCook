## ADDED Requirements

### Requirement: Descriptive ingredient aggregation retains plan contribution context
The system SHALL preserve original nonnumeric ingredient quantity phrases while exposing repeated contributions and meal-plan multipliers without treating those phrases as numeric measures.

#### Scenario: Two descriptive contributions with different multipliers
- **WHEN** a plan includes two recipes contributing a "szczypta" ingredient with multipliers two and one
- **THEN** one generated list entry retains the verbatim "szczypta" phrase and visibly indicates two occurrences and combined multiplier three, including in copied text

#### Scenario: Repeated unquantified ingredients
- **WHEN** multiple planned recipes contribute the same ingredient without quantity or unit
- **THEN** the list retains one item with an explicit repeated-contribution annotation

#### Scenario: Numeric quantities remain arithmetic
- **WHEN** planned recipes contribute numeric quantities in the same unit
- **THEN** their quantities continue to be multiplied by plan factors and summed numerically, without a descriptive-context annotation

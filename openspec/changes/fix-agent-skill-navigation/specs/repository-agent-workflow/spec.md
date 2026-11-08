## ADDED Requirements

### Requirement: Repository guidance references available agent skills

Maintained, task-specific repository skill instructions SHALL NOT direct a contributor to a nonexistent locally installed named skill.

#### Scenario: Test execution skill routes to existing guidance
- **WHEN** a contributor follows the `run-tests` skill to detect a test platform, choose filter syntax, or debug a failing test
- **THEN** the skill supplies its own documented rules or references an installed skill or canonical repository file

#### Scenario: Broken named skill reference is introduced
- **WHEN** the `run-tests` skill introduces a new backtick-quoted named skill reference without a matching `.agents/skills/<name>/SKILL.md`
- **THEN** the agent-tooling contract check fails in pull-request CI

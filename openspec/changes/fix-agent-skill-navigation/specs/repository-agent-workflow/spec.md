## ADDED Requirements

### Requirement: Repository guidance references available agent skills

Maintained, task-specific repository skill instructions SHALL NOT direct a contributor to a nonexistent locally installed named skill.

#### Scenario: Test execution skill routes to existing guidance
- **WHEN** a contributor follows the `run-tests` skill to detect a test platform, choose filter syntax, or debug a failing test
- **THEN** the skill supplies its own documented rules or references an installed skill or canonical repository file

#### Scenario: Broken named skill reference is introduced
- **WHEN** the `run-tests` skill introduces a new backtick-quoted named skill reference without a matching `.agents/skills/<name>/SKILL.md`
- **THEN** the agent-tooling contract check fails in pull-request CI

### Requirement: Agent test commands select CLI syntax by command mode, not SDK alone

The repository's test guidance SHALL distinguish VSTest command mode (including its optional MTP bridge) from native MTP command mode when choosing paths and application flags.

#### Scenario: Default VSTest on .NET 10
- **WHEN** `global.json` contains SDK 10 but no native MTP test runner selection
- **THEN** `dotnet test WhatToCook.slnx` and other VSTest-mode positional paths remain valid

#### Scenario: Native MTP or VSTest bridge
- **WHEN** native MTP is selected explicitly on SDK 10, or MTP is reached from VSTest command mode through the bridge
- **THEN** native MTP uses named project flags and optional `--`, whereas the bridge retains positional paths and a mandatory `--` before MTP application arguments

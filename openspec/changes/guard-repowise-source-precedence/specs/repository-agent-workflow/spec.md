## ADDED Requirements

### Requirement: Maintained source precedence over generated agent index guidance
The repository SHALL clearly state above its generated Repowise region that current source/specs and live freshness metadata override conflicting generated instructions that would discourage source verification. Agent-tool contract tests SHALL protect that precedence and the generated marker boundary.

#### Scenario: Generated region says not to reread indexed source
- **WHEN** an agent sees a generated claim that verified indexed content never requires rereading
- **THEN** the human-maintained section takes precedence whenever indexed revision, scope or freshness is behind or uncertain

#### Scenario: Regeneration updates generated content
- **WHEN** Repowise regenerates the bounded generated region
- **THEN** the maintained precedence instructions remain outside that region and a CI test detects their accidental removal

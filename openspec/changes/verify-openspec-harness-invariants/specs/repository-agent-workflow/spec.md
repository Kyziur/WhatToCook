## ADDED Requirements

### Requirement: OpenSpec harness adapters preserve shared high-risk safety contracts
The repository SHALL verify the common safety invariants of its OMP and agent-skill entry points for OpenSpec apply and archive without requiring identical harness syntax.

#### Scenario: Critical apply behavior in both entry points
- **WHEN** an OMP command or agent skill is used to apply an OpenSpec change
- **THEN** live context remains required, only fully completed tasks can be checked off, and CLI-controlled blocked/ready/all-done states remain authoritative

#### Scenario: Critical archive behavior in both entry points
- **WHEN** an OMP command or agent skill is used to archive an OpenSpec change
- **THEN** synchronization is evaluated and verified before archiving, in-flight sync is not allowed, and advisory guidance cannot override archive paths

#### Scenario: One adapter drops a guardrail
- **WHEN** one checked-in adapter removes a covered safety invariant
- **THEN** the focused repository CI contract fails before merge, without requiring byte-identical templates

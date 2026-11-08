## ADDED Requirements

### Requirement: Pull requests must enforce dependency audit policy

The repository SHALL fail PR CI when a NuGet or npm audit reports a High or Critical known vulnerability, or when the audit command cannot complete. The NuGet audit SHALL cover direct and transitive dependencies and SHALL reject malformed reports rather than silently treating them as clean.

#### Scenario: A known High or Critical advisory is reported
- **WHEN** a vulnerability of High or Critical severity is present in a NuGet top-level or transitive dependency, or is reported by npm
- **THEN** the PR dependency audit gate fails and the finding is available for diagnosis

#### Scenario: Only lower-severity advisories are reported
- **WHEN** only Low or Moderate NuGet advisories are reported and the audit commands complete
- **THEN** the NuGet gate succeeds and still reports those advisories

#### Scenario: Audit report is malformed or audit command fails
- **WHEN** the vulnerability report cannot be parsed or the audit command fails
- **THEN** CI fails instead of interpreting the scan as clean

#### Scenario: Scan result requires review
- **WHEN** dependency audit steps run on a PR, whether they pass or fail
- **THEN** the available machine-readable reports are retained as CI artifacts

## ADDED Requirements

### Requirement: The optional image-transfer probe is verified through a real browser
The PERF-06 measurement CLI SHALL be exercised in CI against an isolated HTTP image fixture with a decodable photo, a cache validator and a conditional no-body response. The verification SHALL distinguish successful capture from missing-image failure and SHALL not claim real-world optimization benefits.

#### Scenario: First and repeat visits observe a decodable image
- **WHEN** Chromium executes the probe against a fixture serving an image with ETag and 304 revalidation
- **THEN** the report captures first- and repeat-visit image resources, a positive first-transfer count, and a visible decoded image
- **AND** the fixture observes a conditional validator on the repeat visit

#### Scenario: Photo unavailable
- **WHEN** a reachable library has no recipe photo
- **THEN** the probe exits with an error rather than reporting a zero-byte performance baseline

#### Scenario: Bounded image load
- **WHEN** the operator changes the photo load timeout
- **THEN** the CLI validates it as a bounded integer and applies it to the browser's image wait

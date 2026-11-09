## ADDED Requirements

### Requirement: Image-transfer measurement precedes optional thumbnail optimization
The repository SHALL provide an opt-in, locally run browser probe for the Web recipe library that distinguishes cold-context first visits from subsequent visits, reports measured image-resource transfer and visible image resolution, and labels unavailable or scope-limited observations without claiming a production speedup.

#### Scenario: Compare initial and repeat image loads
- **WHEN** an operator runs the probe against a reachable recipe library containing actual photo URLs
- **THEN** the probe reports separate first-visit and repeat-visit image resource counts, transferred bytes (including headers), visible image resolution ratios and browser LCP observations

#### Scenario: No usable photo or unreachable Web
- **WHEN** the configured page cannot load a library image
- **THEN** the probe fails with a diagnostic rather than emitting a misleading successful zero-cost baseline

#### Scenario: No change to production images
- **WHEN** the measurement helper and CLI are present
- **THEN** production image storage, API routes, cache behavior and deployment configuration remain unchanged

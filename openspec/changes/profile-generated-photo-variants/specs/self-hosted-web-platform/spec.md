## ADDED Requirements

### Requirement: Generated images can profile candidate photo-size variants without changing production storage
The PERF-06 E2E benchmark SHALL create between 30 and 50 temporary diverse, decodable images in isolated storage. It SHALL upload originals and 640/320 px versions through the production API contract and measure the same filtered library pages in desktop/mobile Chromium viewports without mocking image HTTP responses.

#### Scenario: Real endpoint comparison
- **WHEN** 36 temporary recipes have been created with generated JPEG, PNG and WebP source images
- **THEN** each source and candidate variant can be uploaded to the existing recipe photo API
- **AND** browser resource entries are captured for both library pages on first and return visits for all three variants

#### Scenario: Isolation and cleanup
- **WHEN** the benchmark ends or asserts unsuccessfully
- **THEN** created recipes are archived and the Playwright-owned storage directory is removed on harness shutdown
- **AND** the artifact contains aggregates only, with no recipe title, individual photo path or private URL

#### Scenario: Diagnostic interpretation
- **WHEN** the benchmark finishes
- **THEN** it reports fixture sizes, browser transfer totals and limited LCP observations
- **AND** it does not claim synthetic samples prove representative production speed or justify thumbnails without quality evaluation

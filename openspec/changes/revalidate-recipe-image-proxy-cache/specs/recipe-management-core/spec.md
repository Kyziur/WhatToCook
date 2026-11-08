## ADDED Requirements

### Requirement: Recipe photos support conditional revalidation through the Web proxy
The browser-facing recipe-photo proxy SHALL preserve upstream image cache validators, forward browser conditional requests, and return an unmodified HTTP 304 status without image bytes when the upstream API reports that the file is unchanged. Images SHALL be stored only in a private browser cache that requires revalidation.

#### Scenario: First request for a saved recipe photo
- **WHEN** a browser requests an existing recipe photo without cache validators
- **THEN** it receives HTTP 200, the original image content type, a private revalidation policy and upstream ETag / Last-Modified when provided

#### Scenario: Browser revalidates an unchanged photo
- **WHEN** a browser sends the ETag of a previously downloaded recipe image as If-None-Match
- **THEN** the Web proxy forwards the validator and returns HTTP 304 without a response body

#### Scenario: No image is available
- **WHEN** the upstream API does not have the requested image
- **THEN** the browser-facing route retains the existing not-found behavior

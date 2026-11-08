## Why

OBS-04: the URL-import path swallowed all fetch failures and reported them as unsupported, erasing diagnostic distinctions between timeout, blocked destinations, rejected response, DNS, and HTTP issues.

## What Changes

- Preserve the caller's cancellation instead of converting it into a regular unsupported-source response.
- Classify expected fetch failures into bounded, non-sensitive internal codes, and log adapter key plus duration only.
- Explicitly mark blocked hosts, DNS validation, and invalid content through a typed fetch rejection instead of freeform messages.
- Record an extraction failure category without including URL, query string, HTML or exception messages.
- Keep the HTTP error contract unchanged for ordinary fetch failures.

## Non-Goals

- Do not add raw URL/HTML or unfiltered exception logging.
- No new telemetry backend, retry strategy, persistence changes, or user-facing API schema.

## Verification

Integration coverage for timeout, HTTP status, network and unexpected fetch errors; normal successful import contract; release CI and code review.

# PERF-06 browser probe — evidence

## Verified design boundary
The probe is now invoked from a Node test against a test-only HTTP fixture returning a small decodable SVG via `/recipe-images/`. The fixture returns ETag and conditional HTTP 304 on subsequent requests. The negative path omits photos. No production image bytes or actual household endpoints are used.

## Verification
Pending final-head CI and diff review. Passing synthetic probe checks will verify measurement *functionality*, not actual photo size, mobile hardware performance, LCP p95, thumbnail necessity or a production speedup.

# PERF-06 (partial) — Local real-photo image transfer measurement

## Why
The cache revalidation transport regression is verified, but first-visit photo transfer cost and source-vs-display resolution remain unknown. Thumbnails have additional lifecycle and ARM64 deployment cost; adding them without a representative baseline is speculative.

## Change
- Add an **opt-in** Playwright Chromium measurement CLI operating against a user-supplied Web library URL with real images.
- Capture first and repeat visit ResourceTiming transfer/header-byte observations, viewport-visible photo dimension ratios, and one LCP observation, without logging user photos or titles.
- Add deterministic Node tests for input validation, resource filtering, and byte counting; document limitations and next decision gate.

## Non-goals
No photo derivative format/encoding change, storage migration, automatic collection of household images, remote telemetry, authentication bypass, CI claim of real image measurements, or performance threshold enforcement.

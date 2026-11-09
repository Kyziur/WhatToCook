# PERF-06 generated profile — test traceability

| Contract | Verification |
| --- | --- |
| 36 generated JPEG/WebP/PNG sources within 5 MiB each | `generatePhoto`, `uploadVariant`, API photo validation, `@perf06-generated` |
| Source and 640/320 variants delivered by real Web image HTTP | `@perf06-generated` ResourceTiming first/repeat capture with no route mocks |
| Both library pages represented (24+12) | `runVisit` full photo resource assertions |
| Consistent desktop/mobile variant cohorts | `profileVariant` browser contexts and controlled recipe IDs |
| Temporary DB and image storage removed | Existing Playwright database isolation, E2E runner shutdown cleanup, recipe archive `finally` |
| No arbitrary latency cutoff or false production claim | Diagnostic-only CI artifact, docs, and OpenSpec review |

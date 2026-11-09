# PERF-06 generated profile — test traceability

| Contract | Verification |
| --- | --- |
| 36 generated JPEG/WebP/PNG sources within 5 MiB each | `generatePhoto`, `uploadVariant`, API photo validation, `@perf06-generated` |
| Source and 640/320 variants delivered by real Web image HTTP | `@perf06-generated` ResourceTiming first/repeat capture with no route mocks |
| All 36 images represented across two pages (24+12) | `runVisit` full ResourceTiming assertions; [CI 42641175623](https://github.com/Kyziur/WhatToCook/actions/runs/42641175623) green |
| 30–50 fixture range supports a third catalog page | `photoCountsByPage` regression for 30/36/48/50; smoke suite 100 passed |
| Consistent desktop/mobile variant cohorts | `profileVariant` browser contexts and controlled recipe IDs |
| Temporary DB and image storage removed | Existing Playwright database isolation, E2E runner shutdown cleanup, recipe archive `finally` |
| No arbitrary latency cutoff or false production claim | Diagnostic-only CI artifact, docs, and OpenSpec review |

Full final-head run: [CI 42641175623](https://github.com/Kyziur/WhatToCook/actions/runs/42641175623) — 126 API, 65 component, 100 Playwright, 36-photo generated benchmark and ARM64 green. No production image pipeline changes.

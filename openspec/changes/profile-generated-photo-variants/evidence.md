# PERF-06 generated image profiling — evidence

## Final merged change

- [PR #55](https://github.com/Kyziur/WhatToCook/pull/55) squash-merged as `389564e3cebfc32ac196cbe6e99f45fb940b5a21`.
- Final-head [CI 42641175623](https://github.com/Kyziur/WhatToCook/actions/runs/42641175623): **both jobs green**, with **126 API integration tests**, **65 Blazor component tests**, **100 Playwright smoke tests**, dedicated successful **36-photo/3-variant/2-viewport browser benchmark** and successful **ARM64 image upload smoke**. Existing agent/security/dependency gates passed.
- Initial run [CI 42639512596](https://github.com/Kyziur/WhatToCook/actions/runs/42639512596) was also green. Review discovered that a requested optional sample count of 49–50 exceeded the initial two-page capture logic; a **separate correction commit `3378ee6`** generalized pagination and added non-network regression checks for 30/36/48/50 photo partitioning before the final run.
- Final CI artifact: `perf06-generated-image-metrics` (contains aggregates and six profiles, no photo paths, recipe titles or host addresses).

## Measured first/repeat transfer totals (all 36 resources)

| Cohort | First visit, browser `transferSize` | Repeat visit, browser `transferSize` | First-visit reduction vs original |
| --- | ---: | ---: | ---: |
| Original (mixed dimensions and JPEG/PNG/WebP) | 34,882,939 B (33.27 MiB) | 10,800 B | — |
| 640 px | 3,998,774 B (3.81 MiB) | 10,800 B | 88.54% |
| 320 px | 1,509,769 B (1.44 MiB) | 10,800 B | 95.67% |

The controlled fixtures' encoded image bodies total **34,872,139 B**, **3,987,974 B**, and **1,498,969 B**, respectively. These match the initial browser download sizes after subtracting header bytes. Desktop and mobile viewport measurements of all 36 resources produced the same byte totals; UI LCP timing differed and is not stable enough for a gating threshold.

## Evidence boundaries and outcome

All generated files were uploaded through existing production API endpoints to a one-run isolated Playwright PostgreSQL database and image storage, and measured through the actual Web proxy. The test archives its created recipes; E2E harness shutdown removes dedicated storage. No app production image-processing logic was modified.

This is a **procedural raster fixture**, not representative real photos. No visual quality comparison, image optimization/deletion lifecycle, source-specific edge cases, physical phone/network experiment, p95 or real-image size distribution is provided. Therefore PERF-06 remains **PARTIAL · CI**; the numbers justify evaluating a responsive image variant plan, not immediately deploying one fixed thumbnail size.

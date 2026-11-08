# PERF-05 verification evidence (2026-11-08)

## Final-head CI evidence before documentation updates

- Pull request: [#34](https://github.com/Kyziur/WhatToCook/pull/34)
- Tested implementation commit: `abd947a55c56e56065ed55cd4270213d4f31a60a`
- [PR CI run 42505894142](https://github.com/Kyziur/WhatToCook/actions/runs/42505894142): **success**. Both `verify` and `ARM64 recipe image upload smoke` completed successfully.
- Dedicated opt-in Playwright `@perf05` passed with real browser tabs and live built-in runtime meters. Standard desktop/mobile Playwright smoke reported 98 passed and 4 skipped (the opt-in perf test is skipped in routine suites).

| Observed on shared CI runner | 5 tabs | 20 tabs |
| --- | ---: | ---: |
| Connected circuits peak | 5 | 20 |
| Active circuits peak | 5 | 20 |
| Process working set (MB) | 141.4 | 174.4 |
| Phase process allocations (MB) | 9.1 | 24.4 |
| Gen0 / Gen1 / Gen2 collections | 1 / 0 / 0 | 1 / 0 / 0 |
| Render samples | 45 | 180 |
| Server render-diff p50 (ms) | 11.85 | 15.26 |
| Server render-diff p95 (ms) | 51.66 | 77.16 |
| Playwright search-to-visible-DOM p50 (ms) | 107.5 | 92.6 |
| Playwright search-to-visible-DOM p95 (ms) | 113.9 | 107.5 |

Actual histogram instrument: `Microsoft.AspNetCore.Components.Lifecycle/aspnetcore.components.render_diff.duration` (`Histogram<double>`); circuit metrics are `UpDownCounter<long>`. The profiler accepts both documented and runtime-observed render metric spellings.

The five-tab cohort's connected circuits drained to zero before the twenty-tab cohort. The test asserts functionality and metric visibility, **not** arbitrary latency/memory thresholds.

## Risk-based review

- Production safeguard: `BlazorCircuitProfiler` is only DI-registered when `IsDevelopment()` and explicit output/phase paths are supplied. No metrics HTTP endpoint added.
- No change to circuit retention/reconnect, page state model, component structure, DB schema, or API contracts.
- No conclusion about per-user memory, production p95, or actual family concurrency: working set and allocations are whole-process; this is **one shared-runner sample**.
- No optimization justified yet. Repeat at least three runs and capture homelab data before changing circuit budgets or splitting components.

## Outstanding

- CI must be rerun for the final documentation-only commit(s) before merging.
- Operational/homelab profiling remains optional/conditional and is **not** represented as completed.
- Independent PR review remains separate from this author-side verification.

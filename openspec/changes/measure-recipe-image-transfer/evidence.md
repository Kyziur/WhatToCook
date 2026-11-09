# PERF-06 partial — image probe evidence

- Prior [#41](https://github.com/Kyziur/WhatToCook/pull/41) verified the Web proxy's conditional headers/304 behavior; this is not a first-visit performance baseline.
- [PR #45](https://github.com/Kyziur/WhatToCook/pull/45), squash merge `f660189b9a8f2cf2ac0789bf59f287d3fefb381a`.
- **Final-head [CI run 42599858495](https://github.com/Kyziur/WhatToCook/actions/runs/42599858495) passed**: Node image metric helper tests and CLI syntax check, 126 API, 63 component, 98 browser, ARM64 storage smoke.
- Opt-in Playwright CLI requires a loaded real photo before reporting two visits. It collects aggregate ResourceTiming transfer metrics, visible source-to-display ratios and one browser LCP value without photo names/paths. Source diff review confirms production photo storage and proxy remain untouched.

## Measurement limits and follow-up
**No real-photo, slower-network, mobile hardware, household host or comparative LCP measurements have yet been performed.** A successful CI run does not establish a speedup or the need for thumbnails. The CLI's viewport is not an actual phone. Representative first/repeat transfer measurement must precede any thumbnail decision. PERF-06 remains **PARTIAL · CI**. No independent human review is recorded.

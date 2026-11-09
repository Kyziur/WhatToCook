# PERF-06 — browser probe evidence

## Final merged change
- [PR #48](https://github.com/Kyziur/WhatToCook/pull/48) was squash-merged as `86109a0f123a74217ae30c57f3453b1f3066ea63`.
- Final-head [CI 42616962766](https://github.com/Kyziur/WhatToCook/actions/runs/42616962766): **verify and ARM64 upload smoke both passed**.
- Main CI: 126 API integration tests, 65 Blazor component tests, 98 Playwright browser tests, with the existing diagnostics, agent tests and dependency audit gates passing.
- New **actual Chromium** fixture step: **2/2 tests passed**. One verifies a decodable synthetic image with first-visit resource count and positive transfer, then repeat-visit ETag revalidation yielding 304 at the fixture; the other verifies an empty photo library causes an error instead of a fabricated zero-byte baseline. Output checks confirm image path/fixture host do not appear in JSON.
- Bounded `--load-timeout-ms` parsing is covered by Node tests. Source diff contains only scripts, tests, OpenSpec, docs and CI; production image/API/database paths are unchanged.

## Limits
The image is a local synthetic SVG fixture; it verifies **probe functionality**, not photo upload formats or household image bytes. No representative real-photo first/repeat network captures, mobile hardware observations or LCP comparisons have been made. CI results do not justify introducing 320/640 px variants. No separate independent human review is claimed. PERF-06 remains **PARTIAL · CI** until the manually reproducible homelab baseline is collected.

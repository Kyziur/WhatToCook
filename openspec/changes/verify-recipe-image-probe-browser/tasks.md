# PERF-06 browser probe — tasks

- [x] Re-review current probe and locate the missing real-browser CI coverage.
- [x] Add an isolated HTTP fixture with decodable image and conditional ETag/304.
- [x] Add CLI subprocess tests for initial/repeat capture, lack of URL leakage and missing-photo failure.
- [x] Expose a bounded image-load timeout and cover parser validation.
- [x] Run targeted browser probe in CI after Chromium is installed.
- [x] Update performance usage documentation and OpenSpec traceability.
- [x] Review focused browser-test and full CI results; [final-head CI 42616962766](https://github.com/Kyziur/WhatToCook/actions/runs/42616962766) green.
- [x] Merge [#48](https://github.com/Kyziur/WhatToCook/pull/48) after green CI and synchronize audit/evidence in this documentation follow-up.
- [ ] Run real-photo baseline on the homelab before deciding whether to generate thumbnails (tracked under `measure-recipe-image-transfer`).

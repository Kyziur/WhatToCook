# PERF-06 partial — verified image proxy revalidation

- PR [#41](https://github.com/Kyziur/WhatToCook/pull/41), merged as `5895b1810a7ca06605ce9170d7824f3fa127dc49`.
- Final-head [CI run 42527840882](https://github.com/Kyziur/WhatToCook/actions/runs/42527840882): **verify + ARM64 smoke successful**.
- CI: **122 API integration**, **56 Blazor component**, **98 Playwright**; separate 1-case PERF-05 diagnostic passed, and agent + dependency gates passed.
- The existing photo-upload Playwright scenario now verifies a browser-facing first photo response **200 + ETag + private/no-cache** followed by an explicit conditional GET **304 + same ETag + zero response bytes**.
- Source review: only selected validator headers are passed to the API; upstream ETag and Last-Modified are copied; upstream 304 is no longer mapped to 404; original file storage and upload contracts are unchanged.

## Limits and remaining work

No production/photo library trace, mobile data transfer baseline, device LCP or real-world byte reduction was measured. These are required before deciding whether to create 320/640px photo variants. Mark PERF-06 as **PARTIAL · CI**, not complete. No independent human review is recorded; source diff and CI checks are author-side verification.

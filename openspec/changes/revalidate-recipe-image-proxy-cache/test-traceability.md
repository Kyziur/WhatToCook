# PERF-06 partial traceability

| Requirement | Verification | State |
| --- | --- | --- |
| Browser image GET returns ETag and private revalidation policy | `tests/e2e/recipe-management-core.spec.ts` upload-photo scenario | [CI 42527840882](https://github.com/Kyziur/WhatToCook/actions/runs/42527840882) passed |
| If-None-Match yields 304 without bytes via Web/API | Same scenario, `page.request.get` conditional HTTP GET | [CI 42527840882](https://github.com/Kyziur/WhatToCook/actions/runs/42527840882) passed |
| Recipe details still renders uploaded photo | Existing browser scenario and API native-format tests | [CI 42527840882](https://github.com/Kyziur/WhatToCook/actions/runs/42527840882) passed |
| Resizing decisions are based on representative user photos | Manual network baseline procedure in `docs/performance/recipe-image-transfer.md` | Not collected; no variants |

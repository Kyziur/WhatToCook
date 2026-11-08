# PERF-06 partial traceability

| Requirement | Verification | State |
| --- | --- | --- |
| Browser image GET returns ETag and private revalidation policy | `tests/e2e/recipe-management-core.spec.ts` upload-photo scenario | PR CI pending |
| If-None-Match yields 304 without bytes via Web/API | Same scenario, `page.request.get` conditional HTTP GET | PR CI pending |
| Recipe details still renders uploaded photo | Existing browser scenario and API native-format tests | PR CI pending |
| Resizing decisions are based on representative user photos | Manual network baseline procedure in `docs/performance/recipe-image-transfer.md` | Not collected; no variants |

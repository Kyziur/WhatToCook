# PERF-06 partial — traceability

| Requirement | Verification | Status |
| --- | --- | --- |
| Input requires a local-run HTTP(S) Web URL, bounded settle duration | `tests/ci/image-transfer-metrics.test.mjs` | Pending CI |
| Resource bytes and viewport image ratios are aggregated correctly | `tests/ci/image-transfer-metrics.test.mjs` | Pending CI |
| No photo means no successful performance report | Browser probe checks recipe-photo element; empty helper counts remain unavailable | Real browser with photos pending |
| Cold context vs repeat visit | Playwright isolated context + same-page reload | Real browser with photos pending |
| Cache/storage contracts unchanged | Source diff + prior PERF-06 revalidation tests | Pending full CI |

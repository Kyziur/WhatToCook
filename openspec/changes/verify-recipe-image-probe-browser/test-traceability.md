# PERF-06 browser probe — traceability

| Requirement | Verification | Status |
| --- | --- | --- |
| Real Chromium sees initial photo and cache validator on repeat visit | `tests/ci/image-transfer-browser.test.mjs` | Pending PR CI |
| No photo fails explicitly instead of reporting fake zero baseline | `tests/ci/image-transfer-browser.test.mjs` | Pending PR CI |
| Bounded load timeout accepts/rejects values | `tests/ci/image-transfer-metrics.test.mjs` | Pending PR CI |
| Production image behavior remains untouched | Scoped diff (scripts/tests/docs/CI only) and existing full Playwright/API tests | Pending PR CI |

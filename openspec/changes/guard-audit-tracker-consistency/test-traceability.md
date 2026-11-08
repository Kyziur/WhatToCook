# Audit tracker validation traceability

| Requirement | Verification | State |
| --- | --- | --- |
| Original findings retained exactly once | `tests/ci/audit-tracker.test.mjs` uniqueness/count case | CI pending |
| Header equals actual status totals | `tests/ci/audit-tracker.test.mjs` summary case | CI pending |
| Partial in review not double-counted | `tests/ci/audit-tracker.test.mjs` classification case | CI pending |
| Check is fast and pre-build | `.github/workflows/pr-ci.yml` order and CI | CI pending |

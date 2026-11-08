# Audit tracker validation traceability

| Requirement | Verification | State |
| --- | --- | --- |
| Original findings retained exactly once | `tests/ci/audit-tracker.test.mjs` uniqueness/count case | [CI 42531555728](https://github.com/Kyziur/WhatToCook/actions/runs/42531555728) passed |
| Header equals actual status totals | `tests/ci/audit-tracker.test.mjs` summary case | [CI 42531555728](https://github.com/Kyziur/WhatToCook/actions/runs/42531555728) passed |
| Partial in review not double-counted | `tests/ci/audit-tracker.test.mjs` classification case | [CI 42531555728](https://github.com/Kyziur/WhatToCook/actions/runs/42531555728) passed |
| Check is fast and pre-build | `.github/workflows/pr-ci.yml` order and CI | [CI 42531555728](https://github.com/Kyziur/WhatToCook/actions/runs/42531555728) passed |

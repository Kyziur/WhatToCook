# ARC-05 recipe API/Web consumer compatibility evidence

- PR: [#38](https://github.com/Kyziur/WhatToCook/pull/38); merged at `56c6fda880fa7d31afcbe3e2c7cdd68738e2808e`.
- Final-head [CI run 42530745977](https://github.com/Kyziur/WhatToCook/actions/runs/42530745977) **success**, both regular `verify` and ARM64 image-storage smoke.
- **126 API integration tests**, **63 Blazor component tests**, **98 Playwright tests**, Node and dependency audit checks passed.
- The recipe catalog/details Web client uses `[JsonRequired]` on required JSON properties and explicit checks against present-but-null text/collections. It also enforces returned page/pageSize identity.
- Negative Web tests cover omitted total count, recipe summary title, recipe steps, nested ingredient name and null page items/steps; positive PostgreSQL-backed API test validates actual response field names/types.
- Review correction: `[JsonRequired]` alone does not reject present-but-null properties. Added client guards and regression tests; fixed CSharpier CI failure in a separate commit.

## Scope and limitations

This supplements [ARC-05 planner coverage (#40)](https://github.com/Kyziur/WhatToCook/pull/40), not a new audit finding. The API and Web remain independently deployable with feature-local DTOs; no shared contract assembly or database migration was added. This verifies selected recipe consumers only, not all API routes or independent deployment-version combinations. Review was author-side plus CI; **no independent human review is recorded**.

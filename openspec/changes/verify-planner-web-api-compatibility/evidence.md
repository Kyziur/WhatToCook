# ARC-05 — Web/API compatibility evidence (2026-11-08)

- PR: [#40](https://github.com/Kyziur/WhatToCook/pull/40), merged as `2b3244de52a1e2bfc326e9204c0e2087c01825c2`.
- Final implementation/head CI: [42525322218](https://github.com/Kyziur/WhatToCook/actions/runs/42525322218) **success**, including ARM64 storage smoke.
- API integration tests: **122 passed**, including `MealPlanWebApiContractTests`; component tests: **56 passed**; desktop/mobile Playwright smoke: **98 passed**.
- CI also passed CSharpier formatting, build, database query diagnostics, PERF-05 Blazor profiling, agent-config Node tests and dependency gates.

## Exercised client contract

The test invokes the real Blazor `ApiMealPlanService` against the real ASP.NET Core API with PostgreSQL Testcontainers. It validates plan creation, summaries, details, a saved recipe entry including nested DTO fields and decimal multiplier 1.5, persisted read-back, regeneration-required HTTP conflict, confirmed regenerate/save, and final persisted plan.

## Findings corrected during implementation

- Referencing both API and Web from the integration-test project introduced two public `Program` types; the Web project reference is now scoped via MSBuild `Aliases=WhatToCookWeb` and `extern alias` in the focused test only.
- CSharpier formatting corrections were applied for the project-reference metadata and long deserialization expression.
- The API and Web application DTOs remain feature-local. The only new dependency on Web is in the integration-test project.

## Limitations

This is a real client/API integration scenario for the planner, not a full schema snapshot of every API endpoint. No production application, database schema, or homelab configuration changed. The author performed a scoped diff review and full CI verification; **no separate independent human review is recorded**. Production auth/exposure and restore gates remain outside ARC-05.

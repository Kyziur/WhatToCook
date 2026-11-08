# ARC-03 (partial) — archived plan assignment identity evidence

- PR [#42](https://github.com/Kyziur/WhatToCook/pull/42) merged as `887d8b0dd9e3a4fe9b4f766269f1109a449798f1`.
- Final-head [CI run 42528747845](https://github.com/Kyziur/WhatToCook/actions/runs/42528747845): **verify + ARM64 image smoke successful**.
- CI: **125 API integration**, **56 component**, **98 Playwright**; shared harness Node tests and dependency audit gates also passed.

## Finding and fix

Formerly, a request row could reuse the GUID of an existing plan entry to add a *different* inactive recipe or relocate the *same* archived recipe to another day, because the guard checked only `entry.Id`. The internal pure `MealPlanMutationRules.IsRetainedInactiveAssignment` now requires the same **entry ID, recipe ID and planned date** from the persisted plan.

DB-free test covers null, unknown, swapped recipe and moved date; PostgreSQL-backed HTTP theory verifies move/swap return validation errors while the existing plan stays unchanged. Existing HTTP test also verifies safe preservation of an archived recipe in its original slot, including multiplier edits.

## Scope and limitations

The updated endpoint keeps the existing advisory-lock, database transaction, regeneration conflict/flow and HTTP contracts. No whole-of-endpoint use-case extraction was justified; ARC-03 remains **PARTIAL · CI** until a specific boundary or testability issue demands a larger refactor. Review is author-side and CI-based; no separate independent human review was recorded.

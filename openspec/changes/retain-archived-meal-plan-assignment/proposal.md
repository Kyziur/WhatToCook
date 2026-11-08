# ARC-03 (partial) — Preserve immutable identity of archived plan assignments

## Why
The plan-update endpoint currently treats a request row as an existing historical assignment if it only reuses an existing row GUID. When a recipe has been archived, the caller can therefore use that GUID to substitute another inactive recipe or move the original inactive recipe to a different date, bypassing the policy that archived recipes cannot be newly assigned.

## Change
- Extract the pure historical-assignment identity rule into `MealPlanMutationRules`, a small internal policy near the planning feature.
- A retained inactive assignment must match **row ID, recipe ID and date** from the persisted plan. Name, multiplier and sort order can still be edited.
- Add DB-free policy test and PostgreSQL-backed endpoint tests covering both bypasses, persisted state on failure and retaining historical slots.

## Non-goals
No wholesale service/use-case extraction, transaction changes, lock changes, planner UI changes, DTO redesign or migration. The existing `HasPlanChanged` regeneration check and shopping-list lock remain unchanged.

## Risks
Reject newly reassigned inactive recipes while preserving an archived recipe's original historical day. CI runs existing planner, shopping and browser tests.

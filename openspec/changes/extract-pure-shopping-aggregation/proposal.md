# ARC-02 — Extract pure shopping aggregation

## Why

`ShoppingListService` couples recipe-ingredient aggregation to EF Core persistence and transactional regeneration. The amount/multiplier rules can be tested without PostgreSQL but currently live as private methods on the database service. This is a concrete ARC-02 testability trigger following COR-03/COR-04 and PERF-04.

## What Changes

- Extract the existing deterministic ingredient-to-generated-item calculation into an internal feature-local `ShoppingListAggregation` class.
- Reuse one `ShoppingQuantity` helper for decimal, unit normalization and numeric formatting, rather than duplicating the same rules across aggregation and regeneration.
- Add database-free unit-style xUnit coverage in the existing test project, preserving the HTTP integration tests for persistence and checklist-state reconciliation.
- Leave database transactions, locks, row identity, manual entries and regeneration in `ShoppingListService`.

## Non-goals

No endpoint/DTO/schema change, no new service layer or DI registration, no change to ranking/filtering, amounts, ordering, generated-state retention, and no homelab change.

## Risks

The method extraction must preserve the exact current output and stable matching signatures. Existing end-to-end shopping-list HTTP tests plus new focused tests are the regression gate.

## Why

COR-04: Generated checklist state currently follows only a normalized ingredient-and-unit key, allowing MAM to persist even when an amount is increased or replaced by a descriptive phrase.

## What Changes

- Preserve the state of exactly matched, unambiguous amounts.
- Reset generated checklist state on changed or ambiguous amounts, retaining item identity where safely reusable.
- Persist a short visible reason for every reset, and clear it when the user explicitly changes the status.
- Preserve all manual rows and atomic direct/plan-save regeneration.

## Dependencies and Non-Goals

This PR builds on COR-03's stored descriptive contribution context (PR #15); do not merge first. No Bought, category or multi-device features from the separate Shopping List 2.0 design.

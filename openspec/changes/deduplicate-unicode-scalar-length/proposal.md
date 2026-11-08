## Why

Plan, shopping, and import validators duplicate UTF-16 rune-count loops; the recipe write validator uses UTF-16 `Length` even though PostgreSQL `varchar(n)` counts Unicode code points. Preserve valid supplementary Unicode inputs consistently.

## What Changes

- Reuse one internal Unicode scalar-length helper at API validation boundaries.
- Preserve per-feature error messages and current null/trim semantics.
- Extend PostgreSQL-backed HTTP regression tests for 200/201 supplementary Unicode scalar values.

## Non-Goals

No database migration, normalization-policy change, new shared assembly, or validation framework.

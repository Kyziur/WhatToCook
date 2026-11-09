# ARC-01 (targeted) — Retry a new recipe photo upload without duplicate creation

## Why
Recipe creation and photo upload are separate requests. If creation succeeds and photo upload fails, the editor stays on `/recipes/new`. Retrying its save currently sends a second create request, risking a duplicate/conflict although the first recipe already exists.

## What changes
- Keep the returned recipe ID in editor state immediately after a successful create, before optional photo upload.
- When a user retries the same editor session, send an update for that ID and retry the photo upload. Preserve the pending photo and unsaved state after an upload failure.
- Keep existing recipe editing, success navigation, form validation and upload limits unchanged.
- Cover both a returned upload failure and a thrown upload exception in Blazor component tests.

## Non-goals
No server-side idempotency key for a lost create response, global upload orchestration, backend API change, migration, or unrelated page decomposition. This addresses one concrete ARC-01 save-flow risk, not the whole ARC-01 optional refactor.

## Risk and boundary
The persisted ID is session-local Blazor component state; a circuit loss or page reload after creation is not recovered by this change. Independent write requests cannot guarantee exactly-once creation if the initial create response is lost. The fix must not claim otherwise.

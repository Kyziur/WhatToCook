## Why

Concurrent finalize requests can both read a draft as not finalized before either creates its recipe, leaving ambiguous responses and state.

## What Changes

- Serialize finalization for a given draft at the PostgreSQL row before reading state.
- Reuse one transaction to atomically create a recipe and mark its draft finalized.
- Prove the actual HTTP concurrency contract with ten overlapping requests.

## Non-Goals

No global lock, queue infrastructure, schema migration, or change to unrelated import update APIs.

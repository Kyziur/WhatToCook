## 1. Contract Sign-off — Design Only

- [ ] 1.1 Confirm copy offsets, archived entry resolution, fresh entry IDs/leftover reference rebinding, operation idempotency and preview (WP-01, WP-02)
- [ ] 1.2 Confirm multi-entry slot moves/appends, pairwise placement swaps (including intra-slot position), exclusive No cooking conflicts, replacement multiplier and order (WP-03, WP-04)
- [ ] 1.3 Confirm no-cooking exclusivity, leftover source IDs and earlier-entry validation (WP-05)
- [ ] 1.4 Confirm shopping regeneration and concurrent plan save behavior (WP-06)

## 2. Future Implementation — Not in This PR

- [ ] 2.1 Implement copy/preview with archived resolution and duplicate command protection (WP-01, WP-02)
- [ ] 2.2 Implement targeted replace, move/append and pairwise swap without breaking `SortOrder` encoding (WP-03, WP-04)
- [ ] 2.3 Implement no-cooking/leftover entry types and source-reference/shopping rules (WP-05)
- [ ] 2.4 Implement safe regeneration, versioning and feedback across new edits (WP-06)

## 3. Future Verification and Sync

- [ ] 3.1 Add PostgreSQL tests for copying, fresh target IDs and leftover remapping (including archived source omission), source immutability, inactive recipes, retries, slot ordering and leftovers
- [ ] 3.2 Add Blazor component and E2E tests for preview, replace, append, swap, invalid/exclusive No cooking targets, and cancellation
- [ ] 3.3 Add shopping regeneration rollback and concurrent editor conflict tests
- [ ] 3.4 Replace PENDING rows with automated test names, validate delta and sync canonical spec before archive

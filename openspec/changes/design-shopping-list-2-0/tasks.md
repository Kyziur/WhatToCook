## 1. Design Contract — No Implementation Yet

- [ ] 1.1 Confirm Bought / `MAM` / `NIE MAM` transitions, Finish shopping and recommendations (SL2-01, SL2-02)
- [ ] 1.2 Confirm fixed categories, generated-list prerequisite and manual item behavior (SL2-03, SL2-05)
- [ ] 1.3 Confirm trusted access boundary, <=5s automatic sync and per-item optimistic versions (SL2-04)
- [ ] 1.4 Confirm normalized quantity comparison and state reset behavior (SL2-06)

## 2. Future Implementation — Not in This PR

- [ ] 2.1 Implement purchase indicator, undo, Finish shopping and ingredient recommendations (SL2-01)
- [ ] 2.2 Implement mobile checklist, progress and connected/disconnected UI (SL2-02)
- [ ] 2.3 Implement fixed categories and manual item editing/removal (SL2-03, SL2-05)
- [ ] 2.4 Implement automatic connected-device synchronization and stale-write rejection (SL2-04)
- [ ] 2.5 Implement quantity-safe atomic regeneration in both save paths (SL2-06)

## 3. Future Verification and Sync

- [ ] 3.1 Add PostgreSQL tests for item states, checkout, recommendations, categories, manual duplicates and quantity changes
- [ ] 3.2 Add mobile component/E2E tests for list views, progress, undo, editing and error states
- [ ] 3.3 Add two-device tests for push propagation and stale writes; verify private network access restrictions
- [ ] 3.4 Map implemented scenarios to concrete tests, validate delta and sync canonicals before archive

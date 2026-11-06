## 1. Product and Contract Refinement — Design Only

- [ ] 1.1 Confirm purchase states, `MAM` semantics, undo and shopping-session lifecycle; reconcile SL2-01 and SL2-02 against existing shopping spec
- [ ] 1.2 Specify category defaults, manual-item behavior, inheritance and quantity-change reconciliation for SL2-03, SL2-05 and SL2-06
- [ ] 1.3 Decide authorized household access and same-item conflict resolution for SL2-04
- [ ] 1.4 Decide whether SL2-07 offline is a launch gate and define the client-side storage/sync architecture if approved

## 2. Future Delivery Packages — No Implementation Yet

- [ ] 2.1 Implement persisted Bought state, undo, shopping progress and mobile-first UI (SL2-01, SL2-02)
- [ ] 2.2 Implement department grouping, category editing and manual-entry lifecycle (SL2-03, SL2-05)
- [ ] 2.3 Implement server-authoritative multi-device synchronization, conflict handling and access control (SL2-04)
- [ ] 2.4 Implement non-destructive regeneration including quantity-change policy (SL2-06)
- [ ] 2.5 Implement offline read/write/sync only if the explicit release decision includes SL2-07

## 3. Future Verification and OpenSpec Sync

- [ ] 3.1 Add PostgreSQL-backed integration tests for purchase states, category persistence, manual duplicates and atomic regeneration
- [ ] 3.2 Add Blazor component and phone-viewport E2E tests for shopping progress, grouping, undo and error/empty states
- [ ] 3.3 Add controlled multi-device race and access tests; add offline/network-transition tests if SL2-07 is approved
- [ ] 3.4 Update scenario-level test-traceability from `PENDING` with concrete test names and verify the delta against canonical specifications before archive

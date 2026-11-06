## 1. Product Contract and Design — No Implementation Yet

- [ ] 1.1 Confirm date-offset copying, naming, range and source selection rules for WP-01 and WP-02
- [ ] 1.2 Confirm replace/move/swap collision and accessible mobile interaction rules for WP-03 and WP-04
- [ ] 1.3 Specify leftover reference validation and no-cooking shopping semantics for WP-05
- [ ] 1.4 Validate shopping-list regeneration, concurrency and cancellation contract for WP-06
- [ ] 1.5 Decide if WP-07 saved combinations remain stretch scope

## 2. Future Implementation Slices

- [ ] 2.1 Add plan-copy API/domain logic and preview with independent target identity (WP-01, WP-02)
- [ ] 2.2 Implement replacement, move and swap controls with existing plan validations (WP-03, WP-04)
- [ ] 2.3 Add genuine `No cooking` and `Leftovers` plan-entry types and shopping calculation rules (WP-05)
- [ ] 2.4 Preserve atomic regeneration flow and safe save behavior across new edits (WP-06)
- [ ] 2.5 Add reusable meal combinations only if independently approved (WP-07)

## 3. Future Tests and Traceability

- [ ] 3.1 Add PostgreSQL-backed tests for copy/source immutability, duplicates, multipliers, date mapping, leftover references and rollback
- [ ] 3.2 Add Blazor component/E2E tests for mobile preview, replace, move, swap, undo/cancel and save failure feedback
- [ ] 3.3 Add tests for shopping regeneration and multi-editor conflicts if supported
- [ ] 3.4 Replace PENDING scenario rows with real test references, validate the delta and sync it to canonical specs only before archive

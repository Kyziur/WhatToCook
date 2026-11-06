## 1. Design Baseline and Contract — No Implementation Yet

- [ ] 1.1 Establish the canonical working URL/draft import baseline separately from pending photo/OCR work before syncing this delta
- [ ] 1.2 Confirm parser/candidate selection lifecycle, 24-hour expiry and draft conversion (URI-01, URI-02)
- [ ] 1.3 Confirm actual-connected-IP SSRF checks, disabled redirects and parser bounds (URI-03)
- [ ] 1.4 Validate mobile share-target prerequisites and URL paste fallback (URI-04)
- [ ] 1.5 Agree conservative source URL equivalence and recoverable errors (URI-05, URI-06)

## 2. Future Implementation — Not in This PR

- [ ] 2.1 Harden fetching, destination checks and bounded processing before generic-domain rollout (URI-03)
- [ ] 2.2 Add generic JSON-LD recipe extraction and single-candidate draft creation (URI-01)
- [ ] 2.3 Implement temporary candidate attempts, selection/expiry and repeat-safe draft conversion (URI-02)
- [ ] 2.4 Add mobile share-to-import with manual paste fallback (URI-04)
- [ ] 2.5 Add source/title duplicate guidance and honest recoverable error outcomes (URI-05, URI-06)

## 3. Future Tests and Canonical Sync

- [ ] 3.1 Add parser fixtures for JSON-LD root/graph/arrays, steps, Unicode and malformed content
- [ ] 3.2 Add controlled fake-network SSRF, DNS-rebinding, redirect-block and parser-bound tests
- [ ] 3.3 Add PostgreSQL tests for candidate selection, 24-hour expiry and idempotent finalization; component/E2E review tests
- [ ] 3.4 Test share/paste UX on supported devices and unsupported browser fallback
- [ ] 3.5 Replace PENDING rows with real tests and sync only once the base canonical import spec is available

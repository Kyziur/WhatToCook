## 1. Design and Dependency Resolution — No Implementation Yet

- [ ] 1.1 Complete and sync the existing `add-recipe-import` base capability before treating this delta as sync-ready
- [ ] 1.2 Confirm JSON-LD object selection, fields, `HowToStep` / `HowToSection` handling and multiple-recipe UX (URI-01, URI-02)
- [ ] 1.3 Review arbitrary-host threat model, DNS rebinding, redirect policy and parser resource bounds (URI-03)
- [ ] 1.4 Validate mobile share-target prerequisites and paste fallback (URI-04)
- [ ] 1.5 Agree duplicate source/title normalization and user-friendly error categories (URI-05, URI-06)

## 2. Future Implementation Packages

- [ ] 2.1 Implement hardened fetcher, connection destination enforcement and bounded parsing before generic-host rollout (URI-03)
- [ ] 2.2 Add Schema.org Recipe JSON-LD extraction behind existing source adapters and map into import drafts (URI-01)
- [ ] 2.3 Support multiple-recipe choice, safe draft review, persistence and idempotent finalization (URI-02)
- [ ] 2.4 Provide mobile share-to-import entry and URL paste fallback (URI-04)
- [ ] 2.5 Add source/title duplicate guidance and recoverable errors (URI-05, URI-06)

## 3. Future Tests and OpenSpec Sync

- [ ] 3.1 Add parser fixture tests for JSON-LD roots, `@graph`, arrays, nested steps, Unicode and malformed input
- [ ] 3.2 Add controlled SSRF/security tests covering DNS rebinding, private destinations, redirects, payload limits and HTML sanitization
- [ ] 3.3 Add PostgreSQL-backed draft/finalization tests and component/E2E tests for review, duplicate handling and manual fallback
- [ ] 3.4 Validate mobile share behavior on supported devices/origins and record unsupported-browser fallback coverage
- [ ] 3.5 Replace PENDING traceability rows with real tests, validate against the base canonical capability, then sync before archive

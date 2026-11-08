## Implementation and verification

- [x] Reproduce old and SQL-prefiltered query shapes on PostgreSQL catalog sizes 100/1k/10k.
- [x] Assert candidate IDs, ranking parity and HTTP recommendations using the original service.
- [x] Record timing, materialization and allocation diagnostics without unstable timing assertions.
- [ ] Inspect CI evidence and determine whether a targeted SQL predicate is justified.
- [ ] If justified, implement the predicate, rerun parity tests and document results.
- [ ] Review full CI, record traceability/evidence and update the audit.

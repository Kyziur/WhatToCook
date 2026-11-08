## Implementation and verification

- [x] Reproduce old and SQL-prefiltered query shapes on PostgreSQL catalog sizes 100/1k/10k.
- [x] Assert candidate IDs, ranking parity and HTTP recommendations using the original service.
- [x] Record timing, materialization and allocation diagnostics without unstable timing assertions.
- [x] Inspect green CI 42490440131: 9,999 baseline recipes vs 499 matching candidates at 10k; ranking and HTTP parity passed.
- [x] Add only the SQL EXISTS matching-ingredient predicate to the real shopping recommendation query and document the tradeoff; final regression CI pending.
- [x] Review full CI (run 42491096130: verify and ARM64 success), record evidence and update the audit.

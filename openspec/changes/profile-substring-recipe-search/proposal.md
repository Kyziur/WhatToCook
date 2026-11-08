## Why

PERF-03 identified potentially expensive substring title search, but the audit has no representative query-plan evidence. A premature production GIN migration would add write/storage cost without confirming a bottleneck.

## What Changes

- Reproduce 1,000- and 10,000-recipe title searches on PostgreSQL Testcontainers using the existing Npgsql translation.
- Collect and report `EXPLAIN (ANALYZE, BUFFERS)` summaries for count and bounded page queries with and without a disposable `pg_trgm` index.
- Assert stable matching counts, page IDs and HTTP response semantics before and after a test-only index.
- Capture the decision and test evidence in the audit without inventing production latency.

## Non-Goals

No production database migration/index, permanent extension, alternative matching semantics, prefix-only search, extra runtime telemetry or automatic speed-based CI gate.

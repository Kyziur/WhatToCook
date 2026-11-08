## Why

PERF-04 identified a full active-catalog ingredient materialization in recommendation lookup, but no measured effect at household scale. The current algorithm filters out recipes with zero owned ingredients *after* transferring their ingredient graph.

## What Changes

- Measure materialized recipe and ingredient rows, approximate .NET allocations and timings at 100/1,000/10,000 recipes with real PostgreSQL Testcontainers.
- Compare the current query with a SQL `EXISTS` candidate filter for a possessed ingredient, without limiting the pre-rank candidate set.
- Assert exact eligible candidate IDs and sorted top recommendations against the old behavior and the public API.
- Only consider changing the service query after measurements and regression evidence.

## Non-Goals

No database migration, schema or index change; no new recommendation score, new eligibility rules, pre-ranking top-N cutoff, or caching layer.

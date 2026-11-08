# PERF-04 — performance evidence

## Initial green benchmark

[CI run 42490440131](https://github.com/Kyziur/WhatToCook/actions/runs/42490440131) passed .NET build, 118 API integration tests, 53 component tests, the dedicated 100/1k/10k benchmark, Playwright, ARM64, dependency audit gates and CSharpier.

On the same synthetic 10k-recipe PostgreSQL catalog, the original query loaded **9,999 recipes / 29,997 ingredient links** and the SQL shared-ingredient candidate filter loaded **499 recipes / 1,497 ingredient links**. Single-run timing was **109.75 ms vs 9.14 ms**; global approximate managed allocations **19.91 MB vs 1.02 MB**. At 1,000 recipes the comparison was 999 vs 49 recipes. These measurements reflect a sparse 5%-matching synthetic catalog, not measured production throughput or p95.

## Semantic verification

The diagnostic asserts identical selected candidate IDs, distinct ingredient scoring, 30% coverage/rank ordering, a top-20 limit after ranking, and the public HTTP endpoint's selected recommendation IDs for 100/1,000/10,000 recipes. Existing endpoint integration tests cover planned-recipe exclusion, exact 30% boundary, below-boundary exclusion and the matched-count priority. The synthetic fixture additionally contains one inactive matched recipe.

## Engineering decision

Add only the database-side `EXISTS` filter for at least one owned normalized ingredient. Retain every child ingredient for retained recipes and the existing in-memory coverage/ranking implementation; do not `Take` before ranking.

**Final regression:** [CI run 42491096130](https://github.com/Kyziur/WhatToCook/actions/runs/42491096130) on optimized source passed full .NET tests, repeated PostgreSQL diagnostics, browser Playwright suite, ARM64 smoke, dependency audit gates and CSharpier. No production migrations/DDL, new cutoff, or changed ranking were introduced.

The feature-scoped review found no evidence that an additional service extraction or database index was necessary. The SQL prefilter is the only behavioral query-shape change. Production p95 latency remains unknown.

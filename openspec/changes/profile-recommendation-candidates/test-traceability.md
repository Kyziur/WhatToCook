# PERF-04 traceability

| Acceptance scenario | Test | State |
| --- | --- | --- |
| Filter only unmatched recipe candidates | `RecommendationCandidateDiagnosticsTests.CandidatePrefilter_ShouldPreserveRecommendationsAtGrowingCatalogSizes` | GREEN 42491096130 (full optimized service); baseline 42490440131 |
| Preserve ranking/coverage and final cutoff | Same test + `ShoppingListManagementSpecificationTests.Recommendations_ShouldApplyThirtyPercentCoverageBoundary` | GREEN 42491096130 (full optimized service); baseline 42490440131 |
| Preserve planned/inactive exclusions | Existing shopping list recommendation integration tests; synthetic inactive fixture | GREEN 42491096130 (full optimized service); baseline 42490440131 |
| Report rows/links/timing/allocations without speed gate | Dedicated diagnostic CI step | GREEN 42491096130 (full optimized service); baseline 42490440131 |

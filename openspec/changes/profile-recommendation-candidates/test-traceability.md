# PERF-04 traceability

| Acceptance scenario | Test | State |
| --- | --- | --- |
| Filter only unmatched recipe candidates | `RecommendationCandidateDiagnosticsTests.CandidatePrefilter_ShouldPreserveRecommendationsAtGrowingCatalogSizes` | CI pending |
| Preserve ranking/coverage and final cutoff | Same test + `ShoppingListManagementSpecificationTests.Recommendations_ShouldApplyThirtyPercentCoverageBoundary` | CI pending |
| Preserve planned/inactive exclusions | Existing shopping list recommendation integration tests; synthetic inactive fixture | CI pending |
| Report rows/links/timing/allocations without speed gate | Dedicated diagnostic CI step | CI pending |

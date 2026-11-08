# PERF-04 traceability

| Acceptance scenario | Test | State |
| --- | --- | --- |
| Filter only unmatched recipe candidates | `RecommendationCandidateDiagnosticsTests.CandidatePrefilter_ShouldPreserveRecommendationsAtGrowingCatalogSizes` | Initial green CI 42490440131; final-head CI pending |
| Preserve ranking/coverage and final cutoff | Same test + `ShoppingListManagementSpecificationTests.Recommendations_ShouldApplyThirtyPercentCoverageBoundary` | Initial green CI 42490440131; final-head CI pending |
| Preserve planned/inactive exclusions | Existing shopping list recommendation integration tests; synthetic inactive fixture | Initial green CI 42490440131; final-head CI pending |
| Report rows/links/timing/allocations without speed gate | Dedicated diagnostic CI step | Initial green CI 42490440131; final-head CI pending |

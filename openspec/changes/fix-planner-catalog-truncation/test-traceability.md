# Planner catalog test traceability

| Acceptance scenario | Automated verification | Status |
| --- | --- | --- |
| Planner accesses 551 active recipes over bounded pages | `ApiRecipeCatalogServiceTests.PlannerCatalog_ShouldTraverseEveryPageBeyondFiveHundredWithoutFacets` | CI pending |
| Planner rejects incomplete page traversal | `ApiRecipeCatalogServiceTests.PlannerCatalog_ShouldFailRatherThanSilentlyTruncateOnMissingPage` | CI pending |
| Short nonempty page is detected before catalog is returned | `ApiRecipeCatalogServiceTests.PlannerCatalog_ShouldFailOnShortNonemptyPageRatherThanHideRecipes` | CI pending |
| API opt-out returns entries but no facets | `RecipeManagementCoreSpecificationTests.PagedLibrary_ShouldSkipFacetsOnlyWhenExplicitlyRequested` | CI pending |
| Existing library API default facets retained | Same HTTP regression test + existing paged library tests | CI pending |
| Planner selections and mobile interactions unaffected | Existing component/E2E planner tests | CI pending |

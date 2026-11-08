# ARC-05 test traceability

| Acceptance scenario | Verification | Status |
| --- | --- | --- |
| Missing page total fails fast | `RecipeCatalogHttpContractTests.PagedCatalog_MissingTotalCount_ShouldFailInsteadOfShowingAnEmptyLibrary` | CI pending |
| Missing summary title fails fast | `RecipeCatalogHttpContractTests.PagedCatalog_MissingRecipeTitle_ShouldFailInsteadOfDisplayingABlankCard` | CI pending |
| Server page mismatch fails fast | `RecipeCatalogHttpContractTests.PagedCatalog_WrongPageMetadata_ShouldFailInsteadOfRenderingWrongPage` | CI pending |
| Missing detail steps fails fast | `RecipeCatalogHttpContractTests.RecipeDetails_MissingSteps_ShouldFailInsteadOfSilentlyHidingInstructions` | CI pending |
| Real API page and details shape | `RecipeCatalogClientContractTests.RecipeCatalogHttpResponses_ShouldPreserveRequiredWebConsumerFields` | CI pending |
| Existing planner and library remain compatible | `ApiRecipeCatalogServiceTests`, current component/E2E suites | CI pending |

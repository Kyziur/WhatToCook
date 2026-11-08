# ARC-05 test traceability

| Acceptance scenario | Verification | Status |
| --- | --- | --- |
| Missing page total fails fast | `RecipeCatalogHttpContractTests.PagedCatalog_MissingTotalCount_ShouldFailInsteadOfShowingAnEmptyLibrary` | CI pending |
| Missing summary title fails fast | `RecipeCatalogHttpContractTests.PagedCatalog_MissingRecipeTitle_ShouldFailInsteadOfDisplayingABlankCard` | CI pending |
| Server page mismatch fails fast | `RecipeCatalogHttpContractTests.PagedCatalog_WrongPageMetadata_ShouldFailInsteadOfRenderingWrongPage` | CI pending |
| Present-but-null page items fails fast | `RecipeCatalogHttpContractTests.PagedCatalog_PresentButNullItems_ShouldFailInsteadOfRenderingAnEmptyPage` | CI pending |
| Present-but-null detail steps fails fast | `RecipeCatalogHttpContractTests.RecipeDetails_PresentButNullSteps_ShouldFailInsteadOfHidingInstructions` | CI pending |
| Missing nested ingredient name fails fast | `RecipeCatalogHttpContractTests.RecipeDetails_MissingNestedIngredientName_ShouldFailDeserialization` | CI pending |
| Missing detail steps fails fast | `RecipeCatalogHttpContractTests.RecipeDetails_MissingSteps_ShouldFailInsteadOfSilentlyHidingInstructions` | CI pending |
| Real API page and details shape | `RecipeCatalogClientContractTests.RecipeCatalogHttpResponses_ShouldPreserveRequiredWebConsumerFields` | CI pending |
| Existing planner and library remain compatible | `ApiRecipeCatalogServiceTests`, current component/E2E suites | CI pending |

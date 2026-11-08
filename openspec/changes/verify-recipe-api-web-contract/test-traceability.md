# ARC-05 test traceability

| Acceptance scenario | Verification | Status |
| --- | --- | --- |
| Missing page total fails fast | `RecipeCatalogHttpContractTests.PagedCatalog_MissingTotalCount_ShouldFailInsteadOfShowingAnEmptyLibrary` | [CI 42530745977](https://github.com/Kyziur/WhatToCook/actions/runs/42530745977) passed |
| Missing summary title fails fast | `RecipeCatalogHttpContractTests.PagedCatalog_MissingRecipeTitle_ShouldFailInsteadOfDisplayingABlankCard` | [CI 42530745977](https://github.com/Kyziur/WhatToCook/actions/runs/42530745977) passed |
| Server page mismatch fails fast | `RecipeCatalogHttpContractTests.PagedCatalog_WrongPageMetadata_ShouldFailInsteadOfRenderingWrongPage` | [CI 42530745977](https://github.com/Kyziur/WhatToCook/actions/runs/42530745977) passed |
| Present-but-null page items fails fast | `RecipeCatalogHttpContractTests.PagedCatalog_PresentButNullItems_ShouldFailInsteadOfRenderingAnEmptyPage` | [CI 42530745977](https://github.com/Kyziur/WhatToCook/actions/runs/42530745977) passed |
| Present-but-null detail steps fails fast | `RecipeCatalogHttpContractTests.RecipeDetails_PresentButNullSteps_ShouldFailInsteadOfHidingInstructions` | [CI 42530745977](https://github.com/Kyziur/WhatToCook/actions/runs/42530745977) passed |
| Missing nested ingredient name fails fast | `RecipeCatalogHttpContractTests.RecipeDetails_MissingNestedIngredientName_ShouldFailDeserialization` | [CI 42530745977](https://github.com/Kyziur/WhatToCook/actions/runs/42530745977) passed |
| Missing detail steps fails fast | `RecipeCatalogHttpContractTests.RecipeDetails_MissingSteps_ShouldFailInsteadOfSilentlyHidingInstructions` | [CI 42530745977](https://github.com/Kyziur/WhatToCook/actions/runs/42530745977) passed |
| Real API page and details shape | `RecipeCatalogClientContractTests.RecipeCatalogHttpResponses_ShouldPreserveRequiredWebConsumerFields` | [CI 42530745977](https://github.com/Kyziur/WhatToCook/actions/runs/42530745977) passed |
| Existing planner and library remain compatible | `ApiRecipeCatalogServiceTests`, current component/E2E suites | [CI 42530745977](https://github.com/Kyziur/WhatToCook/actions/runs/42530745977) passed |

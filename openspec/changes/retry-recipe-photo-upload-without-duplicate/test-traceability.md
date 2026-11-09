# ARC-01 targeted — traceability

| Requirement | Test | Verification |
| --- | --- | --- |
| Returned upload failure retries update on the original ID | `RecipeEditorSpecificationTests.CreateRecipe_WhenPhotoUploadFails_ShouldRetryWithoutCreatingDuplicate(false)` | [CI 42599607557](https://github.com/Kyziur/WhatToCook/actions/runs/42599607557) passed |
| Thrown upload exception retries update on the original ID | `RecipeEditorSpecificationTests.CreateRecipe_WhenPhotoUploadFails_ShouldRetryWithoutCreatingDuplicate(true)` | [CI 42599607557](https://github.com/Kyziur/WhatToCook/actions/runs/42599607557) passed |
| Create without photo navigates to details | `RecipeEditorSpecificationTests.CreateRecipe_WithValidPayload_ShouldCallApiServiceAndNavigateToDetails` | Existing regression; [CI 42599607557](https://github.com/Kyziur/WhatToCook/actions/runs/42599607557) passed |
| Existing editor/API/contract unchanged | Scoped diff, full component/API/Playwright suites | [CI 42599607557](https://github.com/Kyziur/WhatToCook/actions/runs/42599607557) passed |

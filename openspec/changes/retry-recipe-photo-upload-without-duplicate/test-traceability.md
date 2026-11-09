# ARC-01 targeted — traceability

| Requirement | Test | Verification |
| --- | --- | --- |
| Returned upload failure retries update on the original ID | `RecipeEditorSpecificationTests.CreateRecipe_WhenPhotoUploadFails_ShouldRetryWithoutCreatingDuplicate(false)` | Pending PR CI |
| Thrown upload exception retries update on the original ID | `RecipeEditorSpecificationTests.CreateRecipe_WhenPhotoUploadFails_ShouldRetryWithoutCreatingDuplicate(true)` | Pending PR CI |
| Create without photo navigates to details | `RecipeEditorSpecificationTests.CreateRecipe_WithValidPayload_ShouldCallApiServiceAndNavigateToDetails` | Existing regression; pending PR CI |
| Existing editor/API/contract unchanged | Scoped diff, full component/API/Playwright suites | Pending PR CI |

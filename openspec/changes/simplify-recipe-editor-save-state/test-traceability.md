# Test traceability

| Behavior | Existing or new verification |
| --- | --- |
| Retry after API save failure | RecipeEditorSpecificationTests.SaveRecipe_AfterApiFailure_ShouldAllowRetry |
| Validation failure followed by valid submit | CreateRecipe_ShouldShowValidationForRequiredFields |
| Failed photo upload then retry, no duplicate create | CreateRecipe_WhenPhotoUploadFails_ShouldRetryWithoutCreatingDuplicate |
| Browser regressions | Playwright recipe editor flows |

No API contract changes.

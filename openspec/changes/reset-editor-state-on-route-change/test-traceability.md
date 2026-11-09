# Traceability

| Scenario | Test |
| --- | --- |
| Existing edit -> new route starts with empty model and new-recipe save | Editor_WhenReusedForNewRoute_ShouldDiscardPreviousRecipeState |
| Switching between edited recipes clears pending photo and submits only target recipe | Editor_WhenSwitchingEditedRecipes_ShouldNotUploadOldSelectedPhoto |
| Missing target recipe disables the previously loaded draft | Editor_WhenTargetRecipeIsMissing_ShouldDisableStaleDraft |
| Failed upload retry remains local to same editor session | CreateRecipe_WhenPhotoUploadFails_ShouldRetryWithoutCreatingDuplicate |

No HTTP contract changes.

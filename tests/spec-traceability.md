# OpenSpec to Test Traceability

Active changes: `define-recipe-management-foundation`, `add-recipe-import`, `design-recipe-metadata-filter`

| Capability | Scenario | Test | Status |
| --- | --- | --- | --- |
| `recipe-management-core` | Save a new recipe with complete core fields | `RecipeManagementCoreSpecificationTests.SaveNewRecipeWithCompleteCoreFields_ShouldPersistRecipe` | `PASS` |
| `recipe-management-core` | Edit an existing recipe | `RecipeManagementCoreSpecificationTests.EditExistingRecipe_ShouldPersistLatestVersion` | `PASS` |
| `recipe-management-core` | Reject duplicate title | `RecipeManagementCoreSpecificationTests.SaveRecipe_ShouldRejectDuplicateTitleAfterNormalization` | `PASS` |
| `recipe-management-core` | Archive a recipe | `RecipeManagementCoreSpecificationTests.ArchiveRecipe_ShouldRemoveItFromLibraryResults` | `PASS` |
| `recipe-management-core` | Save recipe with uploaded photo | `RecipeManagementCoreSpecificationTests.SaveRecipeWithUploadedPhoto_ShouldStoreMainPhotoPath` | `PASS` |
| `recipe-management-core` | Save recipe without photo | `RecipeManagementCoreSpecificationTests.SaveRecipeWithoutPhoto_ShouldKeepMainPhotoPathEmpty` | `PASS` |
| `recipe-management-core` | Display source as clickable link | `RecipeLibraryAndDetailsTests.Details_ShouldShowPlaceholderAndSourceRendering` | `PASS` |
| `recipe-management-core` | Display source as plain text | `RecipeManagementCoreSpecificationTests.SaveRecipeWithPlainTextSource_ShouldReturnSourceAsText` | `PASS` |
| `recipe-management-core` | Save ingredient without quantity | `RecipeManagementCoreSpecificationTests.SaveIngredientWithoutQuantity_ShouldBeAccepted` | `PASS` |
| `recipe-management-core` | Add ingredient from existing suggestion | `RecipeManagementCoreSpecificationTests.AddIngredientFromExistingSuggestion_ShouldReturnNormalizedSuggestions` | `PASS` |
| `recipe-management-core` | Suggest existing tag during editing | `RecipeManagementCoreSpecificationTests.SuggestExistingTagDuringEditing_ShouldReturnNormalizedSuggestions` | `PASS` |
| `recipe-management-core` | Search by title | `RecipeManagementCoreSpecificationTests.SearchByTitleAndIngredient_ShouldUseNormalizedMatching` | `PASS` |
| `recipe-management-core` | Search by all selected ingredients | `RecipeManagementCoreSpecificationTests.SearchByTitleAndIngredient_ShouldUseNormalizedMatching` | `PASS` |
| `recipe-management-core` | Browse recipes alphabetically | `RecipeLibraryAndDetailsTests.Library_ShouldRenderAlphabeticallyAndFilterByTitle` | `PASS` |
| `recipe-management-core` | Empty library state | `RecipeLibraryAndDetailsTests.Library_ShouldShowEmptyState_WhenNoRecipes` | `PASS` |
| `recipe-management-core` | Open recipe details from library | `RecipeLibraryAndDetailsTests.Details_ShouldShowPlaceholderAndSourceRendering` | `PASS` |
| `recipe-app-shell` | Return from details/editor preserves title query, selected tags, and selected ingredients | `recipe-management-core.spec.ts :: Return from details preserves title, tag and ingredient filters` | `PASS` |
| `recipe-app-shell` | API failure is shown as a safe user-facing error instead of an empty state | `recipe-management-core.spec.ts :: API failure shows a safe error instead of an empty library` | `PASS` |
| `recipe-management-core` | Preserve edited step order | `RecipeManagementCoreSpecificationTests.EditExistingRecipe_ShouldPersistLatestVersion` | `PASS` |
| `recipe-management-core` | Seed starter data on empty catalog | `RecipeSeedSpecificationTests.SeedEnabledAndCatalogEmpty_ShouldInsertStarterRecipes` | `PASS` |
| `recipe-management-core` | Do not overwrite existing catalog data | `RecipeSeedSpecificationTests.SeedEnabledAndCatalogAlreadyPopulated_ShouldNotOverwriteExistingData` | `PASS` |
| `recipe-management-core` | Create flow validates required fields (title, servings, ingredient, step) | `RecipeEditorSpecificationTests.CreateRecipe_ShouldShowValidationForRequiredFields` | `PASS` |
| `recipe-management-core` | Editor create flow persists recipe via API | `RecipeEditorSpecificationTests.CreateRecipe_WithValidPayload_ShouldCallApiServiceAndNavigateToDetails` | `PASS` |
| `recipe-app-shell` | Enter application after launch | `RecipeAppShellSpecificationTests.EnterApplication_ShouldLandInRecipeLibrary` | `PASS` |
| `recipe-app-shell` | Start creating a recipe from shell | `RecipeAppShellSpecificationTests.MainShell_ShouldExposePrimaryNavigationAndCreateAction` | `PASS` |
| `recipe-app-shell` | Open planner from shell | `RecipeAppShellSpecificationTests.MainShell_ShouldExposePrimaryNavigationAndCreateAction` | `PASS` |
| `recipe-app-shell` | Open with empty library | `RecipeAppShellSpecificationTests.EnterApplication_ShouldLandInRecipeLibrary` | `PASS` |
| `recipe-app-shell` | Open planner with no meal plans | `RecipeAppShellSpecificationTests.PlannerWithoutPlans_ShouldShowEmptyStateWithCreatePath` | `PASS` |
| `meal-planning` | Create/edit/overlap/day assignment/ordering/multiplier/inactive/regeneration flows | `MealPlanningSpecificationTests` (API integration + component) | `PASS` |
| `planning-workspace-ui` | Desktop weekly workspace, progress metrics, and shopping context | `agent-browser` at 1440 and 1920 pixels; `MealPlanningSpecificationTests.Planner_ShouldShowCompactDayPickerAndOneActiveDayEditor` | `PASS` |
| `planning-workspace-ui` | Mobile selected-day projection, navigation, and responsive recipe details | `agent-browser` at 375 and 768 pixels; `meal-planning.spec.ts`; `recipe-management-core.spec.ts` | `PASS` |
| `planning-workspace-ui` | Encoded meal-slot ranges and legacy dinner decoding | `MealPlanningSpecificationTests.MealSlotOrdering_ShouldEncodeStableRanges`; `MealPlanningSpecificationTests.MealSlotOrdering_ShouldPreserveLegacyDinnerAndClampPosition` | `PASS` |
| `planning-workspace-ui` | Contextual recipe picker and responsive direct assignment | `ResponsiveWorkflowUsabilitySpecificationTests`; `meal-planning.spec.ts` | `PASS` |
| `responsive-workflow-usability` | Reach direct plan assignment from responsive recipe browsing | `ResponsiveWorkflowUsabilitySpecificationTests.AddToPlanPicker_ShouldAddRecipeOnceToSelectedDay` | `PASS` |
| `shopping-list-management` | On-demand generation/aggregation/checklist/manual/explicit save-time regeneration decision/non-destructive reconciliation/copy flows | `ShoppingListManagementSpecificationTests` (API integration + component); `recipe-app-shell.spec.ts` | `PASS` |
| `self-hosted-web-platform` | Save structured recipe data | `SelfHostedWebPlatformSpecificationTests.SaveStructuredRecipeData_ShouldPersistInRelationalStore` | `PASS` |
| `self-hosted-web-platform` | Saved recipes available after restart/session | `SelfHostedWebPlatformSpecificationTests.SavedRecipes_ShouldRemainAvailableAcrossNewClientSessions` | `PASS` |
| `self-hosted-web-platform` | Start with external configuration | `SelfHostedWebPlatformSpecificationTests.StartWithExternalConfiguration_ShouldUseSuppliedStoragePath` | `PASS` |
| `self-hosted-web-platform` | Fail startup without a database connection string instead of a default credential; accept the Aspire `postgres` connection-string alias | `AppDbConnectionStringTests.Resolve_ShouldFailWhenNoDatabaseConnectionStringIsConfigured` + `Resolve_ShouldAcceptAspirePostgresConnectionStringAlias` | `PASS` |
| `self-hosted-web-platform` | Container-based deployment | `SelfHostedWebPlatformSpecificationTests.ContainerBasedDeployment_ShouldStartSuccessfully` | `PASS` |
| `self-hosted-web-platform` | Komodo/Bitwarden deployment contract and operational flow | `SelfHostedWebPlatformSpecificationTests.DeploymentDocumentation_ShouldCoverSelfHostedOperationalFlow` | `PASS` |

## E2E (Playwright)

| Capability | Scenario | Test | Status |
| --- | --- | --- | --- |
| `recipe-app-shell` | Enter the application after launch | `recipe-app-shell.spec.ts :: Enter the application after launch -> lands in recipe library` | `PASS` |
| `recipe-app-shell` | Start creating a recipe from the main shell | `recipe-app-shell.spec.ts :: Start creating a recipe from the main shell` | `PASS` |
| `recipe-app-shell` | Open planner from the main shell | `recipe-app-shell.spec.ts :: Open planner from shell` | `PASS` |
| `recipe-app-shell` | Attempt to leave the editor with unsaved changes | `recipe-app-shell.spec.ts :: Attempt to leave editor with unsaved changes shows warning` | `PASS` |
| `recipe-app-shell` | Attempt to save a changed plan with an existing shopping list | `recipe-app-shell.spec.ts :: Save changed plan with existing shopping list requires regeneration decision` | `PASS` |
| `recipe-app-shell` | Open planner and show create path | `recipe-app-shell.spec.ts :: Open planner and always show create path` | `PASS` |
| `recipe-management-core` | Save a new recipe with complete core fields | `recipe-management-core.spec.ts :: Create recipe with complete fields and uploaded photo` | `PASS` |
| `recipe-management-core` | Edit an existing recipe | `recipe-management-core.spec.ts :: Edit existing recipe and preserve updated step order` | `PASS` |
| `recipe-management-core` | Reject duplicate title | `recipe-management-core.spec.ts :: Reject duplicate title` | `PASS` |
| `recipe-management-core` | Archive a recipe | `recipe-management-core.spec.ts :: Archive recipe from UI` | `PASS` |
| `recipe-management-core` | Save recipe with uploaded photo | `recipe-management-core.spec.ts :: Create recipe with complete fields and uploaded photo` | `PASS` |
| `recipe-management-core` | Save recipe without photo | `recipe-management-core.spec.ts :: Create recipe without photo uses placeholder in details` | `PASS` |
| `recipe-management-core` | Display source as clickable link/plain text | `recipe-management-core.spec.ts :: Display source as clickable URL and plain text` | `PASS` |
| `recipe-management-core` | Save ingredient without quantity | `recipe-management-core.spec.ts :: Add ingredient without quantity is accepted` | `PASS` |
| `recipe-management-core` | Search by title/tag/ingredients | `recipe-management-core.spec.ts :: Search by title, tag and all selected ingredients` | `PASS` |
| `recipe-management-core` | Browse recipes alphabetically | `recipe-management-core.spec.ts :: Browse recipes alphabetically on library screen` | `PASS` |
| `recipe-management-core` | Open recipe details from the library | `recipe-management-core.spec.ts :: Open recipe details from library` | `PASS` |
| `recipe-app-shell` | Return from details preserves title query, selected tags, and selected ingredients | `recipe-management-core.spec.ts :: Return from details preserves title, tag and ingredient filters` | `PASS` |
| `recipe-app-shell` | API failure shows a safe user-facing error and does not expose backend text | `recipe-management-core.spec.ts :: API failure shows a safe error instead of an empty library` | `PASS` |
| `recipe-management-core` | Suggest existing ingredient/tag | `recipe-management-core.spec.ts :: Ingredient suggestions during editing` + `Tag suggestions during editing` | `PASS` |
| `design-recipe-metadata-filter` | Filter by preparation-time range, missing-value behavior, URL state, and invalid ranges | No implementation by design; scenarios remain pending on tasks 3.1-3.2 | `PENDING` |
| `meal-planning` | Create a plan with a default name | `meal-planning.spec.ts :: Create a plan with default name and allow editing that name` | `PASS` |
| `recipe-app-shell` | Save changed plan with existing shopping list requires confirmation or cancellation | `recipe-app-shell.spec.ts :: Save changed plan with existing shopping list requires regeneration decision` | `PASS` |
| `meal-planning` | Overlap/uniqueness/reorder/multiplier/inactive/regeneration business scenarios | `meal-planning.spec.ts` | `PASS` |
| `shopping-list-management` | Generate/no-auto/aggregation/unit split/no-quantity/checklist/manual/non-destructive regeneration/copy | `shopping-list-management.spec.ts` | `PASS` |
| `self-hosted-web-platform` | Browser access and responsive shell navigation | `self-hosted-web-platform.spec.ts :: Open application...` tests | `PASS` |
| `self-hosted-web-platform` | Deployment artifacts and docs availability for local-network hosting | `self-hosted-web-platform.spec.ts :: Container deployment assets exist...` + `Self-hosted deployment documentation exists` | `PASS` |

## Active Change Hygiene

| Change | State | Traceability owner/status |
| --- | --- | --- |
| `define-recipe-management-foundation` | Core implementation present; metadata filtering intentionally deferred | Main capability rows above; contract is tracked by `design-recipe-metadata-filter` task 1.1 |
| `add-recipe-import` | Partially implemented | `openspec/changes/add-recipe-import/test-traceability.md`; photo-import rows remain PENDING on tasks 3.1 and 3.2 |
| `design-recipe-metadata-filter` | Design-only; no implementation | `openspec/changes/design-recipe-metadata-filter/test-traceability.md`; scenarios remain PENDING |
| `align-planning-ui-reference` | Archived after completing implementation, visual verification, automated verification, and independent review | `openspec/changes/archive/2026-09-15-align-planning-ui-reference/test-traceability.md`; all tasks complete |

Completed changes are retained in `openspec/changes/archive/`: `2026-09-15-improve-core-ui-ux`, `2026-09-15-design-library-quick-add-to-plan`, and `2026-09-15-align-planning-ui-reference`.

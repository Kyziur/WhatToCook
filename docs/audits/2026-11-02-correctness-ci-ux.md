# Correctness, CI, and user experience audit

- **Audit snapshot:** [`4eb64d7e3de01faa7bef32db74da94e7e1c50913`](https://github.com/Kyziur/WhatToCook/tree/4eb64d7e3de01faa7bef32db74da94e7e1c50913), audited 2026-11-02.
- **Review:** recommendations were checked against the repository on 2026-11-06. Priorities below reflect that reviewed backlog; where it differs from the original audit, the change is noted.
- **Status:** code and workflow observations are static evidence, not proof of a production incident or a failing hosted run. Concurrency, JSON edge cases, browser behavior, and deployment-host behavior require the stated runtime verification.

## Assessment at a glance

Address input-validation and shopping semantics alongside the two CI reliability issues first. Before rollout, verify the deployment smoke path that the repository tests do not exercise. Extend existing tests for the concrete gaps rather than treating the established test suite or accessibility markup as absent. Defer optional workflow splitting, coverage gates, UI restructuring, and mobile performance budgets until their stated trigger is demonstrated.

| Finding | Reviewed priority | Focus |
|---|---|---|
| COR-01 | P1 | Ingredient/tag collisions after text normalization |
| COR-02 | P1 | Import draft input sizes and collection bounds |
| COR-03 | P1 | Repeated descriptive quantities and plan multipliers |
| COR-04 | P1 (originally P2) | Checklist-state matching during regeneration |
| COR-05 | P1 (originally P2) | Plan-name maximum on creation |
| COR-06 | P1 (originally P2) | Manual shopping-item name maximum |
| COR-07 | P2 | Concurrent draft finalization; verify before choosing a fix |
| COR-08 | P1 (originally P2) | Explicit null collections and nested elements at the HTTP boundary |
| COR-09 | P3 | Document catalog-wide uniqueness after normalization |
| CI-01 | P1 | Release build versus default `dotnet run` configuration |
| CI-02 | P1 | Native-command exit codes in PowerShell steps |
| CI-03 | P3 (originally P2) | Split the long CI job only if measurements justify it |
| CI-04 | P1 before rollout (originally P2) | Verify database and image write/read through deployment mounts |
| CI-05 | P1 alongside the corresponding correctness fixes; otherwise P2 | Add focused validation and boundary regression tests |
| CI-06 | P3 (originally P2) | Consider useful coverage reporting after establishing a baseline |
| CI-07 | P2 (originally P3) | Test operational contracts, not documentation wording |
| UX-01 | P3 (originally P2) | Refactor large UI state only for a demonstrated maintenance issue |
| UX-02 | P2 | Add targeted automated accessibility and keyboard checks |
| UX-03 | P2 | Test SignalR reconnect and preservation of unsaved form state |
| UX-04 | P3 | Establish mobile performance budgets only from representative measurements |
| UX-05 | Conditional P2; defer at P3 if current messages suffice (originally P3) | Improve import-error guidance when users need it |

The reviewed prioritization groups COR-02/05/06/08 and COR-03/04 with P1 data-correctness work, raises CI-04 to a pre-rollout check, and adjusts the optional items as shown. Those priority changes do not mean the affected runtime failures have been observed.

## Existing strengths and test coverage

The findings below are gaps in particular edge cases, not a claim that the repository lacks tests or accessibility work:

- Recipe API integration tests use a PostgreSQL Testcontainers-backed application factory, and import flows already have integration scenarios: [`TestWebApplicationFactory.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/tests/WhatToCook.Api.IntegrationTests/TestWebApplicationFactory.cs), [`RecipeImportSpecificationTests.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/tests/WhatToCook.Api.IntegrationTests/Specs/RecipeImportSpecificationTests.cs).
- Playwright already covers desktop/mobile projects and starts an API with PostgreSQL; a meal-planning scenario checks a 375px viewport: [`playwright.config.ts`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/playwright.config.ts), [`run-api-with-postgres.cjs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/tests/e2e/scripts/run-api-with-postgres.cjs), [`meal-planning.spec.ts`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/tests/e2e/meal-planning.spec.ts).
- The UI has useful accessibility affordances: the recipe library uses live/alert semantics and disabled loading actions; component/E2E tests cover selected states such as `aria-expanded`, `aria-invalid`/`aria-describedby`, and shopping-item `aria-pressed`. See [`RecipeLibraryPage.razor`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Web/Components/Features/Recipes/Library/Pages/RecipeLibraryPage.razor), [`RecipeLibraryAndDetailsTests.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/tests/WhatToCook.Web.ComponentTests/RecipeLibraryAndDetailsTests.cs), [`RecipeEditorSpecificationTests.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/tests/WhatToCook.Web.ComponentTests/Specs/RecipeEditorSpecificationTests.cs), and [`ShoppingListManagementSpecificationTests.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/tests/WhatToCook.Web.ComponentTests/Specs/ShoppingListManagementSpecificationTests.cs).
- These checks do not amount to an automated whole-flow accessibility audit, comprehensive keyboard-only verification, or a browser test of interrupted SignalR connections. Likewise, API tests against a test database do not establish host-volume ownership or media write permissions.

## Correctness findings

### COR-01 — Names can collide after normalization

**Priority: P1.**

**Impact and evidence:** Recipe writes first deduplicate text with ordinal case-insensitive comparison, then build dictionaries with normalized keys. Those are not equivalent operations: names such as `Łosoś` and `Losos` can survive the first check but normalize to the same key (`losos`), causing dictionary construction to throw before a controlled validation response. The same pattern applies to tags. This is a statically identified path, not a report of a production 500. See [`RecipeWriteService.cs`, lines 193–204](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Shared/RecipeWriteService.cs#L193-L204) and [`TextNormalizer.cs`, lines 8–29](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Shared/TextNormalizer.cs#L8-L29).

**Recommendation:** Detect collisions using the same normalized key used by persistence. Choose an explicit policy: reject conflicting values with a validation response, or merge only when ingredient quantities/units can be combined safely. Do not silently drop separate ingredient quantities. Apply the rule consistently to create/update and to tags.

**Verification:** At the HTTP boundary, create and update recipes with diacritic/case variants of the same ingredient and tag. Assert either intentional, lossless merge or controlled 4xx; never an unhandled exception/500. Include compatible and incompatible ingredient quantity/unit pairs.

### COR-02 — Bound import draft input before persistence

**Priority: P1.**

**Impact and evidence:** Draft update and creation from extraction need field and collection-size checks before untrusted content is persisted. Overlong ingredient, step, or tag values can reach database column limits and turn an input problem into a persistence exception instead of a controlled draft issue or 4xx. The update path is transactional, but a transaction alone does not provide useful validation behavior or prove that all input has been checked before mutation. See [`RecipeImportService.cs`, lines 159–263](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Imports/RecipeImportService.cs#L159-L263) and [`AppDbContext.cs`, lines 106–157](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Infrastructure/Data/AppDbContext.cs#L106-L157).

**Recommendation:** Validate request size, per-field limits, and collection counts before delete/add or other durable mutation. Apply consistent rules to draft updates and extraction-created drafts; independently bound extracted HTML/content. For PostgreSQL `varchar` limits, count Unicode scalar values rather than UTF-16 code units so reviewable drafts that fit the database are not rejected. Return controlled validation errors or draft issues rather than relying on a database exception.

**Verification:** Exercise max and max+1 boundaries for relevant fields and collections, including an overlong ingredient, step, and excessive tags. For rejected updates, verify the prior draft remains intact and no partial changes are committed; verify extraction follows the same controlled contract.

### COR-03 — Preserve descriptive quantities, repetitions, and multipliers

**Priority: P1.**

**Impact and evidence:** The shopping-list aggregation tracks repetitions for descriptive quantities, but the rendered result uses the quantity text without the repetition count. Repeated entries such as “a pinch of salt” can therefore collapse to a single-looking “pinch”; numeric amounts and descriptive phrases also do not have the same multiplication semantics. Multiplying “to taste” or “a pinch” as if it were a numeric quantity is not necessarily meaningful. See [`ShoppingListService.cs`, lines 325–455](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Shopping/Lists/ShoppingListService.cs#L325-L455) and the aggregation requirements in [`shopping-list-management/spec.md`, lines 20–33](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/openspec/specs/shopping-list-management/spec.md#L20-L33).

**Recommendation:** Decide the product representation before changing aggregation: for example, show a repetition marker (`×2`), retain source-specific descriptive entries, or explicitly label the phrase as recipe guidance. Preserve repetitions and plan multipliers visibly; multiply numeric quantities as appropriate, but do not invent arithmetic for nonnumeric descriptions.

**Verification:** Cover repeated descriptive quantities from multiple recipes, a plan multiplier of two, numeric quantities, missing quantities, and mixed descriptive/numeric contributions. Assert the final text is unambiguous and does not hide repetitions or imply unsupported arithmetic.

### COR-04 — Define when regenerated items retain checklist state

**Priority: P1 in the reviewed backlog (originally P2).**

**Impact and evidence:** Regeneration uses a key based on normalized ingredient name and unit, not quantity text. Consequently, an item can keep its `MAM` state when its quantity representation changes; in particular, a numeric and descriptive quantity for the same normalized ingredient/unit may be treated as a match. The current specification explicitly requires stable ingredient-and-unit matching and preservation of matching generated-item state, while the safe meaning of a match across quantity-type changes needs a product decision. See [`ShoppingListService.cs`, lines 462–510](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Shopping/Lists/ShoppingListService.cs#L462-L510) and [`shopping-list-management/spec.md`, lines 46–63](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/openspec/specs/shopping-list-management/spec.md#L46-L63).

**Recommendation:** Make the stable matching rule explicit before implementation. Under the current contract, checklist state follows a generated item with the same normalized ingredient-and-unit key, independent of quantity. Confirm whether that remains valid when the quantity changes from numeric to descriptive (or vice versa). If not, define a semantic identity/lineage rule and update the shopping-list specification; retain atomic reconciliation and every manual item, including duplicates. Do not change the matching key without deciding the user-visible behavior.

**Verification:** Regenerate after changing quantity, quantity type, recipe ordering, and ingredient spelling/case while keeping or changing units. Assert exactly which generated rows retain `MAM`/`NIE MAM`, stale rows are removed, and all manual items remain. The expected result must match the updated specification.

### COR-05 — Validate plan-name length on creation

**Priority: P1 in the reviewed backlog (originally P2).**

**Impact and evidence:** The create endpoint does not apply the same maximum name length as update, while the database limits plan names to 200 characters. A 201-character create request can therefore reach persistence instead of receiving a validation response. See [`PlansEndpoints.cs`, lines 62–87](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Planning/Plans/PlansEndpoints.cs#L62-L87) and [`AppDbContext.cs`, lines 176–182](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Infrastructure/Data/AppDbContext.cs#L176-L182).

**Recommendation:** Share the name-length validation between create and update and return the endpoint's normal validation response for invalid input.

**Verification:** Test names at 200 and 201 characters through both create and update. The accepted boundary succeeds; the over-limit value returns 4xx and creates no partial record.

### COR-06 — Enforce the manual shopping-item name limit

**Priority: P1 in the reviewed backlog (originally P2).**

**Impact and evidence:** The manual-item endpoint checks for an empty name but does not enforce the database's 200-character maximum. A value beyond that limit can become a database error instead of a useful client validation response. See [`ShoppingListEndpoints.cs`, lines 100–123](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Shopping/Lists/ShoppingListEndpoints.cs#L100-L123), [`ShoppingListService.cs`, lines 188–236](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Shopping/Lists/ShoppingListService.cs#L188-L236), and [`AppDbContext.cs`, lines 225–231](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Infrastructure/Data/AppDbContext.cs#L225-L231).

**Recommendation:** Trim and validate the manual name against the same 200-character limit as persistence, using the API's normal validation-error contract.

**Verification:** Submit 200- and 201-character names. The former is handled according to the existing trim policy; the latter returns 4xx and leaves the list unchanged.

### COR-07 — Verify concurrent draft finalization before choosing protection

**Priority: P2; behavior requires a concurrency test.**

**Impact and evidence:** Two finalization requests may both observe the same draft as not finalized before either request commits. A unique recipe-title constraint may prevent a duplicate recipe, but that does not establish consistent responses or draft state. The race and its production consequence have not been demonstrated. See [`RecipeImportService.cs`, lines 267–319](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Imports/RecipeImportService.cs#L267-L319).

**Recommendation:** First build a deterministic concurrent-finalization test and inspect the recipe count, each response, and the persisted draft state. Add a transaction lock, optimistic concurrency, idempotency key, or return of the existing finalized recipe only if the observed behavior violates the intended contract; select the smallest mechanism that satisfies that contract.

**Verification:** Send ten finalizations for one draft concurrently under test. Confirm exactly one recipe, a single consistent finalized recipe ID/state, and controlled responses (such as success for the winner and success/conflict for others), with no 500. This is a proposed check, not a claim that the race has caused an incident.

### COR-08 — Validate explicit nulls at the HTTP boundary

**Priority: P1 in the reviewed backlog (originally P2); deserialization behavior is unverified.**

**Impact and evidence:** DTO annotations and validators may not by themselves establish what happens when JSON explicitly supplies a null collection or a null nested element. Inputs such as `"ingredients": null` or `"ingredients": [null]` could reach code that expects non-null values, but a runtime failure has not been demonstrated. Test the actual JSON-to-endpoint path rather than infer behavior from nullable annotations alone. See [`RecipeContracts.cs`, lines 3–18](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Shared/RecipeContracts.cs#L3-L18) and [`RecipeWriteValidator.cs`, lines 40–49](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Shared/RecipeWriteValidator.cs#L40-L49).

**Recommendation:** Define nullability and collection-element rules in request validation at the HTTP boundary. Return a controlled validation response for disallowed values; do not rely on an unhandled null dereference or on assumptions about deserializer behavior.

**Verification:** Post JSON with omitted, empty, explicit-null collections and null nested elements, plus oversized values. Assert the intended success/4xx contract for each and no 500 or partial write. Add property-based/fuzz coverage only if it complements these explicit boundary cases.

### COR-09 — Document catalog-wide normalized-title uniqueness

**Priority: P3 / optional.**

**Impact and evidence:** Diacritics are removed during normalization, and normalized titles participate in catalog-wide uniqueness. Two titles that are genuinely distinct to a user may therefore map to one normalized title. This is a product trade-off, not a demonstrated defect. See [`TextNormalizer.cs`, lines 8–51](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Shared/TextNormalizer.cs#L8-L51) and the uniqueness mapping in [`AppDbContext.cs`, lines 28–38](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Infrastructure/Data/AppDbContext.cs#L28-L38).

**Trigger and recommendation:** For now, document the existing rule and retain catalog-wide uniqueness. Revisit it only if a product requirement or user example establishes that distinct recipes with the same normalized title must coexist. Decide the new identity rule against the actual product model before changing the index or write path; do not introduce category-specific uniqueness speculatively.

**Verification:** Record the product decision and test representative Polish diacritic variants. If the rule changes, check existing data for collisions, exercise the newly specified identity cases, and verify the migration and controlled conflict response.

## CI findings

### CI-01 — Align the E2E server configuration with the CI build

**Priority: P1.**

**Impact and evidence:** The workflow builds `Release`, while both Playwright server processes use `dotnet run --no-build` without explicitly selecting `Release`. `dotnet run` normally uses `Debug`; the reviewed local MSBuild default was also `Debug`. On a clean runner that contains only Release outputs, the E2E servers may not start from the expected output. This is a configuration mismatch and risk, not evidence that a GitHub-hosted run failed. See [`.github/workflows/pr-ci.yml`, lines 32–44](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/.github/workflows/pr-ci.yml#L32-L44), [`playwright.config.ts`, lines 40–50](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/playwright.config.ts#L40-L50), and [`run-api-with-postgres.cjs`, lines 42–46](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/tests/e2e/scripts/run-api-with-postgres.cjs#L42-L46).

**Recommendation:** Make both server commands explicitly use `--configuration Release --no-build` for CI, while preserving the convenient local Debug workflow. Avoid solving the mismatch by silently building a second configuration in CI unless there is a specific need.

**Verification:** On a clean checkout/runner, perform the Release build followed by the same Playwright commands used in CI. Confirm both processes start and record their exit codes. Do not treat local Debug success as proof of the Release path.

### CI-02 — Propagate failures from each PowerShell native command

**Priority: P1.**

**Impact and evidence:** The PowerShell steps chain native commands with `;`, including tool restore followed by restore, and package listing followed by `npm audit`. A failed first native command can have its `$LASTEXITCODE` replaced by a successful later command unless the step explicitly checks and exits on each result. A narrow local Windows PowerShell check observed a first command returning 7, a second returning 0, and final status 0; this was not a GitHub Actions run. Bash `shell` fail-fast behavior is not interchangeable with PowerShell semantics. See [`.github/workflows/pr-ci.yml`, lines 28–30](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/.github/workflows/pr-ci.yml#L28-L30) and [lines 46–48](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/.github/workflows/pr-ci.yml#L46-L48).

**Recommendation:** Put each native command in its own workflow step, or explicitly test `$LASTEXITCODE` immediately after each command and exit with that code on failure. Keep bash and PowerShell failure behavior explicit rather than relying on a semicolon convention.

**Verification:** In a controlled workflow check, make the first command fail and the following command succeed. The step/workflow must still fail with the first command's nonzero status. Also verify the normal successful path remains green.

### CI-03 — Split the long job only when measurements show value

**Priority: P3 / optional (originally P2).**

**Impact and evidence:** The workflow combines build/test work, Compose integration work, and Playwright projects in one job with a 30-minute limit. This can make feedback and failure localization less useful, but no measured time-to-signal problem or CI performance regression is established here. See [`.github/workflows/pr-ci.yml`, lines 9–52](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/.github/workflows/pr-ci.yml#L9-L52) and [`SelfHostedWebPlatformSpecificationTests.cs`, lines 94–190](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/tests/WhatToCook.Api.IntegrationTests/Specs/SelfHostedWebPlatformSpecificationTests.cs#L94-L190).

**Trigger and recommendation:** Defer restructuring unless PR measurements show slow time-to-signal, expensive reruns, poor failure localization, or recurrent job-limit pressure. If triggered, separate fast checks, integration/Compose, and E2E into appropriately dependent jobs; consider NuGet/npm caching and path selection without removing required gates.

**Verification:** Compare representative PR durations, time-to-first-failure, rerun scope, failure locality, and cancellation behavior before and after. Retain all required checks and confirm changes to shared code still execute the relevant integration and E2E gates.

### CI-04 — Verify database and media access through the deployment mount before rollout

**Priority: P1 before rollout (originally P2).**

**Impact and evidence:** The self-hosted Compose test reaches the Web response but does not prove a DB-backed write/read or image write/read through the host-mounted storage path. API integration tests against PostgreSQL do not prove that the deployed host directory has permissions for the application UID. A successful upload test in a test environment is not evidence that the real mount is writable. See [`SelfHostedWebPlatformSpecificationTests.cs`, lines 94–177](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/tests/WhatToCook.Api.IntegrationTests/Specs/SelfHostedWebPlatformSpecificationTests.cs#L94-L177) and the deployment write/read requirement in [`self-hosted-deployment.md`, lines 32–34](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/docs/self-hosted-deployment.md#L32-L34).

**Recommendation:** Before rollout, exercise a DB-backed recipe create/read and image upload/retrieval through the deployment-compatible Compose/API path, using the documented host preparation and established deployment wrapper. Use an isolated environment where possible or a dedicated disposable recipe on the target; never replace a household recipe's photo. Archive the test recipe and retain its referenced image, or use an approved cleanup that removes the database reference before the file. The API runs as UID/GID `1654:1654`; `/srv/whattocook/recipe-images` must be prepared by the fail-closed Periphery helper with owner/group `1654:1654` and mode `0750`. Keep secrets in the existing `homelab-workloads` scope and wrapper; do not make the application require `BWS_ACCESS_TOKEN`.

**Verification:** Confirm database-backed operations and image write/read as the application user, including after container restart. In an isolated deployment, a deliberately non-writable test mount should fail the smoke check; do not change the live mount's permissions for the negative test. Record this as a homelab rollout result; repository tests or Compose syntax alone cannot establish host permissions, backup state, or a completed deployment.

### CI-05 — Add regression cases for input boundaries

**Priority: P1 alongside the corresponding correctness fixes; otherwise P2.**

**Impact and evidence:** Existing happy-path and import scenarios do not by themselves cover normalization collisions, explicit nulls, or all database maximum boundaries implicated by COR-01/02/05/06/08. These omissions can leave an HTTP endpoint returning a persistence or unhandled error even while ordinary workflows pass. See [`RecipeWriteService.cs`, lines 193–204](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Shared/RecipeWriteService.cs#L193-L204), [`RecipeImportService.cs`, lines 184–263](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Imports/RecipeImportService.cs#L184-L263), and the existing import tests in [`RecipeImportSpecificationTests.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/tests/WhatToCook.Api.IntegrationTests/Specs/RecipeImportSpecificationTests.cs).

**Recommendation:** Add focused parameterized HTTP integration cases alongside the affected COR fixes: empty/one/max/max+1, null and null element, Unicode/diacritic collision, and duplicate inputs. Reuse existing test fixtures and contracts; do not create a second generic validation framework or broad fuzz suite in place of specific regressions.

**Verification:** For each case assert the expected status and response body, persistence/no-partial-write behavior, and compatibility with existing successful requests. Tests should fail against the unhandled boundary and pass once the corresponding behavior is corrected.

### CI-06 — Add coverage reporting only after a baseline

**Priority: P3 / optional (originally P2).**

**Impact and evidence:** The reviewed PR workflow has no visible coverage report/gate, although the repository has coverage-analysis guidance. That absence does not prove the tests are poor or that a global percentage threshold would improve quality. See [`.github/workflows/pr-ci.yml`, lines 1–52](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/.github/workflows/pr-ci.yml#L1-L52) and [coverage-analysis guidance](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/.agents/skills/coverage-analysis/SKILL.md#L18-L55).

**Trigger and recommendation:** Consider this after the team has a stable baseline and wants review-visible evidence for changed code or known risk hotspots. Publish a Cobertura report and trend changed-lines coverage first; only introduce a calibrated warning or threshold if the report proves actionable. Do not add an arbitrary blocking 80% target or complexity gate by default.

**Verification:** Confirm PRs expose the report and identify genuinely untested changed branches with an appropriate owner. Validate that the reporting step itself does not mask test failures; any later gate should be exercised with a deliberately uncovered changed branch.

### CI-07 — Test deployment contracts instead of documentation phrases

**Priority: P2 (originally P3).**

**Impact and evidence:** Some self-hosted checks assert that documents contain expected text. Such assertions can fail on harmless wording edits without detecting a broken Compose contract, while a text match does not prove the deployment works. See [`SelfHostedWebPlatformSpecificationTests.cs`, lines 193–230](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/tests/WhatToCook.Api.IntegrationTests/Specs/SelfHostedWebPlatformSpecificationTests.cs#L193-L230).

**Recommendation:** When deployment files change, validate the effective Compose configuration and required variables (for example, `docker compose config --quiet`) and test meaningful runtime contracts such as the API UID/media path. Keep a real backup restore drill as a homelab operational check; do not claim a repository assertion substitutes for it. Preserve the existing project/volume and secret-injection contract.

**Verification:** Removing a genuinely required Compose variable or breaking a required service/path contract should fail its executable check; changing only prose should not. Record backup restoration separately with an isolated restore result and the homelab owner.

## User-experience and accessibility findings

### UX-01 — Refactor large UI state only to address a concrete problem

**Priority: P3 / optional (originally P2).**

**Impact and evidence:** The meal planner and shopping page hold substantial UI state, which can make changes and concurrent operations harder to reason about. File size/state volume alone does not demonstrate sluggish rendering, a user-visible interaction bug, or a need for a broad rewrite. See [`MealPlannerPage.razor.cs`, lines 11–75](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Web/Components/Features/Planning/Pages/MealPlannerPage.razor.cs#L11-L75) and [`ShoppingListPage.razor`, lines 1–55](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Web/Components/Features/Shopping/Pages/ShoppingListPage.razor#L1-L55).

**Trigger and recommendation:** Defer restructuring unless a concrete maintenance or interaction problem arises while changing one of these flows. If triggered, extract the affected responsibility into a focused component/coordinator, make cancellation/awaiting explicit, and expose a busy state per operation. Avoid splitting components solely to reduce line count.

**Verification:** Add or extend targeted tests for the motivating behavior, such as double-submit, disconnect during save, or rapid tab/selection changes. Confirm only one mutation occurs where required, busy state clears on completion/failure, and ordinary planner/shopping flows remain intact.

### UX-02 — Add focused accessibility checks to CI

**Priority: P2.**

**Impact and evidence:** The UI already contains ARIA/live-region and loading affordances, with tests for selected ARIA states and mobile planner layout. However, the reviewed CI workflow does not run an automated accessibility scanner or comprehensive keyboard-only scenarios. Markup attributes alone cannot establish adequate contrast, focus order/trapping, heading structure, or keyboard operability. See [the PR workflow](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/.github/workflows/pr-ci.yml#L38-L44), [`RecipeLibraryPage.razor`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Web/Components/Features/Recipes/Library/Pages/RecipeLibraryPage.razor), and the existing component tests listed under [Existing strengths and test coverage](#existing-strengths-and-test-coverage).

**Recommendation:** Add a proportionate axe-based smoke check to critical flows and explicit keyboard-only checks for library, editor, planner, shopping drawer, and modal interactions. Include reduced-motion behavior where the UI provides motion. Treat automation as an aid, not a replacement for manual accessibility review; use WCAG 2.2 AA as the interaction target.

**Verification:** Run the new checks in CI on the critical flows, verify keyboard access, visible focus, correct dialog/drawer focus behavior, and useful accessible names. Review and fix each reported violation rather than suppressing the scanner wholesale; retain periodic manual checks for issues automation cannot assess.

### UX-03 — Verify SignalR reconnect and unsaved form behavior

**Priority: P2; runtime behavior unverified.**

**Impact and evidence:** The Web application uses Blazor Interactive Server and includes a reconnect component. A dropped connection interrupts interactivity, so unsaved form behavior is worth checking, but the presence of the reconnect flow means reconnect is not proven broken and loss of user input has not been observed. See [`Program.cs`, lines 13–15](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Web/Program.cs#L13-L15), [`ReconnectModal.razor`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Web/Components/Layout/ReconnectModal.razor), and the [self-hosted connection requirements](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/docs/self-hosted-deployment.md#L20-L24).

**Recommendation:** Test reconnect in an editor and planner form before adding persistence or warning behavior. If the test demonstrates lost unsaved data, choose a proportionate response such as preserving a local draft or warning before navigation; do not introduce that complexity without evidence.

**Verification:** In a real-browser/Playwright test, interrupt connectivity for 10–30 seconds while editing, restore it, and check reconnect status, field values, and the ability to save exactly once. Repeat for the relevant form types and confirm cancellation/navigation behavior.

### UX-04 — Set mobile performance budgets from representative measurements

**Priority: P3 / optional.**

**Impact and evidence:** Recipe-library image volume and Blazor updates could affect mobile transfer or responsiveness, but the static review did not measure LCP, bytes transferred, or interaction latency on a representative device/network. Potential cost is not a measured performance defect. See [`RecipeLibraryPage.razor`, lines 1–72](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Web/Components/Features/Recipes/Library/Pages/RecipeLibraryPage.razor#L1-L72) and [`FileSystemRecipeImageStorage.cs`, lines 19–70](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Infrastructure/Storage/FileSystemRecipeImageStorage.cs#L19-L70).

**Trigger and recommendation:** Establish a budget only if representative users/devices show a problem or the product needs a defined mobile performance target. Measure a realistic weak-Wi-Fi/device profile and catalog size first; then target the largest demonstrated contributor, rather than preemptively adding image processing or changing rendering behavior.

**Verification:** Record LCP, transferred bytes, and key interaction latency with Lighthouse/WebPageTest or Playwright traces at representative catalog sizes (including 100 and 500 recipes). Re-run the same profile after any optimization and compare the result to an agreed target.

### UX-05 — Improve import-error guidance when generic messages impede recovery

**Priority: Conditional P2; keep/defer at P3 if current messages are sufficient (originally P3).**

**Impact and evidence:** The import service maps multiple external-page failures to broadly similar messages, which may leave a user unsure whether a source is unsupported, malformed, unavailable, or timing out. This is a potential usability issue; the review did not establish user confusion or an incident. See [`RecipeImportService.cs`, lines 62–79](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Imports/RecipeImportService.cs#L62-L79).

**Trigger and recommendation:** If current messages fail to give users a useful next action or support reports show repeated confusion, define safe, typed error categories and map them to actionable UI text (for example, retry a transient timeout versus choose a supported source). Preserve cancellation. Log stage/category only where useful; do not log full source URLs or fetched HTML. If current messages already support recovery, leave this as P3 rather than changing contracts for completeness alone.

**Verification:** Exercise offline/network failure, 403, timeout, unsupported host, and malformed HTML. Assert the appropriate user guidance, retry/cancel behavior, and category-level diagnostics without full URL/HTML disclosure. Confirm the normal successful import still works.

## Evidence and verification limits

The source audit was predominantly static. The 2026-11-06 review recorded a local MSBuild default configuration of `Debug` and a narrow Windows PowerShell example where an initial nonzero native-command status was followed by a successful command and final status 0. Neither observation is a GitHub-hosted CI result. The review did not run the full CI workflow, application test suite, browser accessibility/reconnect checks, security scans, or homelab deployment/restore checks. Recommendations above identify checks to perform; they are not reported as completed.

For implementation, first recheck current code and keep changes focused. Preserve existing shopping-list reconciliation semantics unless its specification is deliberately changed. Keep deployment and homelab ownership separate: application-repository changes must not replace the established secret wrapper, expand the BWS scope, or make the application consume `BWS_ACCESS_TOKEN`.

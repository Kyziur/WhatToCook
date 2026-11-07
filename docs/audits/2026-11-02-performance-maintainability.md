# Performance, architecture, documentation, and agent tooling

- **Source snapshot:** [`4eb64d7e3de01faa7bef32db74da94e7e1c50913`](https://github.com/Kyziur/WhatToCook/tree/4eb64d7e3de01faa7bef32db74da94e7e1c50913), the 2026-11-02 audit snapshot.
- **Reassessment:** the reviewed backlog checked repository and configuration evidence on 2026-11-06. Recommendations and priorities below reflect that reassessment, not necessarily the original audit's wording or priority.
- **Evidence limits:** the assessment was predominantly static. A code pattern or query shape is not evidence of a production incident or measured performance problem. No runtime performance, production, or homelab state is inferred here. Confirm current code before implementation if it has changed since the cited snapshot.
- **Priority:** P1 = address first; P2 = worthwhile next or when the stated condition applies; P3 = optional, measurement-led work. “Verify” items require evidence before choosing a remedy. Each recommendation includes a trigger, a way to verify the result, and its principal trade-off.

## Performance

### PERF-01 — P2: paginate the complete library and search flow

**Evidence.** The library endpoint limits its response to 500 recipes ([`LibraryEndpoints.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Library/LibraryEndpoints.cs#L28-L39)). The Blazor Server library page filters the recipes it has loaded into component state ([`RecipeLibraryPage.razor`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Web/Components/Features/Recipes/Library/Pages/RecipeLibraryPage.razor#L344-L354), [L579-L625](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Web/Components/Features/Recipes/Library/Pages/RecipeLibraryPage.razor#L579-L625)). The search endpoint returns an unpaged result and matches a recipe when **any** requested tag matches ([`SearchEndpoints.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Search/SearchEndpoints.cs#L44-L68)); the library UI's multi-tag filter means **all** selected tags. These are different existing behaviors, not interchangeable descriptions of one filter.

**Action.** Design pagination end to end, with a stable ordering and bounded page size for both library and search. Ensure filters, sort order, counts, and suggestions operate over the full catalog rather than silently over only the currently loaded page. Preserve the library's all-selected-tags behavior and search API's any-matching-tag behavior unless a deliberate product/API contract change says otherwise; document and regression-test any such change. Do not merely raise the 500 cap or paginate only the API while leaving the UI unaware of later pages.

**Trigger.** The library already makes recipes after its 500-item limit unavailable to this page. Prioritize a complete flow before the catalog approaches or exceeds that cap, and bound search responses as their size grows.

**Verification.** Use a catalog with more than 500 recipes. Confirm a recipe beyond the first page can be found and opened; exercise filters, multiple tags, sorting, search, and suggestions across page boundaries; check for omissions or duplicates when data changes between requests. Verify response sizes are bounded and that the documented any-tag/all-tags semantics remain intact.

**Trade-off.** Pagination adds API, UI state, and boundary-condition complexity, and server-side filtering may require query changes. It avoids a hidden catalog ceiling and unbounded search payloads; a partial migration could instead make filtering or sorting appear inconsistent.

### PERF-02 — P2: resolve the inactive output-cache annotation

**Evidence.** The library route carries `.CacheOutput()` ([`LibraryEndpoints.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Library/LibraryEndpoints.cs#L15-L19)), while API startup is the relevant place to configure services and middleware ([`Program.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Program.cs#L14-L65)). The reviewed configuration did not activate the output-cache pipeline; the annotation alone does not establish that responses are cached.

**Action.** Decide whether output caching is useful. If not, remove the inactive annotation. If so, configure the service and middleware, define cache variation and expiration, and ensure recipe create, update, and archive operations invalidate or refresh affected responses.

**Trigger.** Resolve the misleading inactive annotation during endpoint maintenance; enable caching only if repeated library reads or measurements justify its freshness and invalidation cost.

**Verification.** If caching is enabled, request the endpoint repeatedly and then create, update, and archive a recipe; each change must be reflected without waiting for an unsafe stale interval. If caching is not wanted, confirm the route no longer advertises inactive cache behavior.

**Trade-off.** Removing the annotation is simple but forgoes a potential repeated-read optimization. Enabling caching adds invalidation and variation complexity; stale recipe data is worse than a modest uncached query for a small catalog.

### PERF-03 — P3: consider a substring-search index only after query-plan evidence

**Evidence.** Search uses `NormalizedTitle.Contains(...)` ([`SearchEndpoints.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Search/SearchEndpoints.cs#L38-L42)); the reviewed model defines ordinary title indexes ([`AppDbContext.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Infrastructure/Data/AppDbContext.cs#L28-L38)). Arbitrary substring matching commonly cannot use a conventional B-tree index efficiently, but no slow production query was established.

**Action.** Capture and inspect the generated SQL and `EXPLAIN ANALYZE` on representative data first. If substring search is a demonstrated bottleneck, compare PostgreSQL `pg_trgm` with GIN against a product-approved prefix-search change; do not add a specialized index speculatively.

**Trigger.** Search latency or database work is material at the actual catalog size, and the query plan shows the title predicate is responsible.

**Verification.** Compare plans and latency with representative catalogs (for example, 1,000 and 10,000 recipes), and verify search result semantics before and after the index or query change.

**Trade-off.** A trigram index consumes disk and adds write/maintenance cost. Prefix search can be cheaper but changes what users can find. A plan test is useful evidence, not a guarantee of production latency.

### PERF-04 — P3: reduce recommendation candidates only if catalog growth makes it worthwhile

**Evidence.** Recommendation calculation loads ingredients across active, unplanned recipes before applying its result limit ([`ShoppingListService.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Shopping/Lists/ShoppingListService.cs#L25-L100)). This is a statically observed query/work shape; no runtime memory or latency incident was measured.

**Action.** If profiling identifies this load as material, constrain candidates in SQL using the shared-ingredient criteria, then rank a suitably small candidate set. Preserve the current eligibility and ranking rules rather than limiting arbitrary rows before ranking.

**Trigger.** Catalog growth or measured allocations, database transfer, or response time show that loading all active-recipe ingredients is costly.

**Verification.** Compare rows returned, allocations, and elapsed time at representative sizes such as 100, 1,000, and 10,000 recipes. Check that the same eligible recipes and ranking are produced for cases with shared and non-shared ingredients.

**Trade-off.** SQL-side filtering can reduce materialization but moves ranking assumptions into a more complex query. Early candidate limits can silently change which recipes are recommended.

### PERF-05 — P3: profile Blazor circuit and render cost before reorganizing UI

**Evidence.** The Web app uses Blazor Interactive Server ([`Program.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Web/Program.cs#L10-L20)); the recipe library holds catalog data in its stateful component ([`RecipeLibraryPage.razor`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Web/Components/Features/Recipes/Library/Pages/RecipeLibraryPage.razor#L1-L70)). Circuits retain per-session state, but the review did not establish circuit pressure, slow rendering, or a concurrency limit.

**Action.** Profile representative use first. If render work is demonstrably excessive, reduce unnecessary rerenders, use stable `@key` values where identity matters, and extract focused presentational components only where that improves the measured path.

**Trigger.** Concurrent sessions, circuit memory, allocations/GC, or render duration become a measured concern at the expected number of devices.

**Verification.** Compare allocations/GC, active SignalR connections, and median/p95 render duration under representative 5- and 20-session loads before and after a targeted change. Confirm UI state and interactions remain correct.

**Trade-off.** Smaller components and render controls can improve isolation but add component boundaries and maintenance overhead. Do not treat component size or hypothetical circuit cost as evidence of a user-visible problem.

### PERF-06 — P3: add image variants only when transfer measurements justify them

**Evidence.** Image storage writes uploaded bytes without producing resized variants ([`FileSystemRecipeImageStorage.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Infrastructure/Storage/FileSystemRecipeImageStorage.cs#L28-L70)); the Web app streams images through its image route ([`Program.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Web/Program.cs#L62-L102)). This does not prove that stored photos are 5 MB or that current page transfer is slow. Recipe tiles already use lazy loading.

**Action.** If representative measurements show oversized tile transfers, generate appropriately sized thumbnail variants (for example, 320/640 px) and consider WebP/AVIF where supported. Keep originals available for full-detail use, apply suitable cache headers, and retain lazy loading rather than replacing it.

**Trigger.** Actual image sizes and a representative mobile/network profile show image transfer materially harms page load or data use.

**Verification.** Compare transfer size and Lighthouse/LCP on representative mobile and Wi-Fi profiles with real recipe images. Check thumbnail dimensions/format, full-size detail behavior, caching, and an unsupported-format fallback.

**Trade-off.** Variants use additional storage and add upload/derivative lifecycle and format-fallback complexity. Optimizing without transfer evidence may add cost without improving the page.

### PERF-07 — P3: inspect detail-query shape before splitting or projecting

**Evidence.** Recipe details load related collections through multiple `Include`s ([`DetailsEndpoints.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Details/DetailsEndpoints.cs#L18-L33)). Joining several collections can multiply result rows, but the review did not measure the generated result size or query latency.

**Action.** Inspect generated SQL, query count, result rows/bytes, and allocations. Use `AsSplitQuery()` or a DTO projection only if measurement shows a material improvement and the resulting detail contract remains complete.

**Trigger.** Large recipes or observed p95/transfer/allocations show that the current query shape is costly.

**Verification.** Compare query count, returned row volume, allocations, and latency for representative recipes, including a stress fixture with 100 ingredients and 100 steps. Confirm all detail fields and collection ordering remain correct.

**Trade-off.** Split queries can reduce join multiplication but add round trips; projections can reduce transferred columns but increase mapping maintenance. Neither is automatically faster for ordinary small recipes.

## Architecture and boundaries

### ARC-01 — P2 when changing the affected flow; otherwise optional P3: split page responsibilities selectively

**Evidence.** The planner page code-behind and recipe editor page code-behind combine substantial page behavior ([`MealPlannerPage.razor.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Web/Components/Features/Planning/Pages/MealPlannerPage.razor.cs#L11-L75), [`RecipeEditorPage.razor.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Web/Components/Features/Recipes/Editor/Pages/RecipeEditorPage.razor.cs#L12-L75)). File size is an observed maintainability signal, not proof of a regression.

**Action.** When modifying a concrete workflow, extract only the responsibility that makes that workflow hard to reason about or test—for example planner state/orchestration or presentation-only components. Avoid introducing a broad coordinator/CQRS layer or rewriting either page because of line count alone.

**Trigger.** A change to save/regenerate or another affected flow exposes tangled state, repeated behavior, or a testability problem.

**Verification.** Move one flow at a time; add focused behavior/component tests where appropriate and compare dependencies and responsibility boundaries. Preserve the page's existing user-visible behavior.

**Trade-off.** A targeted extraction can make change impact clearer, but extra abstractions/components can scatter behavior and create more interfaces to maintain. Defer unrelated extraction; the priority may be P3 when no affected work is underway.

### ARC-02 — P2 when modifying shopping behavior; otherwise optional P3: separate shopping responsibilities only as needed

**Evidence.** `ShoppingListService` contains recommendation, aggregation, persistence, and export-related behavior ([`ShoppingListService.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Shopping/Lists/ShoppingListService.cs#L9-L100), [L325-L465](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Shopping/Lists/ShoppingListService.cs#L325-L465)). No runtime failure follows from the concentration alone.

**Action.** If a concrete shopping change crosses independently testable rules, separate pure aggregation/ranking from transactional use-case persistence. Preserve the existing transaction, regeneration, and checklist-status contracts; do not split classes just to meet a size target.

**Trigger.** A shopping behavior change is difficult to isolate, or a focused test requires a database only because pure calculation and persistence are inseparable.

**Verification.** Test aggregation/ranking without a database where practical, and run behavior-focused integration coverage for regeneration and retained item status.

**Trade-off.** Separation can clarify pure logic and transaction boundaries, but premature decomposition creates more types and coordination points. Defer to P3 if no concrete maintenance or testability pain exists.

### ARC-03 — P3: extract a use case from an endpoint only when it improves boundaries

**Evidence.** Plan endpoint handlers contain persistence/transaction behavior, and recipe editor endpoints contain application work ([`PlansEndpoints.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Planning/Plans/PlansEndpoints.cs#L75-L215), [`EditorEndpoints.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Editor/EditorEndpoints.cs#L109-L135)). EF Core in a handler does not by itself demonstrate that the domain depends on EF.

**Action.** Keep endpoint handlers as HTTP adapters where they are already simple. Extract a use case when complex validation, transaction handling, or side effects would become easier to test or maintain outside the handler; do not add a service layer mechanically.

**Trigger.** A specific handler grows enough that HTTP mapping obscures its behavior or makes the transaction/use case difficult to test.

**Verification.** Confirm endpoint tests still cover HTTP status and mapping, while focused use-case tests cover extracted behavior and transaction-sensitive outcomes.

**Trade-off.** Extraction can improve test isolation but may add indirection without benefit for a small handler.

### ARC-04 — P2 before broadening exposure: confirm the intended trust and ownership model

**Evidence.** The README gives only a short product introduction ([`README.md`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/README.md#L1-L15)); the deployment guide already describes important application-versus-homelab responsibilities ([`self-hosted-deployment.md`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/docs/self-hosted-deployment.md#L1-L24)). The reviewed state therefore does not support claiming that ownership boundaries are wholly undocumented. Identity and access decisions still matter if the product moves beyond its current household/trusted-network model.

**Action.** Before adding accounts, mobile/public access, or broader API reachability, document the intended users, authorization boundary, trusted networks, API/Web relationship, and homelab-owned backup responsibility. Keep the established deployment and secret boundary; this is a product/trust decision, not an instruction to move homelab secrets into the application repository.

**Trigger.** Planned expansion of users or network exposure, or a design change involving authentication, public endpoints, or synchronization.

**Verification.** Review the resulting decision before implementing the exposure/authentication change; verify each data and image route against the chosen identity/access model and confirm operational ownership with the homelab operator.

**Trade-off.** A decision record costs little but can become stale if it duplicates the deployment runbook. Keep it short, decision-focused, and aligned with the canonical operational contract.

### ARC-05 — P3: validate API contracts without centralizing DTOs by default

**Evidence.** Recipe contracts are feature-local ([`RecipeContracts.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Shared/RecipeContracts.cs#L1-L18)); planning endpoint contracts are declared in their feature ([`PlansEndpoints.cs`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Planning/Plans/PlansEndpoints.cs#L428-L464)). Distribution across features is not itself evidence of inconsistent contracts.

**Action.** Keep DTOs near the feature that owns them. If independently maintained clients or recurring contract drift make it useful, add a focused OpenAPI or client/API compatibility check; do not create a shared contracts project solely to centralize types.

**Trigger.** A consumer depends on the public API contract and a change has a demonstrated risk of client/API mismatch.

**Verification.** Validate the affected schema or endpoint contract against the consumer, ideally with a focused compatibility test or snapshot.

**Trade-off.** Contract checks catch accidental incompatibility but create a baseline to maintain. Centralizing unrelated feature DTOs can couple otherwise independent features.

## Documentation

### DOC-01 — P2: fill practical quickstart gaps without duplicating operations guidance

**Evidence.** The README is short but provides a basic start and links to deployment guidance ([`README.md`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/README.md#L1-L15)); the canonical deployment guide contains operational ownership and recovery details ([`self-hosted-deployment.md`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/docs/self-hosted-deployment.md#L1-L69)). The review did not conclude that the deployment runbook should be copied into the README.

**Action.** Add only missing, useful maintainer steps for local development, test entry points, and common troubleshooting. Keep one concise quickstart path and link to the canonical deployment guide for operations; preserve its ownership and secret-injection contract.

**Trigger.** A new maintainer cannot complete a normal local setup/test or diagnose a common failure from the README and linked canonical instructions.

**Verification.** Have someone unfamiliar with the setup follow the documented development path from a clean checkout and confirm links and commands point to the right source of truth.

**Trade-off.** A concise guide reduces onboarding friction; duplicating setup, secrets, or deployment steps in multiple files invites drift.

### DOC-02 — P2 only if maintainers need a durable architecture reference; otherwise P3

**Evidence.** The reviewed README and deployment guide focus on setup/operations rather than a full architecture explanation ([`README.md`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/README.md#L1-L15), [`self-hosted-deployment.md`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/docs/self-hosted-deployment.md#L1-L69)); OpenSpec specs and source code also carry design knowledge. Their existence may already answer some questions.

**Action.** Ask which stable decisions or boundaries are hard to recover from existing specs. If there is a real gap, add a short architecture overview or decision record and link to canonical specs; do not add a C4/ADR/domain-document set by default.

**Trigger.** Repeated onboarding or change-review confusion about a durable boundary, or a significant architecture change that requires a recorded decision.

**Verification.** Review the proposed page with maintainers and ensure it points to the relevant implementation/spec rather than restating details that will drift.

**Trade-off.** A concise map can reduce rediscovery; extra documents create another representation to maintain. Priority can be P3 if existing specifications suffice.

### DOC-03 — P2 if product status is a recurring source of confusion; otherwise P3

**Evidence.** OpenSpec change tasks can represent in-progress work ([`tasks.md`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/openspec/changes/add-recipe-import/tasks.md#L1-L22)); the OpenSpec config has also been identified as template-oriented rather than a product capability index ([`config.yaml`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/openspec/config.yaml#L1-L25)). An unfinished task file does not by itself prove that a feature is absent from the shipped product.

**Action.** If maintainers or users need a product-level status map, add a compact page linking each relevant capability to its canonical spec and identifying shipped, planned, design-only, or blocked status. Update only affected entries when capability status actually changes; do not mirror the entire OpenSpec archive.

**Trigger.** A user or contributor cannot distinguish shipped behavior from a proposal or stale change task using current docs/specs.

**Verification.** Check a sample of listed capabilities against current routes and canonical specs; ensure the page has a clear owner/update point and does not mark incomplete tasks as proof of product state.

**Trade-off.** A status map improves discoverability but can itself become stale. Skip it if existing specs and release information meet the need.

### DOC-04 — P2: replace project-mismatched template context

**Evidence.** The OpenSpec configuration and agent instructions contain template/MAUI-era context not aligned with WhatToCook ([`openspec/config.yaml`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/openspec/config.yaml#L1-L36), [`AGENTS.md`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/AGENTS.md#L25-L36)).

**Action.** Update those active defaults to describe the current product and stack (.NET 10, Blazor Interactive Server, PostgreSQL, and the two deployable application services) and state the application/homelab boundary. Remove irrelevant MAUI/React/Angular/reusable-template guidance from the active context. Avoid copying the detailed deployment runbook into OpenSpec defaults.

**Trigger.** Before relying on OpenSpec-generated change context or agent instructions for new work; keep the correction localized to files that are actually active.

**Verification.** Review a newly generated change description and confirm it names the current product/stack and ownership boundary without steering contributors toward unrelated template technology.

**Trade-off.** Accurate context prevents irrelevant recommendations; overloading global instructions with operational detail increases context and drift. Coordinate with AGENT-05, the same context mismatch from the agent-tooling perspective.

### DOC-05 — P3: preserve the deployment note unless real duplication appears

**Evidence.** The Raspberry Pi deployment note links to the more comprehensive self-hosted deployment guide ([`deploy-raspberry.md`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/docs/deploy-raspberry.md#L1-L24), [`self-hosted-deployment.md`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/docs/self-hosted-deployment.md#L1-L69)). The reassessment did not find that two complete deployment runbooks must be merged.

**Action.** Keep the platform note as a short pointer or platform-specific supplement. Consolidate only repeated details that actually require parallel edits; retain the self-hosted guide as canonical for shared deployment variables and ownership.

**Trigger.** A concrete change has to be made in multiple documents, or the two files diverge on a shared contract.

**Verification.** Check that the canonical guide has one authoritative set of shared variables/ownership rules and that the Raspberry note contains no contradictory duplicate instructions.

**Trade-off.** Removing a useful platform note or migrating it wholesale can lose context and churn links; selective de-duplication reduces drift without a documentation reorganization project.

### DOC-06 — P3: update useful docs with behavior changes; do not require a new skill or gate by default

**Evidence.** The source snapshot's agent guidance discusses index and documentation workflow ([`AGENTS.md`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/AGENTS.md#L202-L208)); the reviewed backlog found no justification for a separate documentation skill or broad mandatory evidence gate.

**Action.** Update the canonical user/developer docs or spec when a behavior, requirement, or operational contract changes. Add a dedicated documentation-maintenance skill, link checker, or required checklist only if recurring omissions show that existing review practice is insufficient.

**Trigger.** A behavior/contract change makes existing instructions wrong, or repeated review findings demonstrate a specific documentation-quality gap.

**Verification.** Review the changed behavior's directly relevant docs and links as part of that change; if adding automation, demonstrate that it catches a real class of stale/broken documentation without duplicating existing checks.

**Trade-off.** Timely updates reduce stale instructions. A new skill or hard gate has ongoing maintenance cost and may turn small documentation-only changes into disproportionate process work.

## Agent tooling and workflow

### AGENT-01 — P3: use live Repowise freshness signals, not the superseded stale-index assertion

**Evidence.** The original audit claimed the generated Repowise section was already stale. That claim was no longer current at the 2026-11-06 review: review-time generated metadata reported an index through the audited commit (`4eb64d7`) on 2026-11-06. The original finding's `AGENTS.md` lines are the relevant generated section ([`AGENTS.md`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/AGENTS.md#L202-L231)); treat freshness as time-dependent rather than carrying forward the original date comparison.

**Action.** Do not hand-edit or delete the generated Repowise section because of the old claim. At the time of a task, check tool-provided freshness/staleness signals and re-read live source when those signals identify a mismatch. The index is useful for navigation, not a substitute for current evidence when it reports stale or approximate data.

**Trigger.** A Repowise response reports stale/approximate scope, the indexed commit differs from the source being changed, or the tool's freshness metadata changes.

**Verification.** Confirm the response's indexed revision/scope before relying on indexed claims; read the affected current source when freshness is uncertain. Do not create a recurring manual check solely because of the superseded stale-date observation.

**Trade-off.** Checking a live signal preserves the value of generated context without needless duplicate work. Treating every index snapshot as authoritative risks stale guidance; manually maintaining generated content risks drift from its generator.

### AGENT-02 — P3: keep review and evidence proportional to change risk

**Evidence.** Agent instructions describe implementation and handoff responsibilities ([`AGENTS.md`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/AGENTS.md#L76-L84), [L183-L200](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/AGENTS.md#L183-L200)). The reviewed backlog does not support making an `evidence.md` file or an independent reviewer a mandatory artifact for every small change.

**Action.** Retain a clear handoff/review path. Use independent review and explicit acceptance-criterion evidence especially for data, network, and deployment changes; for small changes, concise references to exercised checks and decisions are sufficient. Do not make the author their own independent reviewer.

**Trigger.** Apply stronger evidence to higher-impact or difficult-to-reverse work; keep low-risk changes lightweight.

**Verification.** For a representative substantial change, map relevant acceptance criteria to diff and verification evidence, identify any waiver, and show that review is independent. Confirm the process does not force a heavyweight artifact on a trivial change.

**Trade-off.** Proportional review improves confidence without slowing every edit. A fixed evidence ceremony can consume time and encourage boilerplate rather than useful evidence.

### AGENT-03 — P3: deduplicate instruction only after confirming a real conflict

**Evidence.** General agent instructions and skills contain related testing/handoff material ([`AGENTS.md`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/AGENTS.md#L100-L125), [`run-tests/SKILL.md`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/.agents/skills/run-tests/SKILL.md#L78-L99), [`verification-fast-path/SKILL.md`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/.agents/skills/verification-fast-path/SKILL.md#L31-L60)). Similar wording is not proof of an inconsistent duplicate.

**Action.** Compare instructions for contradictory commands, ownership, or acceptance criteria before removing anything. If conflicting sources are found, make the general policy authoritative and leave skills with task-specific execution details.

**Trigger.** A contributor receives conflicting guidance or the same procedural change must repeatedly be applied to separate copies.

**Verification.** Review the affected instructions side by side and confirm one unambiguous policy remains, while task-specific skill guidance is still available.

**Trade-off.** A single policy reduces drift, but over-centralization makes task-specific guidance harder to discover. Do not perform a broad deduplication pass based on resemblance alone.

### AGENT-04 — P2 only when portability is required: remove the absolute checkout dependency

**Evidence.** The checked-in MCP configuration references a local Windows checkout path ([`.mcp.json`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/.mcp.json#L1-L14)).

**Action.** If the MCP service is expected to work from different checkout directories, WSL, or other developers' machines, use a supported cwd-relative/portable configuration or documented local override. Do not assume an unverified relative-path feature exists.

**Trigger.** A developer moves the checkout or attempts to use the committed MCP configuration outside the path it currently names.

**Verification.** Launch Repowise through the supported MCP client from a clean clone in a different directory (and WSL if in scope); confirm it resolves the intended repository rather than another checkout.

**Trade-off.** Portability reduces machine-specific setup, but changing path resolution without testing the actual MCP host can break a currently working local configuration. If this is a single fixed environment, leave it alone.

### AGENT-05 — P2: align agent/OpenSpec context with the current product

**Evidence.** The OpenSpec configuration and agent context retain template/MAUI references inconsistent with the application ([`openspec/config.yaml`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/openspec/config.yaml#L1-L36), [`AGENTS.md`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/AGENTS.md#L25-L36)). This is the agent-context counterpart of DOC-04.

**Action.** Correct active defaults to identify WhatToCook's actual .NET/Blazor/PostgreSQL stack and application/homelab boundary; remove irrelevant technology and reusable-template assumptions. Keep operational specifics in their canonical owner documentation.

**Trigger.** Before new OpenSpec changes or agent work relies on these defaults, and whenever the product stack or ownership boundary changes.

**Verification.** Generate/review a sample change context and confirm its product assumptions and technology recommendations are relevant; verify deployment/secret ownership remains unchanged.

**Trade-off.** Accurate defaults improve agent suggestions; duplicating a full architecture or homelab runbook in multiple prompts creates drift and unnecessary context.

### AGENT-06 — P3: treat harness-specific commands as adapters unless a conflict is demonstrated

**Evidence.** OpenSpec apply guidance appears in both an `.omp` command and an `.agents` skill ([`opsx-apply.md`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/.omp/commands/opsx-apply.md#L1-L22), [`openspec-apply-change/SKILL.md`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/.agents/skills/openspec-apply-change/SKILL.md#L1-L10)). Their presence in different harness locations does not alone establish that one is a redundant generated copy.

**Action.** Identify the canonical OpenSpec behavior and whether either file is generated. If a concrete behavior drift or duplicate maintenance burden is found, generate harness adapters from a maintained source where supported; do not hand-edit generated files or consolidate by assumption.

**Trigger.** A process change must be repeated, the two entry points disagree, or maintainers cannot identify their source of truth.

**Verification.** Compare the invoked behavior in both harnesses and, if generation is adopted, verify regeneration reproduces both adapters from the canonical source.

**Trade-off.** Generated adapters reduce drift but require generator/version maintenance and may not express harness-specific differences. Keeping deliberate adapters is preferable to an unproven generator project.

### AGENT-07 — P3: use existing documentation and homelab review practice before adding skills

**Evidence.** The repository has task-specific skills ([`dotnet-quality-governor/SKILL.md`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/.agents/skills/dotnet-quality-governor/SKILL.md#L1-L10)) and a concise README ([`README.md`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/README.md#L1-L15)). The reviewed backlog recommends using the existing appropriate homelab process/checklist rather than assuming that new skills are missing necessities.

**Action.** When a documentation or deployment-boundary change exposes a specific repeated omission, improve the current checklist or skill at its source. Add a new documentation-maintenance or homelab-boundary skill only if an existing process cannot cover the gap. Preserve the established boundary: homelab owns host/network/Komodo/secret injection and backups; the application must not require `BWS_ACCESS_TOKEN`.

**Trigger.** Repeated review findings or a real change-risk gap remains after using the existing process, not merely because a skill could be added.

**Verification.** Apply the current checklist to a representative documentation and deployment-boundary change; confirm it catches the identified gap without duplicating existing guidance or crossing ownership boundaries.

**Trade-off.** A focused checklist can reduce omissions. Each new skill also costs selection/context overhead and becomes another source to maintain.

### AGENT-08 — P3: reduce skill-selection cost only after measuring it

**Evidence.** The skill catalog includes substantial guidance and references (for example, [`coverage-analysis/SKILL.md`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/.agents/skills/coverage-analysis/SKILL.md)); the original audit's size observation is qualitative, not a measured context-cost or wrong-skill incident.

**Action.** If selection/loading is demonstrably costly, make skill triggers and summaries concise, keep deep reference material on demand, and identify the maintained source of truth. Retain specialized content that materially improves results; do not remove skills because a file is long.

**Trigger.** Repeated irrelevant skill activation, missed relevant skill use, or measured context/token overhead in representative tasks.

**Verification.** Compare skill selection, loaded context, and task outcomes for a representative set of common tasks before and after changes; include cases that require the specialized references.

**Trade-off.** A smaller entry point can reduce noise, but excessive compression may hide important instructions or make the skill harder to select. Catalog size alone does not determine quality.

## Evidence caveats

The source audit and the 2026-11-06 reassessment were primarily static. No finding in this chapter should be read as a measured latency, memory limit, image-transfer cost, production incident, or externally verified deployment condition. The cited source commit makes the original observations reproducible; recheck the live implementation and choose optional work only when its stated trigger is met. No requirement is made here to create an evidence file or other artifact for every small change.
## Context

The application already has a complete manual recipe flow: recipe CRUD, ingredient normalization, tag suggestions, archive behavior, and HTTP-backed Blazor screens. Imported recipes must end in the same `recipes`, `recipe_ingredients`, `recipe_steps`, and `recipe_tags` tables, but they cannot be written there immediately because extraction can be incomplete or wrong.

Current constraints:
- Recipe writes are implemented directly inside recipe editor endpoints, so import finalization would currently have to duplicate domain rules.
- The stack is .NET-first, while likely future OCR tooling such as MarkItDown or vision extraction may run outside the main ASP.NET Core process.
- Tests already follow capability-level integration specs against PostgreSQL Testcontainers and component specs for Blazor.
- The user explicitly wants both URL scraping and future photographed recipe imports, but the smallest safe implementation slice is URL draft import plus manual review/finalize.

Stakeholders:
- User importing recipes from favorite cooking pages and handwritten/photo-based sources
- Future implementation work that will add OCR/MarkItDown-style extraction
- Reviewers who need clear separation between extracted drafts and trusted recipe records

## Goals / Non-Goals

**Goals:**
- Introduce a draft-based import model that isolates uncertain extracted data from trusted recipe data.
- Support URL-driven recipe import in the first implementation package with strong test coverage.
- Reuse existing recipe write behavior so imported recipes obey the same invariants as manual recipe creation.
- Preserve a pluggable extraction boundary so future photo imports can use OCR or document converters without redesigning the domain flow.
- Surface extraction issues and finalize blockers explicitly so the client can guide the user through corrections.

**Non-Goals:**
- Full OCR or image-based extraction in the first implementation package
- Background job orchestration or a separate worker service in the first package
- Generic internet-scale crawling, feed discovery, or bulk scraping of external sites
- Automatic finalization without review

## Decisions

### 1. Use a draft-first import model instead of writing directly into recipe tables

Decision:
- Imported data will be stored in dedicated import draft tables until the user explicitly finalizes the draft.

Rationale:
- Extraction can miss servings, misread units, or pull non-recipe content from pages.
- Existing recipe tables enforce invariants that are correct for trusted data but too strict for intermediate extraction output.

Alternatives considered:
- Direct write into `recipes` followed by later editing: rejected because it pollutes the main catalog with low-confidence data.
- Purely client-side draft state: rejected because review and finalize should survive refreshes and be testable through the API.

### 2. Extract shared recipe persistence logic into a reusable backend service

Decision:
- Final recipe creation and update rules will move behind a shared recipe write service used by both the existing editor endpoints and import finalization.

Rationale:
- Import finalization must reuse duplicate-title checks, ingredient normalization, supported units, and related persistence behavior.
- Duplicating the logic in import endpoints would create drift and inconsistent validation outcomes.

Alternatives considered:
- Keep logic in static endpoint helpers and call them indirectly: rejected because endpoint-private helpers are already too coupled to the editor feature.

### 3. Make extraction source-specific through adapters

Decision:
- Import creation will call an adapter abstraction that converts one source into a `RecipeImportDraftData` shape.
- The first package will provide a URL adapter pipeline with an initial AniaGotuje-specific extractor and a generic failure path.

Rationale:
- Recipe pages vary structurally, and future image imports will require a different extraction implementation.
- A small adapter seam keeps the current code pragmatic without prematurely introducing a distributed worker.

Alternatives considered:
- One generic scraper/parser for all sites: rejected because it is too fragile for real recipe pages.
- Build the full worker/OCR system first: rejected because it delays the first usable import slice.

### 4. Keep the first implementation package synchronous inside the API

Decision:
- URL import in the first package will fetch, extract, persist the draft, and return it synchronously from the API.

Rationale:
- It is the fastest path to a testable vertical slice.
- It avoids adding queueing or worker orchestration before we have validated the draft/finalize UX and contracts.

Alternatives considered:
- Background job plus polling from day one: rejected as unnecessary complexity for the first slice.

### 5. Model confidence as field-level issues and blockers, not as one recipe-wide percentage

Decision:
- The API will expose per-field issues and a draft-level finalize readiness flag instead of relying on one aggregate confidence score.

Rationale:
- One bad title or missing servings matters more than several high-confidence steps.
- The existing domain rules already define true blockers for final recipe persistence.

Alternatives considered:
- One numeric confidence score for the whole draft: rejected because it is too lossy for review UX and validation.

## Risks / Trade-offs

- [Risk] Site-specific URL extraction can be brittle when upstream HTML changes. -> Mitigation: isolate extractors per source, keep the first adapter narrow, and fail with actionable errors instead of silently creating bad drafts.
- [Risk] The draft schema may grow when photo imports land. -> Mitigation: store source metadata and keep draft child collections separate from final recipe tables.
- [Risk] Moving recipe write behavior into a shared service can regress existing manual CRUD flows. -> Mitigation: keep the existing recipe API integration suite green and extend it with finalize coverage.
- [Risk] Synchronous fetch-and-extract can become slow for complex sources. -> Mitigation: keep the first package URL-only and leave worker orchestration for a later package.

## Migration Plan

1. Add new import draft entities and EF Core migration.
2. Introduce shared recipe write service and switch existing editor endpoints to use it.
3. Add URL import extractor abstraction and initial adapter implementation.
4. Add import draft endpoints: create from URL, read draft, update draft, finalize draft.
5. Add integration tests for import lifecycle and duplicate-title conflict handling.
6. Leave photo-import tasks pending, with draft schema and adapter boundaries ready for the next package.

Rollback strategy:
- If import endpoints regress, remove the new route mappings and revert to the previous migration snapshot.
- Because imported drafts live in isolated tables, rollback does not require mutating trusted recipe data already stored in `recipes`.

## Open Questions

- Should the eventual photo import keep one draft per recipe page set or support multiple recipes discovered in one upload batch?
- Do we want to persist the raw fetched HTML for troubleshooting, or is normalized extracted markdown/text enough for MVP?

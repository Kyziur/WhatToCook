## Why

The current recipe import flow supports a small set of source-specific websites. Users discover recipes elsewhere, frequently on phones, and manually transferring them is slow. Many recipe websites expose Schema.org `Recipe` JSON-LD that can be extracted deterministically before a user reviews and saves the draft.

## What Changes

- Add a generic structured-data extraction path for public recipe pages, retaining specialized adapters when appropriate.
- Support single, array, and `@graph` JSON-LD layouts, ordered instructions, missing-value reporting, and pages containing multiple recipes.
- Reuse the existing draft correction and finalization workflow; never trust raw scraped content directly.
- Define secure arbitrary-host fetching as a **mandatory release gate** before broadening beyond current allowlisted hosts.
- Add mobile share-to-import with explicit confirmation and manual paste fallback.
- Define source/title duplicate guidance and actionable recovery for unsupported sites.
- Supply detailed user stories and PENDING traceability for future implementation.

## Capabilities

### New Capabilities
None.

### Modified Capabilities
- `recipe-imports`: extend draft-based imports with generic structured extraction, safe network fetching, phone sharing, duplicate handling, and error recovery.

## Dependencies / Integration

The existing [add-recipe-import](../add-recipe-import/proposal.md) change is still active and owns the base `recipe-imports` capability. The canonical file `openspec/specs/recipe-imports/spec.md` is not yet present. **Do not sync or archive this delta before that base change is completed and the capability exists in canonical specs.** Treat this as a separate, design-only follow-up to avoid editing another change's uncompleted work.

## Impact

Future work touches import page fetching and SSRF controls, generic JSON-LD parser, adapter selection, draft review, source duplicate detection, mobile share/PWA experience, error messages, and PostgreSQL/browser tests.

## Non-Goals

- No implementation, external network crawling, schema migration, or tests in this design-only change.
- No AI/LLM dependency, bulk scraping, image OCR, video/PDF imports, or automatic downloading of copyrighted images.
- No automatic finalization or execution of remote HTML/scripts.

## Product outcome

Users can submit recipes from many structured websites with minimal manual retyping and an explicit correction step. See [user-stories.md](user-stories.md).

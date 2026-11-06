## Context

The repository has a draft-first recipe import workflow with URL adapters and shared finalization rules. The existing `add-recipe-import` OpenSpec change remains active, with photo import intentionally pending. Generic URL extraction can increase usability substantially, but also broadens the SSRF and untrusted-content threat model.

**Dependency:** `recipe-imports` is not yet a canonical capability file. This change uses **ADDED Requirements** named uniquely so its delta can be synchronized after `add-recipe-import` supplies the base canonical spec. Do not mark either change archived prematurely.

## Goals / Non-Goals

**Goals**
- Prefer deterministic Schema.org/JSON-LD parsing from public recipe pages.
- Continue using reviewable import drafts and existing recipe validation rather than bypassing trust boundaries.
- Handle common structured-data layouts and honest partial/unsupported outcomes.
- Maintain or improve outbound-fetch SSRF defenses, bounded parsing and markup sanitization.
- Provide a convenient URL share/paste entry point on phones.

**Non-Goals**
- No implementation or schema changes now.
- No universal success claim for websites that block automation or omit structured recipe metadata.
- No automatic OCR, AI, social-video imports, or arbitrary site crawling.

## Proposed decisions and trade-offs

### 1. Add generic structured-data extraction behind existing adapter boundary
Try an appropriate source-specific adapter where supported; otherwise detect valid Schema.org `Recipe` objects in JSON-LD. Keep the result mapped to the existing draft data model and issue/blocker approach. Multiple `Recipe` objects require an explicit choice rather than silent first-match selection.

### 2. Keep one trusted finalization pathway
Draft review persists corrections independently of recipe tables. Finalization reuses the shared recipe writer's validation and normalized title uniqueness. Concurrent/repeated finalization must be idempotent, e.g. through draft state and transaction constraints.

### 3. Treat arbitrary outbound fetch as a security boundary
Before any generic-host support, validate URL schemes, special address families, and the **actual connected IP** against internal-address restrictions to address DNS rebinding. Redirects remain disabled for the first slice unless each hop can be revalidated. Keep strict content-type, total-response-size, time, nesting and processing limits. HTML/JSON-LD is parsed as data only and is never executed or rendered as raw markup.

### 4. Share-target is a client capability, not an assumption
Mobile share integration depends on HTTPS origin, PWA manifest/share-target support and mobile browser/OS behavior. Do not promise support merely by adding a Blazor route. Always retain a manual URL paste entry point; confirm the URL before network fetching.

### 5. Keep duplicate detection helpful, not destructive
Use normalized source URLs without discarding meaningful path/query semantics; also check normalized titles. A matching URL suggests an existing recipe, but users must retain control over whether to open a record or continue reviewing a draft.

## Risks / Trade-offs

- **SSRF via generic hosts**: release blocker until connection-level destination protections are independently verified.
- **Fragile website markup**: only claim import when valid recipe fields were extracted and the draft can be reviewed.
- **Nested/large JSON-LD payloads**: cap total bytes and parsing depth; fail gracefully.
- **Client share behavior differences**: validate on target Android/iOS browsers with HTTPS and an installed PWA where applicable.
- **OpenSpec dependency conflicts**: synchronize this change only after the base `recipe-imports` capability reaches canonical specs.

## Future delivery sequence

1. Finalize existing `add-recipe-import` contract and canonical `recipe-imports` capability.
2. Confirm structured-data field mapping and multiple-recipe UX.
3. Harden and test the fetcher before accepting arbitrary recipe domains.
4. Add generic parser and the existing draft review end-to-end workflow.
5. Add duplicate warnings and phone share integration with paste fallback.
6. Validate tests/traceability and sync the delta only after implementation.

## Open Questions

- Should source-specific adapters take precedence over valid JSON-LD when both are present, or should the richest validated extraction be chosen?
- What UI is best for pages containing multiple recipes?
- Is a mobile PWA installation required for share-target support or can a launcher/link route cover initial needs?
- What is the acceptable supported-sites baseline and success rate before calling the feature "universal"?

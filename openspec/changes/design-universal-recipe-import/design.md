## Context

The current `add-recipe-import` change implements URL drafts, source adapters and finalization but still tracks unfinished photo/OCR tasks. This proposal needs the **implemented URL/draft baseline**, not OCR. A canonical `recipe-imports` file is not yet available.

## Goals / Non-Goals

**Goals:** deterministic Schema.org `Recipe` imports, multiple-candidate selection, draft review, safe arbitrary-domain fetching, phone share/paste, conservative duplicate detection.

**Non-Goals:** no runtime implementation in this design change, OCR, LLM extraction, bulk crawling, remote script execution, automatic trusted recipe creation, or automatic image downloads.

## Decisions

### Do not block generic URL import on unfinished OCR

This proposal may be merged as **design-only backlog**. Before implementation/sync, the implemented URL/draft portion of the original `add-recipe-import` change must become canonical; move its remaining photo/OCR tasks into a separate future change (or an equivalent approved scope split). Do not archive an incomplete change or sync this delta before the base canonical capability exists. Photo OCR is not a dependency of generic JSON-LD extraction.

### Retain adapters and use deterministic JSON-LD extraction

Retain source-specific adapters, with generic parsing for other supported public recipe pages. Recognize `Recipe` objects in root, arrays or `@graph`, string/array `@type`, and ordered text/`HowToStep`/`HowToSection`. Uncertain fields remain reviewable draft data with blockers; do not fabricate values.

### Exactly one candidate becomes one persisted editable draft

If extraction returns one plausible recipe, persist the ordinary draft. Multiple candidates produce a **temporary persisted import attempt** with bounded candidate summaries/IDs and a `selection required` response. Selecting one yields the single normal draft. Repeating selection for the same attempt/candidate returns the same draft; choosing a different candidate from the already selected attempt returns an explicit conflict and the selected draft, without creating or overwriting another. Unselected attempts expire after **24 hours** and cannot be selected afterward. Cancel/expire creates no trusted recipe; expired candidate data may be cleaned. This is separate from the trusted recipe tables and preserves existing finalization validation.

### Network security is a release gate

For arbitrary domains only HTTP(S) public destinations, verified both at resolution and **actual connection**; block loopback, private/link-local, metadata service, ambiguous IP forms and rebinding. **Disable redirects** for MVP and report the limitation. Bound connect/total timeout, bytes, content types, JSON nesting and candidate count. Parse untrusted HTML/JSON-LD as data only; do not execute scripts or render unescaped source markup.

### URL equivalence is conservative and advisory

Normalize host case, default ports, fragments and known tracking parameters (`utm_*`, `fbclid`, `gclid`). Preserve scheme, meaningful path, and unknown/meaningful query parameters. False negatives are acceptable; false positives must not silently merge distinct recipes. A source match only suggests the existing recipe; normalized title uniqueness still blocks finalization when applicable.

### Mobile sharing has explicit compatibility fallback

Share-target registration requires compatible OS/browser, HTTPS origin and supported PWA installation. Shared URLs prefill a confirmation UI before fetching. Manual URL paste remains a usable fallback, with mobile-device compatibility tests.

## Risks and mitigations

- Arbitrary outbound URL support expands SSRF exposure: prohibit rollout without connection-level security tests.
- Multiple recipe candidates may leave temporary records: bound count/size and use 24-hour expiry/idempotent selection.
- Incorrect URL normalization merges different recipes: preserve meaningful components and use soft warnings.
- Incomplete OCR tasks in base change: split before canonical sync rather than require OCR.
- PWA share behavior differs by browser: verify explicit supported devices and manual fallback.

## Future delivery sequence

1. Separate/sync working URL-draft base from photo backlog, before this delta is synced.
2. Agree candidate selection lifecycle and extraction mapping.
3. Harden network fetch and bounded parsing; test hostile destinations and payloads.
4. Implement generic parser, temporary attempts and idempotent draft selection.
5. Add mobile sharing, duplicate guidance, error recovery and E2E tests; sync only after base canonical exists.

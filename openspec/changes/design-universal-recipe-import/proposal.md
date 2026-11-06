## Why

WhatToCook imports recipes from a few hard-coded sources; many other pages provide Schema.org `Recipe` data that could save manual retyping.

## What Changes

- Add generic deterministic JSON-LD extraction while retaining existing adapters and reviewable drafts.
- Handle pages with multiple recipes via a **24-hour temporary candidate-selection attempt**; only a selected candidate becomes a draft.
- Require connection-level SSRF defenses, bounded input and safe markup before accepting generic domains.
- Support mobile share-to-import with explicit URL confirmation and a manual paste fallback.
- Offer conservative URL/source duplicate warnings, never automatic source-based merging or blocking.
- Reuse validated, repeat-safe draft finalization.

## Capabilities

### New Capabilities
None.

### Modified Capabilities
- `recipe-imports`: generic structured extraction, candidate selection, safe fetching, mobile entry and duplicate guidance.

## Dependencies / Integration

The existing [add-recipe-import](../add-recipe-import/proposal.md) change still has unfinished **photo/OCR** tasks. These do **not** block merging this design-only backlog proposal or implementing generic extraction once the URL/draft base is canonical. Before synchronizing this delta, sync the implemented URL/draft foundation and move pending OCR work into a separate future OpenSpec change; do not archive incomplete changes.

## Impact

Future URL fetcher, JSON-LD parser, attempt persistence, import draft APIs, mobile share UX and PostgreSQL/security/E2E tests.

## Non-Goals

- **No implementation yet** and no runtime, migration or test changes in this PR.
- No OCR, generative AI parsing, bulk crawling, image downloads or automatic trusted recipe creation.

## Product outcome

A user can import a structured recipe and select the right one when a page contains several. Full product detail: [user-stories.md](user-stories.md).

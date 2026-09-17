## Why

Manual przepisywanie receptur z ulubionych stron i własnych przepiśników jest czasochłonne, a obecny system pozwala dodać przepis tylko przez ręczne wypełnienie edytora. Potrzebujemy kontrolowanego importu, który przyspieszy zasilanie biblioteki przepisów bez psucia jakości danych domenowych.

## What Changes

- Add a recipe import draft workflow that keeps extracted recipe data outside the primary recipe tables until the user reviews and finalizes it.
- Add URL-based recipe import that fetches a recipe page, extracts draft fields, and preserves the original source URL.
- Add review and correction behavior for imported drafts, including confidence-driven issues and finalize blockers before creating a real recipe.
- Establish source-adapter boundaries for future image-based imports from photographed notebook pages and OCR/MarkItDown-style extraction.
- Reuse the existing recipe write rules for finalization so imported recipes obey the same validation, normalization, and duplicate-title constraints as manually created recipes.

## Capabilities

### New Capabilities
- `recipe-imports`: create, review, correct, and finalize imported recipe drafts from external sources such as recipe URLs and photographed recipe pages.

### Modified Capabilities
- None.

## Impact

- Affects backend feature slices under `src/WhatToCook.Api/Features/Recipes` by introducing import-specific endpoints, extraction adapters, and shared recipe write services.
- Adds new relational persistence for import drafts and related extracted fields in `AppDbContext`, plus migrations and integration tests.
- Introduces an explicit architecture seam for future OCR/document extraction without coupling the current API directly to a Python or LLM runtime.
- Creates a new test surface for capability-level import scenarios and pending photo-import traceability.

## 1. Import Draft Foundation

- [x] 1.1 Add import draft entities, status model, and EF Core persistence for draft fields, ingredients, steps, and issues
- [x] 1.2 Extract shared recipe write logic from recipe editor endpoints so manual save and import finalization use the same validation and persistence rules
- [x] 1.3 Add import API contracts and route mapping for draft create/read/update/finalize behavior

## 2. URL Import MVP

- [x] 2.1 Implement URL import source adapters with an initial supported extractor for recipe pages used in tests and a clear unsupported-source failure path
- [x] 2.2 Implement create-from-URL, reopen-draft, update-draft, and finalize-draft endpoints with blocker evaluation
- [x] 2.3 Add PostgreSQL-backed integration tests covering URL import success, draft correction, finalize success, duplicate-title conflicts, and invalid URL handling

## 3. Photo Import Follow-up

- [ ] 3.1 Add photo-based draft source upload endpoints and persisted source asset records
- [ ] 3.2 Introduce a pluggable raw extraction boundary for OCR/MarkItDown-style processing of photographed recipe pages
- [ ] 3.3 Add pending capability-level traceability entries and later automated tests for photo-import scenarios

## 4. Client Review Flow

- [x] 4.1 Add Blazor services and screens for opening an import draft, correcting extracted fields, and finalizing into a recipe
- [x] 4.2 Add component and end-to-end tests covering the import review flow and finalize outcomes

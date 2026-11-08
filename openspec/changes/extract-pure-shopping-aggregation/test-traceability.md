# ARC-02 test traceability

| Requirement | Automated check | State |
| --- | --- | --- |
| Numeric, descriptive and name-only quantities/multipliers preserved | `ShoppingListAggregationTests.Aggregation_IsPureAndKeepsNumericDescriptiveAndNameOnlyAmounts` | Implementation CI passed ([42507447247](https://github.com/Kyziur/WhatToCook/actions/runs/42507447247)); see `evidence.md` |
| Different units and decimal comma parsing preserved | `ShoppingListAggregationTests.Aggregation_DoesNotMergeDifferentUnitsAndParsesCommaDecimal` | Implementation CI passed ([42507447247](https://github.com/Kyziur/WhatToCook/actions/runs/42507447247)); see `evidence.md` |
| Regeneration retains checklist state and manual rows | Existing `ShoppingListManagementSpecificationTests` HTTP integration and Playwright shopping tests | Implementation CI passed ([42507447247](https://github.com/Kyziur/WhatToCook/actions/runs/42507447247)); see `evidence.md` |
| No new external API/deployment contract | Code review: extraction only; existing endpoints unchanged | Author-side source review complete; independent review pending |

# ARC-02 test traceability

| Requirement | Automated check | State |
| --- | --- | --- |
| Numeric, descriptive and name-only quantities/multipliers preserved | `ShoppingListAggregationTests.Aggregation_IsPureAndKeepsNumericDescriptiveAndNameOnlyAmounts` | CI pending |
| Different units and decimal comma parsing preserved | `ShoppingListAggregationTests.Aggregation_DoesNotMergeDifferentUnitsAndParsesCommaDecimal` | CI pending |
| Regeneration retains checklist state and manual rows | Existing `ShoppingListManagementSpecificationTests` HTTP integration and Playwright shopping tests | CI pending |
| No new external API/deployment contract | Code review: extraction only; existing endpoints unchanged | Review pending |

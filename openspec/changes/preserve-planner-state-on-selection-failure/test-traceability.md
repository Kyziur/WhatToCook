# Test traceability

| Scenario | Verification |
| --- | --- |
| Failed plan selection keeps the previous draft, dirty banner and selected ID | MealPlanningSpecificationTests.FailedPlanSwitch_ShouldPreserveCurrentDraftAndSelection |
| Successful plan selection and save retain current behavior | Existing MealPlanningSpecificationTests |
| Full UI, API and deployment invariants | Existing Playwright and ARM64 CI |

No API or schema change.

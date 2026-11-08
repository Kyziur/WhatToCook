# ARC-05 traceability

| Scenario | Verification | State |
| --- | --- | --- |
| Web client create/list/details against API JSON | `MealPlanWebApiContractTests.WebClient_CanRoundTripPlanAndRecognizeRegenerationConflict` | PR CI pending |
| Regeneration conflict code mapped by actual Web client | Same integration test, `RegenerationRequired` assertion | PR CI pending |
| Confirmed regeneration round-trip through Web DTOs | Same integration test, saved/read-back assertions | PR CI pending |
| No production DTO centralization | Project diff review; only test project references Web | Independent review pending |

# ARC-03 partial — traceability

| Requirement | Verification | Status |
| --- | --- | --- |
| Historical archive entry is retained only with same ID, recipe and date | `MealPlanMutationRulesTests` (DB-free) | CI pending |
| Existing archived recipe remains editable in its original slot | `MealPlanningSpecificationTests.InactiveRecipes_ShouldRemainInExistingPlans_AndBeExcludedFromNewAssignments` | CI pending |
| Reused row GUID cannot move archived recipe or swap in archived recipe | `MealPlanningSpecificationTests.ArchivedRecipe_CannotBeReassignedByReusingAPlanEntryId` | CI pending |
| Invalid updates leave persisted plan unchanged | Same Testcontainers HTTP theory | CI pending |
| Lock and shopping regeneration unchanged | Scoped source diff + full regression CI | Review pending |

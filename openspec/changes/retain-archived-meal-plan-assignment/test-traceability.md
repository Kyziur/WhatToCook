# ARC-03 partial — traceability

| Requirement | Verification | Status |
| --- | --- | --- |
| Historical archive entry is retained only with same ID, recipe and date | `MealPlanMutationRulesTests` (DB-free) | [CI 42528747845](https://github.com/Kyziur/WhatToCook/actions/runs/42528747845) passed |
| Existing archived recipe remains editable in its original slot | `MealPlanningSpecificationTests.InactiveRecipes_ShouldRemainInExistingPlans_AndBeExcludedFromNewAssignments` | [CI 42528747845](https://github.com/Kyziur/WhatToCook/actions/runs/42528747845) passed |
| Reused row GUID cannot move archived recipe or swap in archived recipe | `MealPlanningSpecificationTests.ArchivedRecipe_CannotBeReassignedByReusingAPlanEntryId` | [CI 42528747845](https://github.com/Kyziur/WhatToCook/actions/runs/42528747845) passed |
| Invalid updates leave persisted plan unchanged | Same Testcontainers HTTP theory | [CI 42528747845](https://github.com/Kyziur/WhatToCook/actions/runs/42528747845) passed |
| Lock and shopping regeneration unchanged | Scoped source diff + full regression CI | Author-side diff review + final-head CI passed; independent human review not recorded |

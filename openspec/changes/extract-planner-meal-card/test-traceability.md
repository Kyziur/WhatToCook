# Traceability

| Requirement | Test evidence |
| --- | --- |
| Compact and expanded meal cards retain layout hooks and multiplier text | PlannerMealCardSpecificationTests |
| Meal action events are forwarded without the component mutating plan state | PlannerMealCardSpecificationTests |
| Planner persists, regenerates and renders desktop/mobile cards | MealPlanningSpecificationTests + Playwright planner flows |

No HTTP contract, database, or deployment changes.

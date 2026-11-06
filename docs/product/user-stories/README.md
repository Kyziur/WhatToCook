# WhatToCook — Product User Stories

These documents describe **proposed product capabilities**, not functionality that is already delivered. They are written for product refinement and implementation planning. They are deliberately independent from technical designs, infrastructure details, and implementation estimates.

## Product goal

Help a household answer three recurring questions with minimal effort:

1. **What should we cook?**
2. **What do we need to buy?**
3. **How do we add recipes we discover?**

The intended primary user is a household member using WhatToCook on a phone or desktop. Multiple household members may use the same household data. Current production deployment is self-hosted and LAN-oriented; remote and offline behavior requires an explicit product and security decision.

## Proposed epics

| Epic | Document | Outcome | Suggested order |
| --- | --- | --- | --- |
| Shopping List 2.0 | [01-shopping-list-2.0.md](01-shopping-list-2.0.md) | A grocery checklist usable by multiple household members while shopping | First |
| Plan a Week in Two Minutes | [02-fast-weekly-planning.md](02-fast-weekly-planning.md) | Reuse earlier plans rather than recreate them manually | Second |
| Universal Recipe Import | [03-universal-recipe-import.md](03-universal-recipe-import.md) | Save recipes from more websites and from phone sharing | Third |

The proposed order is a hypothesis, not a commitment; validate it with actual household usage.

## How to read and implement these stories

- Each document contains a user problem, measurable outcome, **MVP scope**, user stories with numbered acceptance criteria, edge cases, dependencies, exclusions, and later opportunities.
- **MVP** means a coherent usable release, not a promise to implement every story at once. Delivery slices may be smaller when their end-to-end behavior is meaningful.
- Each story must produce demonstrable behavior and automated tests for its acceptance criteria; assertions about cross-device or offline behavior must be validated on real devices or realistic network conditions as appropriate.
- A proposed feature is **not** marked as implemented by adding this document.
- When implementation is approved, create or update the relevant OpenSpec change and test-traceability matrix; reconcile its requirements with existing canonical specifications rather than silently overriding them.
- Do not equate the existing state `MAM` (“already have at home”) with the new state `BOUGHT` (“purchased during this shopping trip”).
- Explicitly design synchronization, authorization, conflict resolution, and connectivity before claiming multi-device or offline support.

## Related current contracts

- [Shopping list specification](../../../openspec/specs/shopping-list-management/spec.md)
- [Meal planning specification](../../../openspec/specs/meal-planning/spec.md)
- [Recipe management specification](../../../openspec/specs/recipe-management-core/spec.md)
- [Recipe import proposal](../../../openspec/changes/add-recipe-import/proposal.md)
- [Existing import specification](../../../openspec/changes/add-recipe-import/specs/recipe-imports/spec.md)

## Decisions needed before implementation

1. **Access and identity:** Does the product remain one trusted household on LAN, or support authenticated household members over a secure remote endpoint?
2. **Offline behavior:** Is temporary connectivity loss in a supermarket an MVP requirement, or a later release? Offline changes require explicit client-side persistence and merge rules; Blazor Interactive Server alone is not sufficient.
3. **Plan semantics:** Does “copy last week” duplicate the most recent plan by date or let users select an arbitrary source plan? This proposal supports either source via a picker.
4. **Import delivery:** Is OS-level mobile share integration a release gate, or is paste-from-clipboard an acceptable first delivery slice? Both have stories, but their rollout can differ.

## Success criteria (product hypotheses, not observed metrics)

- A user can copy and adjust a five-dinner weekly plan in **under three minutes**.
- A user can save a supported structured recipe URL in **under one minute**, excluding their own content corrections.
- Two members can update the same shopping list without losing completed purchases or duplicating items.
- A shopper can identify unpurchased items quickly, even after leaving the app and reopening it.

Measure actual outcomes in usability sessions before interpreting these as targets achieved.

> Product-oriented user stories and detailed acceptance criteria for a **design-only** OpenSpec change. The testable capability delta is in [specs/shopping-list-management/spec.md](specs/shopping-list-management/spec.md). These items are proposed, not implemented.

# Epic SL2 — Shopping List 2.0

**Priority:** P0 — first proposed product epic  
**Status:** Proposed / not implemented  
**Users:** Household shopper, household planner, second household member  
**Primary outcome:** One trustworthy shopping checklist that works on a phone and remains consistent across members.

## Problem and job to be done

Today WhatToCook builds a shopping list from a meal plan, aggregates ingredients, supports manually added items, and tracks `MAM` (already available) versus `NIE MAM` (needed). This tells a person **what is needed**, but not **what has been put in the trolley**. A household shopping together needs to see what is left, reduce duplicate purchases, and avoid losing progress.

**Job story:** *When I am walking through a shop with a list generated from our meal plan, I want to see only the products I still need to pick up, mark purchases instantly, and trust that my household sees the same progress, so we finish shopping without checking multiple messages or paper notes.*

## Scope and business definitions

- **Have at home**: `MAM` means this ingredient is already available and need not be purchased.
- **Need to buy**: `NIE MAM` means this item should appear among products to purchase.
- **Bought**: a new, separate purchasing state for the current shopping trip. Bought items must remain distinguishable from items already at home.
- **Manual item**: any free-text product entered by the user, including identical repeated entries when intentionally added.
- **Grocery grouping**: an optional, editable department/category label (for example Produce, Dairy, Bakery, Pantry, Frozen, Other). It is not an inventory count.
- **Shopping session**: a user's active purchasing progress against a shopping list. The precise schema and whether a new session is explicit are implementation decisions; the user-visible semantics are mandatory.

## MVP user stories and acceptance criteria

### SL2-01 — Mark an item as bought

**As a** shopper, **I want** to mark a needed product as bought **so that** I know what remains to pick up.

**Acceptance criteria**

- **AC1:** Given an item is `NIE MAM` and not yet bought, when I tap its primary action, then the item becomes **Bought**, its appearance changes immediately, and it is removed from the **Still to buy** count.
- **AC2:** Given an item is **Bought**, when I undo the action, then it returns to the **Need to buy** view without creating a duplicate item.
- **AC3:** Given an item is `MAM`, then it is visibly represented as **Already at home**, not as **Bought**. The two concepts are never silently merged.
- **AC4:** When I navigate away or reopen the app, the purchase state is preserved in persistent data.
- **AC5:** When a state update fails, the UI explains the failure and does not falsely display it as saved.
- **AC6:** The action is usable with touch and keyboard, with an accessible state announcement and a sufficiently large tap target.

### SL2-02 — Shop from a focused mobile view

**As a** shopper, **I want** a distraction-free view of outstanding products **so that** I can use the list while moving around a shop.

**Acceptance criteria**

- **AC1:** The shopping screen defaults to **Still to buy** items, with an explicit way to show **Bought** and **Already at home** items.
- **AC2:** A visible progress summary shows bought items against all shopping-trip items, excluding `MAM` items from the shopping-trip denominator.
- **AC3:** An empty list, completed trip, loading state, and error state each have a clear explanation and next action.
- **AC4:** On a narrow phone screen, product name, amount, unit, category, and action are legible without horizontal scrolling.
- **AC5:** Users can navigate directly to the active list from the main app navigation; the current list is retained after returning from another screen.

### SL2-03 — Group products by shop department

**As a** shopper, **I want** needed products grouped by area of the store **so that** I avoid walking back and forth.

**Acceptance criteria**

- **AC1:** Each list item can have one category, chosen from a small user-editable list; items without a known category appear under **Other / Uncategorised**.
- **AC2:** The shopping screen groups items by category and preserves a consistent category order.
- **AC3:** I can move an item to a different category without changing its name, quantity, or purchase state.
- **AC4:** The category selection persists for that item and is retained where the same stable ingredient is used in a regenerated list, subject to the explicitly defined category inheritance rule.
- **AC5:** Manually entered items can be categorised too; free-text entries are not required to match an existing ingredient.
- **AC6:** The screen still works if categories are disabled or not configured.

**Product note:** Automatic category suggestions are optional. They must be editable rather than treated as authoritative.

### SL2-04 — Collaborate on the same household list

**As a** household member, **I want** list updates made by another member to appear on my device **so that** we do not buy the same thing twice.

**Acceptance criteria**

- **AC1:** Two devices with access to the **same authorised household** open the same list and can see item additions, purchase changes, and reversals.
- **AC2:** When device A marks an item bought, device B sees the change without manually exporting/importing the list; if a live connection is unavailable, B can refresh when connectivity returns.
- **AC3:** Concurrent changes to **different** items are all preserved.
- **AC4:** Concurrent, conflicting changes to the **same** item resolve according to a documented and tested policy; no update silently destroys unrelated item data.
- **AC5:** Pending, successful, and failed saves are distinguishable in the UI; the app must not claim successful sync while disconnected.
- **AC6:** A household member must not be able to access another household's list if multi-household or identity support is introduced.

**Dependency:** This story requires a deliberate identity/access model and synchronization contract. A LAN deployment does not, by itself, provide safe remote access.

### SL2-05 — Add and manage non-recipe items

**As a** shopper, **I want** to add everyday items not included in recipes **so that** the shopping list covers our full trip.

**Acceptance criteria**

- **AC1:** I can add a manual item with a required, non-blank name; category and amount are optional.
- **AC2:** I can edit or remove a manual item, with safe handling of accidental removal (confirmation or undo).
- **AC3:** Manually added items can be marked **Bought** and return to the correct view after refreshing.
- **AC4:** Two intentionally identical manual entries remain distinct until a user explicitly removes or merges them.
- **AC5:** Updating or regenerating the meal plan does not delete manually added items, their purchasing state, or their category.
- **AC6:** Invalid input and length limits show understandable validation messages.

### SL2-06 — Regenerate without losing shopping progress

**As a** planner, **I want** my shopping progress preserved when I adjust the meal plan **so that** I do not have to tick off everything again.

**Acceptance criteria**

- **AC1:** When the meal plan changes and I choose to regenerate its list, matching items retain **Already at home**, **Bought**, and category state as applicable.
- **AC2:** New items start in **Need to buy** unless the user explicitly sets another state.
- **AC3:** Removed recipe-derived items do not unexpectedly remain as active shopping requirements; manually added items remain.
- **AC4:** Quantity changes retain the stable matching item's purchase state **but** make a changed required quantity visible, so the user can decide whether additional buying is necessary; the app must not silently imply the increased quantity was purchased.
- **AC5:** Regeneration is atomic from the user's perspective: on failure, the previous list and progress remain intact.
- **AC6:** Both the direct regeneration action and plan-save-with-regeneration have consistent behavior.

**Open decision:** For changed quantities on bought items, the MVP may reset the item to **Need to buy** with an explanation, instead of tracking partial purchasing. This choice must be explicit and covered by tests.

## Connectivity acceptance gate

### SL2-07 — Maintain a useful shopping experience after temporary disconnection

**As a** shopper, **I want** to keep using a previously opened list when mobile connectivity drops **so that** I am not blocked in the store.

This is a **separately approved MVP gate** if real supermarket use is required at launch. It is not safely provided by the existing Blazor Interactive Server setup.

**Acceptance criteria**

- **AC1:** A previously loaded list remains readable after a network drop; the UI clearly identifies that it is offline.
- **AC2:** When offline edits are supported in the selected release, I can mark items bought and those edits are queued on the device with a visible **Pending sync** state.
- **AC3:** Reconnecting retries queued mutations without creating duplicate manual items or losing unrelated changes.
- **AC4:** If the server version conflicts with offline changes, the app applies a documented merge rule and exposes any unresolved conflict; silent overwrite is not acceptable.
- **AC5:** The app never suggests that a new or unvisited list is available offline unless it was stored locally.
- **AC6:** Browser refresh, app suspension, and temporary network failures are tested under realistic mobile conditions.

**Release constraint:** If offline operation is not implemented, label this as a connected-only release and do not market it as reliable offline shopping.

## Out of scope for first release

- A full pantry with stock counts, expiry dates, or barcode recognition.
- Product prices, store-specific offers, and route optimization.
- Automatic purchase receipt matching.
- Multiple shopping trips per plan unless validated as necessary.
- Automatic remote access exposed on the public internet without authentication.

## Quality, test scenarios, and non-functional requirements

- Integration tests cover **Bought vs MAM** distinction, manual duplicates, regeneration, conflicts, and persistence across sessions.
- End-to-end tests cover mobile grouping, progress, undo, error and empty states.
- Two-device tests verify cross-device consistency; offline testing is mandatory **only if SL2-07 is approved**.
- Purchase and undo actions should feel immediate; measure p95 update time with real devices before setting a release threshold.
- The system should remain readable and accessible even with hundreds of list entries.
- Avoid logging sensitive user-entered manual product text by default.

## Product success measures

- The percentage of generated lists actually opened during a shopping trip.
- The percentage of opened trips with at least one bought item and a completed list.
- Reported duplicate purchases or lost checklist updates in household usability trials.
- Time to find the next needed item when standing in a store.

## Dependencies and implementation handoff

- Existing [shopping-list-management](../../../openspec/specs/shopping-list-management/spec.md) requirements and aggregation semantics.
- Need an explicit purchase-state model, persistence and migration plan, sync contract, category semantics, and UI flow.
- Resolve access to the app outside the LAN before promising an in-store shared experience.
- Create OpenSpec change and test traceability on implementation approval; do not replace current `MAM`/`NIE MAM` semantics.

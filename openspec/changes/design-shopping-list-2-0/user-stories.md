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
- **Bought**: a purchase indicator on an item still marked `NIE MAM`. It is not yet available at home. On **Finish shopping**, all bought items atomically become `MAM` (clearing Bought), so the recommendation service considers them owned.
- **Manual item**: any free-text product entered by the user, including identical repeated entries when intentionally added.
- **Grocery grouping**: an optional, editable department/category label (for example Produce, Dairy, Bakery, Pantry, Frozen, Other). It is not an inventory count.
- **Shopping trip**: actions against the currently generated plan-linked checklist. No separate trip entity, Start button, or history in MVP; **Finish shopping** converts Bought items to `MAM` and leaves unmet needs outstanding.

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
- **AC7:** Finish shopping atomically converts Bought items to `MAM`, while unpurchased `NIE MAM` items remain outstanding. Recommendations then recognize the newly owned ingredients.

### SL2-02 — Shop from a focused mobile view

**As a** shopper, **I want** a distraction-free view of outstanding products **so that** I can use the list while moving around a shop.

**Acceptance criteria**

- **AC1:** The shopping screen defaults to **Still to buy** items, with an explicit way to show **Bought** and **Already at home** items.
- **AC2:** A visible progress summary shows bought items divided by Bought plus outstanding `NIE MAM` items, excluding `MAM`. After finishing, the counter reflects unmet needs, not a historical trip percentage.
- **AC3:** An empty list, completed trip, loading state, and error state each have a clear explanation and next action.
- **AC4:** On a narrow phone screen, product name, amount, unit, category, and action are legible without horizontal scrolling.
- **AC5:** Users can navigate directly to the active list from the main app navigation; the current list is retained after returning from another screen.

### SL2-03 — Group products by shop department

**As a** shopper, **I want** needed products grouped by area of the store **so that** I avoid walking back and forth.

**Acceptance criteria**

- **AC1:** Each item uses the fixed categories Produce, Dairy, Bakery, Meat & Fish, Pantry, Frozen, Household or Other; uncategorized items appear in **Other**. Creating, renaming, deleting and reordering categories is outside MVP.
- **AC2:** The shopping screen groups items by category and preserves a consistent category order.
- **AC3:** I can move an item to a different category without changing its name, quantity, or purchase state.
- **AC4:** An item's category survives refresh and regeneration when a generated item matches the same normalized ingredient and unit, including when quantity changes.
- **AC5:** Manually entered items can be categorised too; free-text entries are not required to match an existing ingredient.
- **AC6:** An ungrouped view displays all items regardless of category.

**Product note:** Automatic category suggestions are optional. They must be editable rather than treated as authoritative.

### SL2-04 — Collaborate on the same household list

**As a** household member, **I want** list updates made by another member to appear on my device **so that** we do not buy the same thing twice.

**Acceptance criteria**

- **AC1:** Two connected devices on the **same trusted single-household installation** display the shared list and can see additions, purchases and reversals.
- **AC2:** After device A's write succeeds, connected device B sees the saved change automatically without refresh (target within five seconds on a healthy connection); disconnected devices display that they are out of sync.
- **AC3:** Concurrent changes to **different** items are all preserved.
- **AC4:** Same-item concurrent edits use optimistic item versioning. A stale write is rejected and the current state displayed, allowing deliberate retry; no unrelated data is overwritten.
- **AC5:** Pending, successful, stale-conflict, and disconnected states are distinct. This connected-only client never claims that offline mutations were queued.
- **AC6:** Only devices granted access to the trusted LAN or authenticated private tunnel can reach the list. No multi-household tenancy, app user accounts, or public unauthenticated endpoints are included in MVP.

**Security boundary:** one trusted household installation. Shopping outside LAN requires a separately configured authenticated private tunnel/VPN; the current unauthenticated API must never be opened publicly.

### SL2-05 — Add and manage non-recipe items

**As a** shopper, **I want** to add everyday items not included in recipes **so that** the shopping list covers our full trip.

**Acceptance criteria**

- **AC1:** On an **already generated plan-linked** shopping list, I can add a manual item with a required, nonblank name; amount and fixed-list category are optional. Standalone manual-only lists are outside MVP.
- **AC2:** I can edit or remove a manual item, with safe handling of accidental removal (confirmation or undo).
- **AC3:** Manually added items can be marked **Bought** and return to the correct view after refreshing.
- **AC4:** Two intentionally identical manual entries remain distinct until a user explicitly removes or merges them.
- **AC5:** Updating or regenerating the meal plan does not delete manually added items, their purchasing state, or their category.
- **AC6:** Invalid input and length limits show understandable validation messages.

### SL2-06 — Regenerate without losing shopping progress

**As a** planner, **I want** my shopping progress preserved when I adjust the meal plan **so that** I do not have to tick off everything again.

**Acceptance criteria**

- **AC1:** When a generated item has unchanged normalized ingredient, unit and amount, it retains its home/purchase state and category after regeneration.
- **AC2:** New items start in **Need to buy** unless the user explicitly sets another state.
- **AC3:** Removed recipe-derived items do not unexpectedly remain as active shopping requirements; manually added items remain.
- **AC4:** If a matched generated item's normalized amount changes in any way, both `MAM` and Bought are reset to `NIE MAM`; its category remains and the user sees an explanation.
- **AC5:** Regeneration is atomic from the user's perspective: on failure, the previous list and progress remain intact.
- **AC6:** Both the direct regeneration action and plan-save-with-regeneration have consistent behavior.

**MVP decision:** conservative amount changes always reset the confirmation to `NIE MAM`; partial purchase tracking is deferred.

## Deferred future candidate (not part of this OpenSpec delta)

This story is retained solely for later prioritization. It is **not a current SHALL requirement**, is not in the delta spec and must be delivered in a separate future OpenSpec change.

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

- Integration tests cover Bought-to-`MAM` checkout, downstream recommendations, manual duplicates, changed-quantity resets, regeneration and stale-item conflict handling.
- End-to-end tests cover mobile grouping, progress, undo, error and empty states.
- Two-device tests verify automatic connected propagation and rejected stale writes; offline tests belong to a separate future OpenSpec change.
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
- Remote shopping requires an authenticated private network/tunnel; the unauthenticated app/API must not be publicly exposed.
- Create OpenSpec change and test traceability on implementation approval; do not replace current `MAM`/`NIE MAM` semantics.

## Context

The current shopping list belongs to a meal plan and is available only after explicit generation. Items carry `MAM` / `NIE MAM`; recipe recommendations count only `MAM`. The client uses Blazor Interactive Server, so offline persistence is not available out of the box.

## Goals / Non-Goals

**Goals:** phone-first connected shopping; distinct purchase and household-stock state; fixed store departments; two-device automatic synchronization; quantity-safe regeneration.

**Non-Goals:** no code in this design change; no offline list, standalone manual-only list, accounts or multi-household tenancy, store pricing, stock counts, trip history or customizable category taxonomy.

## Decisions

### One current trip against a generated list

No separate shopping-session record or Start action. Marking a `NIE MAM` item as Bought creates persisted progress; undo clears Bought. `Finish shopping` atomically converts all Bought items to `MAM` and leaves other `NIE MAM` needs outstanding. Further buying may continue for remaining items. Past trip completion percentages are not retained. Recipe recommendations continue to consider only `MAM`, so bought items count as owned only after Finish shopping.

### Bought is an orthogonal purchase indicator

Do not redefine the existing `ChecklistState` values or historical rows. Bought applies only when `NieMam`; setting `Mam` clears Bought. Implementation selects the precise persisted representation, keeping this transition table invariant.

### Regeneration is conservative and deterministic

Match only generated items by normalized ingredient and normalized unit; preserve identity and item-level category. If the normalized amount/descriptor is unchanged, retain availability/purchase state. If the amount increases, decreases or changes text, reset to `NIE MAM` and clear Bought, with a visible explanation. An altered unit is a different item and cannot inherit buying state. Manual duplicates retain separate IDs and all their state. Both direct regeneration and plan-save-with-regeneration remain atomic.

### Fixed MVP categories

Produce, Dairy, Bakery, Meat & Fish, Pantry, Frozen, Household, Other. Item-level reassignment and ungrouped view only; no custom labels, global ingredient taxonomy or category CRUD in MVP. Legacy/unclassified entries appear under Other.

### Trusted connected-only single-household deployment

No new app accounts, `Household` table or multi-tenant roles in MVP. The single household shares the deployment through a trusted LAN or an authenticated private tunnel/VPN; the unauthenticated app/API must never be exposed publicly. Two connected devices receive updates automatically (target <= 5 seconds on healthy connectivity). Server-side item versions detect stale writes and require explicit user retry; different-item updates coexist. A disconnected client reports lack of sync and does not pretend offline changes are queued.

### Offline remains a separate future change

SL2-07 is preserved as a **non-normative future story** only. To provide offline reading and writes on a phone, a separate OpenSpec change must define local persistence, mutation queue and replay/conflict resolution, rather than relying on Blazor Interactive Server.

## Trade-offs and risks

- Conservative quantity reset can require repeating a manual decision, but prevents false `MAM` and Bought carryover.
- LAN/private tunnel access is an explicit deployment boundary; remote connection setup and user account management are not silently assumed.
- New item purchase/version fields require data migration and integration tests.
- Any stale list cannot claim success or automatic offline reconciliation.

## Future delivery sequence

1. Confirm state transitions, `Finish shopping` and recommendation behavior.
2. Add backwards-compatible purchase/version storage and quantity-aware regeneration, with PostgreSQL tests.
3. Add the mobile shopping view, fixed category selection and manual-entry editing/deletion.
4. Implement automatic connected-device updates, stale-write feedback and private-network deployment checks.
5. Complete E2E and concurrency tests and sync canonical specifications before archive.

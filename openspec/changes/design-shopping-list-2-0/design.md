## Context

WhatToCook already aggregates recipe ingredients into shopping lists and stores `MAM` and `NIE MAM` state. Lists also contain manually entered items and are regenerated when meal plans change. The primary client is Blazor Interactive Server, deployed in a self-hosted household LAN environment.

## Goals / Non-Goals

**Goals**
- Separate **already owned** from **purchased during this trip** without breaking existing state meaning.
- Maintain item identity, manual duplicates, categories, and progress across regeneration.
- Support more than one device with explicit authorization, consistency, and user-visible sync status.
- Specify an offline release gate separately from connected-list functionality.

**Non-Goals**
- No implementation now, and no requirement to introduce a general inventory system.
- No assumption that server-side Blazor circuits or browser memory provide offline durability.

## Decisions for this proposal

### 1. Introduce purchasing state orthogonally to home availability
A new **Bought** outcome is necessary; simply renaming `MAM` would conflate "we already had it" and "we purchased it". Future schema may use an orthogonal purchase-status field rather than expanding one enum. Choose and verify during implementation design.

### 2. Keep stable list-item identity across refresh and regeneration
Matching must take normalization, unit and provenance into account; intentionally duplicated manual items are not merged. Changed quantities on previously bought entries must be surfaced and either reset to outstanding or otherwise explicitly reconciled.

### 3. Define sync and access boundaries before multi-device release
Use versioned or conditional updates on server-side item state; distinguish independent updates from conflicting writes. Do not rely solely on a shared LAN for confidentiality once remote access is introduced. Authorization requirements should be aligned with a planned household identity solution.

### 4. Avoid coupling shopping-category data to recipe source text
Categories may be explicitly assigned by a member and optionally suggested from normalized ingredients. Manual entries can have their own category. Missing classifications must not block shopping.

### 5. Treat offline as a separate architectural decision
Connected multi-device updates can use existing server APIs and realtime refresh. Offline read/write with durable conflict-safe sync needs browser-side persistent storage and an explicitly designed communication path (for example a PWA client or isolated offline-capable view); it is not covered by the current Blazor Interactive Server circuit.

## Risks / Trade-offs

- **False progress** from mixing `MAM` with `Bought`: mitigate with separate states and migration tests.
- **Silent loss during regeneration**: mitigate with transactional reconciliation and identity/provenance-aware mapping.
- **Conflicting devices**: mitigate with documented per-item conflict policy and integration tests.
- **Security of remote lists**: access and identity must be designed before external availability.
- **Offline scope creep**: treat SL2-07 as an explicit release gate and avoid claiming more than is tested.

## Migration and release approach (future)

1. Agree on state model, category inheritance, and changed-quantity rules.
2. Add backwards-compatible persistence and API contracts with Testcontainers coverage.
3. Deliver a focused connected-only shopping view and one-device purchase flow.
4. Introduce authorized two-device reconciliation and failure feedback.
5. Decide separately on offline delivery; implement browser-side durability and conflict handling only if approved.
6. Test release behavior on real phones and across network transitions before updating canonical specs.

## Open Questions

- Will shopping list access require household logins, a trusted VPN, or another explicit access mechanism?
- If a bought item is regenerated with a larger quantity, should it revert to outstanding automatically or retain partial progress?
- Is robust offline read/write required for the first shopping release or explicitly deferred?
- Are store departments household-global, per-list, or per-user preferences?

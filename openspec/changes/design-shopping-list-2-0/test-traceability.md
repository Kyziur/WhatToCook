# Shopping List 2.0 — Test Traceability

This is a **design-only** change. `PENDING` means tests must verify the proposed delta before delivery; the four pre-existing scenarios are repeated because OpenSpec MODIFIED requirements must restate the full canonical requirement. Their baseline behavior exists, but new quantity-aware conditions require regression verification (including changes to `Regeneration_ShouldPreserveMatchingStateAndManualDuplicates`). Deferred offline SL2-07 is outside this contract.

| Capability scenario | Automated coverage | Status | Owning task |
| --- | --- | --- | --- |
| SL2-01 — Mark a required item bought | Not implemented | PENDING | 2.1, 3.1 |
| SL2-01 — Undo buying an item | Not implemented | PENDING | 2.1, 3.1 |
| SL2-01 — Distinguish stocked and purchased items | Not implemented | PENDING | 2.1, 3.1 |
| SL2-01 — Finish shopping and mark purchases as owned | Not implemented | PENDING | 2.1, 3.1 |
| SL2-01 — Make completed purchases available to recommendations | Not implemented | PENDING | 2.1, 3.1 |
| SL2-01 — Recover from an unsuccessful update | Not implemented | PENDING | 2.1, 3.1 |
| SL2-01 — Accessible purchase control | Not implemented | PENDING | 2.1, 3.1 |
| SL2-02 — Show outstanding items first | Not implemented | PENDING | 2.2, 3.2 |
| SL2-02 — Calculate trip progress | Not implemented | PENDING | 2.2, 3.2 |
| SL2-02 — Make the list usable on a phone | Not implemented | PENDING | 2.2, 3.2 |
| SL2-02 — Explain empty and failed states | Not implemented | PENDING | 2.2, 3.2 |
| SL2-03 — Group items by department | Not implemented | PENDING | 2.3, 3.1 |
| SL2-03 — Reassign a category | Not implemented | PENDING | 2.3, 3.1 |
| SL2-03 — Keep category choices through list regeneration | Not implemented | PENDING | 2.3, 3.1 |
| SL2-03 — Show an ungrouped list | Not implemented | PENDING | 2.3, 3.1 |
| SL2-04 — Automatically synchronize connected devices | Not implemented | PENDING | 2.4, 3.3 |
| SL2-04 — Preserve updates to different items | Not implemented | PENDING | 2.4, 3.3 |
| SL2-04 — Reject stale changes to the same item | Not implemented | PENDING | 2.4, 3.3 |
| SL2-04 — Report disconnected or rejected synchronization | Not implemented | PENDING | 2.4, 3.3 |
| SL2-04 — Enforce trusted network access | Not implemented | PENDING | 2.4, 3.3 |
| SL2-05 — Create and validate a manual item | Not implemented | PENDING | 2.3, 3.1 |
| SL2-05 — Edit or remove a manual item | Not implemented | PENDING | 2.3, 3.1 |
| SL2-05 — Preserve duplicate manual entries | Not implemented | PENDING | 2.3, 3.1 |
| SL2-05 — Retain manually entered products after regeneration | Not implemented | PENDING | 2.3, 3.1 |
| Attempt to save changed plan with existing shopping list | Existing baseline; verify/update tests against revised contract | PENDING | 2.5, 3.1 |
| Cancel shopping list regeneration | Existing baseline; verify/update tests against revised contract | PENDING | 2.5, 3.1 |
| Confirm shopping list regeneration | Existing baseline; verify/update tests against revised contract | PENDING | 2.5, 3.1 |
| Regenerate a shopping list directly | Existing baseline; verify/update tests against revised contract | PENDING | 2.5, 3.1 |
| SL2-06 — Preserve state for unchanged ingredient and quantity | Not implemented | PENDING | 2.5, 3.1 |
| SL2-06 — Reset home/purchase state when amount changes | Not implemented | PENDING | 2.5, 3.1 |
| SL2-06 — Add and remove generated needs | Not implemented | PENDING | 2.5, 3.1 |
| SL2-06 — Do not carry buying state across different units | Not implemented | PENDING | 2.5, 3.1 |
| SL2-06 — Regeneration is atomic in both update paths | Not implemented | PENDING | 2.5, 3.1 |

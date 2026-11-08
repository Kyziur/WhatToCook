# AGENT-06 — Guard OpenSpec harness adapter invariants

## Why
OpenSpec `apply` and `archive` are surfaced through both OMP commands and `.agents` skills. Their wrappers differ intentionally, but shared critical safety contracts currently occur in both. A future edit to only one wrapper could silently drop a safety rule. Do not introduce a generator or edit OpenSpec-generated content solely to solve this.

## What changes
- Add a targeted Node contract test of **shared high-risk invariants** in both apply and archive entry points: CLI state, completion evidence, in-flight sync protection, sync validation and archive-path safety.
- Run the test in the existing agent configuration CI step, without a separate job.
- Keep harness-specific invocation and metadata untouched; the test does not require byte-for-byte equivalence.

## Non-goals
No generator, auto-edit of generated skills, no restructuring, no behavioral/runtime change, no requirement that non-critical wording be identical.

## Limitations
This catches deletion or material rewriting of explicitly selected phrases. It does not prove all agent prompt semantics are equivalent. On an intentional wording change, review both adapters and update the test to match the preserved contract.

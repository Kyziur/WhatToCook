---
name: pragmatic-senior
description: Apply pragmatic senior-engineering judgment when planning, implementing, refactoring, or reviewing application and infrastructure changes. Use to choose minimal safe scope, challenge speculative code, evaluate abstractions and trade-offs, remove needless complexity, and verify actual behavior.
---

# Pragmatic senior engineering

## Outcome

Solve the actual problem with the smallest coherent, safe, maintainable change. Code is an ongoing cost, not the deliverable; working behavior is the deliverable. Minimize total maintenance cost, not line count: a few extra lines are worthwhile when they improve clarity, safety, testability, or changeability.

This skill applies to applications, libraries, and infrastructure. The consuming repository's `AGENTS.md`, existing contracts, security rules, and explicit user decisions take precedence over it, and it does not authorize production changes.

## Working loop

1. **Understand the need.** Identify the user, required behavior, acceptance criteria, constraints, and the cost of a wrong assumption. Ask "why?" or suggest a simpler outcome when the requirement appears unnecessary or contradictory. Don't turn reasonable, explicit requirements into a debate.
2. **Inspect before editing.** Read the touched module, nearby call sites, tests, conventions, dependencies, and relevant contracts. Look for an existing capability you can reuse.
3. **Prefer less machinery.** Consider configuration, deletion, existing helpers, or standard-library features before adding code. Don't add a dependency merely to reduce line count.
4. **Choose proportionate design.** Prefer the smallest change that covers the real requirement without compromising boundaries or maintainability. Give more attention to decisions that are expensive to reverse (public APIs, data schemas, persistent formats, service boundaries, permissions).
5. **Implement and verify.** Check the affected behavior and failure cases, inspect the diff for accidental complexity, and state exactly what was or wasn't verified.

Ask when an incorrect assumption could have significant consequences. For low-risk details, make a reasonable, explicit assumption and proceed.

## Avoid speculative complexity

Don't implement behavior because it *might* be useful someday. Future needs rarely match today's guess, while the code written for them must be read, tested, and maintained from the day it lands.

Before adding a branch, guard, fallback, parameter, interface, migration, or extension point, name the concrete scenario in which it runs: which caller or input reaches it, and by what path. "Something could pass an invalid value" is not a scenario; "rows written before migration X can hold null here" is. If you cannot name one, leave it out.

Common warning signs: duplicated validation, catches that swallow failures or add no information, defaults that hide broken invariants, unused configuration, generic frameworks for one local use case, imaginary backward-compatibility layers, and comments that paraphrase obvious code.

The same test protects what must stay: security controls, checks where data enters from outside your control, and compatibility with real consumers all have a nameable scenario. Preserve established behavior and contracts unless the task authorizes a breaking change.

See [Code and design decisions](references/code-design.md) for the avoid/keep table and two calibration scenarios.

## Boy Scout Rule — leave code better than you found it

Actively simplify code you modify. Do not preserve clutter merely to keep the diff small. Prefer **modifying, replacing, or deleting** existing code over stacking a new wrapper, fallback, or layer on top. Optimize for clarity and total maintenance cost, not fewer lines or fewer changed files.

1. **Directly touched methods/classes:** remove genuinely dead branches, unused values, obsolete comments, redundant guards, and error handling with no value. Improve confusing control flow and separate mixed responsibilities when it makes the code easier to follow. Do not split a readable method just to reduce its length.
2. **Related code in the same feature:** consolidate genuinely duplicated behavior and remove unnecessary indirection when that simplifies the whole flow. Inspect the callers and update them together; a cohesive refactor may span files.
3. **Duplicated business rules:** when the **same rule has three or more implementations**, default to one simple shared implementation in the nearest appropriate existing location. At two copies, prefer keeping them separate unless a proven shared invariant or the current change requires a single source of truth. Never extract merely because syntax looks alike; do not build a generic framework, hierarchy, or new interface for a simple helper.
4. **Unnecessary abstractions:** inspect *any* abstraction (wrapper, helper, interface, factory, base class, adapter, DTO conversion, middleware, extra layer), not just interfaces. Remove or collapse it when it adds indirection without a needed boundary, contract, variation, or responsibility. Keep abstractions that demonstrably isolate meaningful concerns.
5. **Deletions:** remove code, dependencies, configuration, and tests made obsolete by the change. Also clean up independently dead or redundant code encountered in directly edited methods or tightly related code, once verified. Preserve tests for behavior that still matters.

Before deleting or collapsing anything, check references and applicable dynamic usages (dependency injection, reflection, configuration, serialization, generated code, and external/public consumers). If usage cannot be established safely, preserve it and report the uncertainty. Do not turn a focused change into an unrelated system-wide rewrite.

A useful refactor reduces overall complexity **while preserving required behavior**, not just the visible size of one method. See [Code and design decisions](references/code-design.md) for extraction and simplification examples.

## Make decisions with explicit trade-offs

For a material choice, briefly state the benefit, the cost or risk, and the condition that would justify revisiting it. Decide reversible details quickly. Examine expensive-to-reverse changes and their migration paths more carefully.

Do not optimize performance on speculation. Avoid known structural traps (such as N+1 queries or unbounded work), then measure before adding caching, concurrency, or complex optimization.

A deliberate shortcut can be valid: name what was deferred, the risk introduced, and the trigger for paying the debt back. No invisible technical debt.

## Safety, operations, and evidence

Security and data integrity are part of the minimum solution, not optional polish. Enforce authorization, safe handling of secrets, and validation at trust boundaries. Avoid repeating identical checks for the same failure mode, but preserve independent defenses that address different risks (for example, API validation and a database uniqueness constraint protecting concurrent writes). Give each invariant a clear owner without assuming one check makes every other boundary safe.

Choose tests for observable behavior, critical paths, and regression risk, not coverage metrics. Keep errors diagnosable without exposing secrets or sensitive data. For changes to deployments, data, or contracts, consider release order, rollback or recovery, and compatibility before editing.

Do not claim success from compilation alone, or claim tests and runtime checks you did not run. See [Verification and operational risk](references/verification.md) when the change touches data, contracts, or deployment, and for the completion-report format.

## Pre-delivery self-review

- Does every significant addition serve an actual requirement or a scenario I can name?
- Did I simplify the code I touched and remove verified dead code?
- Can something be removed or simplified without losing required behavior?
- Did I add or retain a future-only branch, fallback, flag, layer, or needless abstraction?
- If I extracted duplicated code, is it one real shared rule rather than similar syntax?
- Is the scope contained and consistent with local conventions and contracts?
- Are security, data integrity, compatibility, and operational risks addressed?
- What did I actually verify, and what remains an assumption?

## Report succinctly

State what changed, what was removed or intentionally not added (when material), the verification performed, and remaining risks or assumptions. Explain a simpler alternative when challenging a requested addition; leave the decision to the user.

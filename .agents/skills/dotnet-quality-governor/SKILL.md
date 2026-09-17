---
name: dotnet-quality-governor
description: Govern code quality during active C#/.NET development. Use whenever Codex creates, changes, refactors, or reviews production or test code in .NET, including ASP.NET Core, Blazor, MAUI, domain logic, persistence, and shared libraries. Keep changes correct, locally understandable, cohesive, loosely coupled, testable, secure, observable, and proportionate without metric gaming or speculative abstraction.
---

# .NET Quality Governor

Apply these guardrails while designing and implementing the change, not as cleanup after it.

## Work in this order

1. Establish the required behavior, invariants, failure semantics, constraints, and affected boundaries.
2. Inspect nearby conventions and dependency direction before introducing a new pattern.
3. Choose the simplest explicit design that keeps the change local and preserves correctness.
4. Implement in small cohesive units, validating the narrowest meaningful behavior as work proceeds.
5. Before handoff, review the diff against the quality questions below and run repository-required verification.
6. Improve quality within the changed scope. Record unrelated debt rather than expanding a feature or fix into a broad cleanup.

## Govern the design

- Keep one cohesive reason to change per method, type, and module. Allow an orchestration method to coordinate several named steps when they form one use case.
- Keep policy/domain decisions separate from transport, UI, persistence, and other I/O. Make dependencies explicit and respect the intended dependency direction.
- Prefer local reasoning: minimize hidden state, ambient context, deep delegation, side effects, and knowledge spread across unrelated files.
- Default to `private` or `internal`; expose only the smallest contract consumers need. Do not introduce project-reference cycles.
- Prefer clear, direct code. Add an abstraction only for a real boundary, variation, dependency, or repeated concept, not for a hypothetical future.
- Remove duplicated knowledge. At a third occurrence, evaluate extraction; do not merge coincidentally similar code whose reasons to change differ.

## Control complexity

- Target cognitive complexity at or below 8 and cyclomatic complexity at or below 8 per method.
- At cognitive complexity 9-14 or cyclomatic complexity 9-10, reassess boundaries and simplify when it improves local understanding.
- Use the configured analyzer as the source of truth. Without one, treat complexity thresholds as heuristics and never claim an exact score.
- At cognitive complexity 15 or cyclomatic complexity above 10, simplify or record why no proportionate reduction would improve quality.
- Do not worsen an existing violation. Reduce localized legacy debt when safe; otherwise record it without expanding scope.
- Treat more than 3 nesting levels, about 30 lines, more than 5 parameters, or a long chain of trivial delegations as review signals, not automatic failures.
- Never split code or add indirection solely to improve a metric. Extract a named concept, rule, responsibility, or boundary.

Read [design-metrics.md](references/design-metrics.md) when method, type, module, dependency, or duplication boundaries are material to the change.

## Preserve operational quality

- Make errors, cancellation, timeouts, retries, concurrency, transactions, and resource ownership explicit where relevant.
- Validate at trust boundaries, authorize every protected operation, protect secrets and personal data, and use secure defaults.
- Optimize measured hot paths and prevent obvious unbounded work; do not trade clarity for speculative micro-optimization.
- Add structured, contextual observability at system boundaries and meaningful state transitions. Do not log noise or sensitive data.
- Document non-obvious algorithms, constraints, trade-offs, and reasons; do not narrate self-explanatory code.

Read [quality-model.md](references/quality-model.md) for substantial production changes or when quality concerns conflict.

## Prove the change

- Test behavior at the lowest stable boundary that provides sufficient confidence. Avoid tests coupled to private structure or incidental call order.
- Use unit tests for isolated rules, integration tests for real infrastructure and contracts, component tests for UI behavior, and E2E tests for critical journeys.
- Build successfully and keep all relevant automated checks green. A passing test suite is evidence, not proof that unspecified cases are correct.

Read [verification-strategy.md](references/verification-strategy.md) when choosing test scope or preparing handoff.

## Resolve trade-offs

Prioritize correctness and security, then clarity and changeability, then operability and measured performance. Follow stricter repository instructions when present. Report any deliberate exception, its evidence, and its risk instead of hiding it.


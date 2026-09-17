# Design and Metrics

Use metrics to locate risk and prompt design reasoning. Never optimize a score independently of behavior and readability.

Use the repository's configured analyzer as the source of truth. If it does not calculate a metric, reason from the visible control flow and treat thresholds as review heuristics; do not claim a precise cognitive-complexity score.

## Method-level signals

| Signal | Target | Required response |
| --- | ---: | --- |
| Cognitive complexity | `<= 8` | At `9-14`, reassess; at `>= 15`, refactor or justify an exception |
| Cyclomatic complexity | `<= 8` | At `9-10`, reassess; above `10`, refactor or justify an exception |
| Nesting depth | `<= 3` | Prefer guard clauses, named predicates, or a coherent rule extraction |
| Method length | about `<= 30` lines | Review the boundary; length alone does not require extraction |
| Parameters | `<= 5` | Consider a value, request, command, or options type when parameters form a concept |

Cognitive complexity is the primary readability signal because nesting and interrupted flow increase mental load. Cyclomatic complexity is a test-path and branching signal. A flat dispatch table can score high cyclomatically while remaining readable; deeply nested code can be hard to understand even when short.

For inherited code already over a threshold, do not make an unrelated change larger merely to reach a target. Do not worsen the signal; reduce it when the required change offers a safe, local opportunity, otherwise record the debt separately.

## Extraction test

Extract only when the new unit can answer at least one question clearly:

- What domain rule does it implement?
- What responsibility or phase does it own?
- What external boundary does it isolate?
- What repeated knowledge becomes authoritative here?
- What independently testable behavior becomes clearer?

Do not extract `Part2`, `ProcessData`, or two-line forwarding methods that add no vocabulary, policy, isolation, or substitutability. Prefer a short orchestration method whose calls read as a use-case narrative, but stop when following the behavior requires excessive jumping.

## Type-level signals

- A class whose methods use disjoint subsets of fields may have low cohesion. Inspect its reasons to change before splitting it; do not act on LCOM alone.
- Types that repeatedly change together may expose change coupling. Consider whether behavior and data belong in one module or whether an unstable contract leaks details.
- A large constructor dependency list can signal mixed responsibilities, but orchestration types naturally coordinate several collaborators. Judge cohesion and use-case scope.
- Broad public APIs, cross-layer references, and project cycles increase the cost and blast radius of change.

## Duplication test

At the third occurrence, compare semantics:

1. Do the copies encode the same business rule or invariant?
2. Must they change together when that rule changes?
3. Can the shared code receive a precise domain name?
4. Would extraction reduce knowledge duplication without adding flags or coupling unrelated callers?

Extract when the answers support one shared concept. Keep copies separate when similarity is accidental or the callers evolve independently.

In tests, preserve local Arrange/Act/Assert readability. Deduplicate costly setup and shared domain vocabulary, but do not hide distinct scenarios behind generic helpers merely to reduce repeated lines.

## Valid exceptions

Generated code, declarative mapping, serialization shapes, and flat dispatch may exceed a signal without being hard to maintain. Record a specific reason and protect behavior with tests. "The analyzer is wrong" or "legacy code" is not sufficient evidence.

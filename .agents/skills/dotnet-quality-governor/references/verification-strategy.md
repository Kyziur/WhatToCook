# Verification Strategy

Select evidence from the risk introduced by the change. Prefer the lowest stable boundary that proves the behavior, then add broader coverage only for risks that cross boundaries.

## Match evidence to risk

| Changed risk | Primary evidence |
| --- | --- |
| Pure domain rule, algorithm, value object | Focused unit/property tests |
| Persistence, transactions, queries, messaging, filesystem | Integration tests against real infrastructure |
| Serialization, HTTP/API contract, authentication/authorization | Contract or API integration tests |
| UI state and component interaction | Component tests |
| Critical user journey or multi-service workflow | Small E2E scenario |
| Concurrency, retry, timeout, cancellation | Deterministic integration tests or controlled fakes at the unstable boundary |
| Performance-sensitive path | Benchmark or workload test with an explicit budget |
| Logging, metrics, tracing, auditing | Tests for important structured events/fields, without asserting incidental message prose |

Unit tests become brittle when they assert private helpers, mock every collaborator, or prescribe call order without a contractual reason. Integration and E2E tests become brittle when they cover every permutation or depend on uncontrolled time, network, and shared state. Test externally meaningful behavior at the narrowest stable seam.

## Scenario coverage

For each implemented contract or business scenario, cover:

- the expected path,
- material boundary values,
- invalid or unauthorized input,
- relevant failure and recovery behavior,
- persistence or side effects that must occur,
- side effects that must not occur.

Use property-based tests when a rule is better expressed as an invariant over many inputs. Use mutation or coverage analysis as a diagnostic for weak tests, not as the objective itself.

## Verification loop

1. Run the smallest affected test or analyzer while implementing.
2. Fix failures before broadening scope.
3. Build the supported solution/configuration.
4. Run the repository's required unit, integration, component, and E2E scope sequentially when shared outputs can conflict.
5. Run formatting, analyzers, architecture checks, and repository hooks.
6. Report exact commands, results, intentional omissions, and remaining risk.

Never claim success from compilation alone. Never hide a failing or skipped check. If infrastructure prevents verification, distinguish an environment blocker from a product failure and preserve the evidence.

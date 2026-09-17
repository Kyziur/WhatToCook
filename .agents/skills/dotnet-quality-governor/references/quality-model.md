# Quality Model

Use this model to reason about substantial changes and trade-offs. Good code makes a correct change safe, local, and economical under the system's real constraints.

## Correctness

- State the observable contract and important invariants before choosing structure.
- Cover invalid input, boundary values, partial failure, and consistency across state changes.
- Keep transaction boundaries aligned with invariants; avoid leaving partially applied work.
- Treat builds, analyzers, and tests as evidence. Check assumptions they do not encode.

## Understandability and changeability

- Make names describe intent and domain meaning.
- Keep data flow, dependencies, state, and side effects visible.
- Prefer code that can be understood from a small neighborhood of files.
- Keep one business rule in one authoritative place so a change does not require shotgun edits.
- Remove dead code, stale feature flags, obsolete compatibility paths, and misleading comments when they are within the changed scope. Record broader debt rather than silently enlarging a feature or fix.

## Cohesion, coupling, and boundaries

- Group behavior that changes for the same reason; separate behavior that changes independently.
- Distinguish cohesion from coupling: LCOM can indicate a class whose methods do not share coherent state, while classes that repeatedly change together indicate change coupling or a misplaced boundary.
- Treat LCOM and co-change history as investigation signals, not standalone verdicts.
- Keep domain policy independent of delivery and infrastructure details.
- Avoid cyclic project/module references, service location, global mutable state, and ambient dependencies.
- Keep public surface area minimal and design public contracts for consumers rather than implementation convenience.

## Simplicity and duplication

- Start with the simplest explicit design that handles real edge cases.
- Introduce an abstraction only when it names a stable concept, protects a boundary, contains variation, or removes duplicated knowledge.
- Use the third occurrence as a prompt to compare reasons to change. Extract shared policy; retain coincidental similarity.
- Reject abstractions that require flags, type checks, or frequent exceptions to represent unrelated concepts.
- Avoid both accidental complexity and naive simplicity that ignores failure, concurrency, security, or data integrity.

## Reliability and evolution

- Define error meaning and ownership; preserve useful context without exposing secrets.
- Make cancellation and timeouts flow through asynchronous I/O. Do not block async work with `.Result` or `.Wait()`.
- Define retry and idempotency together; retry only transient failures and avoid duplicate effects.
- Dispose owned resources and make concurrency and synchronization rules explicit.
- Preserve compatibility deliberately. For breaking contracts or schemas, provide a migration path and rollback considerations.
- Validate configuration at startup and give defaults explicit meaning.

## Security and privacy

- Validate untrusted data at trust boundaries and encode output for its destination.
- Authenticate identity and authorize the specific action and resource.
- Apply least privilege to APIs, storage, infrastructure, and dependencies.
- Keep secrets and personal data out of source, errors, telemetry, and logs.
- Prefer secure defaults and review new packages, serialization, file handling, queries, and outbound requests as attack surfaces.

## Operability and performance

- Emit structured logs at boundaries, failures, and meaningful state transitions with correlation context.
- Prefer metrics for rates/latency and traces for cross-boundary flow; avoid logs that merely narrate execution.
- Define performance budgets for relevant paths and measure before adding non-obvious optimization.
- Prevent obvious N+1 I/O, unbounded queries/collections, repeated expensive work, and avoidable hot-path allocation.
- Document why a non-obvious optimization exists and protect it with a benchmark or performance test when practical.

## Documentation

- Document algorithms, invariants, contracts, security assumptions, compatibility constraints, and surprising trade-offs.
- Explain why the design is constrained. Let names, types, and structure explain what routine code does.
- Update documentation in the same change when its claims or operational procedures change.

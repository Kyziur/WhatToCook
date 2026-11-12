# Verification and operational risk

Use this reference when a change touches data, contracts, or deployment, or when choosing what to test and how to report it.

## Verify the claim, not just the build

Choose checks based on the changed behavior and regression risk:

- Reproduce the bug or identify the failing scenario before fixing it when feasible.
- Cover observable behavior and important failures, especially contract and boundary cases.
- Prefer a small relevant test suite plus a realistic invocation over a large collection of implementation-coupled mocks.
- For refactoring or deletion, confirm the remaining contracts and critical behavior still work; do not delete regression tests for behavior you retain.
- Report checks that could not run and the specific uncertainty they leave.

A meaningful regression test is worth its maintenance cost; asserting mocked implementation details without behavior is not.

## Where trust actually changes

Do not repeat identical validation without a reason. A well-defined internal type or validated value may carry reliable guarantees, but a type alone does not establish every domain invariant. Trust or consistency may change when data arrives from somewhere the current code does not control:

- user input, network responses, files, and external messages,
- deserialized payloads and records written by older versions of the code,
- background jobs or other processes that can modify state between a check and its use.

Check at the boundary that owns each risk. Keep layered enforcement when the protections are different: request-shape validation is not a substitute for a database `UNIQUE` constraint against concurrent writes. Avoid logging tokens, credentials, personal data, or entire payloads by default; log safe identifiers and causes instead.

## Performance changes

Before adding caching, batching infrastructure, parallelism, or bespoke optimization, identify the relevant workload and measure the bottleneck. Record what the optimization costs (invalidation, memory, concurrency, eventual consistency).

## Deployments, data, and contracts

Before changing a database schema, persistent format, external event, public API, security rule, or infrastructure stack:

1. Identify current consumers, versions, persisted data, and ordering constraints.
2. Determine whether the change is backwards compatible. If not, plan the migration or an explicit coordinated cutover.
3. Define a safe deployment sequence and a realistic rollback or recovery path. Not every data migration is reversible.
4. Identify preflight and post-deploy checks plus stop conditions.
5. Follow the environment's permissions and required human approval. Never use production as a speculative test.

**Infrastructure example:** For a service moved between independently managed Compose stacks, verify volume ownership, network connectivity, ordering, and isolation. A green Compose config parse does not prove the service is deployable.

**Application example:** For a change in message serialization or ORM persistence mapping, a passing unit test does not prove compatibility with existing stored records or consumers. Add a contract or migration check where the risk is real.

## Honest completion report

Distinguish:

- **verified**: the command you ran and its result,
- **reasoned**: reviewed but not executed,
- **unknown**: environment or evidence unavailable.

Never invent successful test runs, monitoring coverage, or rollback guarantees.

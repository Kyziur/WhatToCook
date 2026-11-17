# Verification and CI

Read when selecting tests, editing CI pipelines, changing data or deployments, or preparing a completion report.

## Verify behavior proportionately

- Reproduce the failure or identify the expected behavior before changing it when feasible.
- Prefer observable regression tests, relevant integration/contract checks, and a realistic invocation over mocks that assert implementation details.
- Exercise the path a consumer actually runs: the real invocation with its shell, working directory, and platform-specific file attributes where the consumer is sensitive to them, and the failure it would observe (exit status, output, written file). Grepping a script's source proves only that the text exists.
- Check that refactoring or deletion preserves supported behavior; retain useful regression tests.
- Run the narrowest meaningful available checks. Record commands and outcomes; explain what could not be run.

A compilation pass does not prove runtime behavior. Don't duplicate the same assertion merely to increase coverage or job counts.

## CI workflows: cost and ownership

Keep required PR gates small, deterministic, and stable. Evaluate each proposed check by its actual failure mode, coverage already provided, correct execution boundary, and recurring cost. Do not add a job, matrix axis, scheduled task, or custom change-detection framework for a hypothetical benefit.

- Run expensive integration suites, container builds, or platform-specific tests when the affected change or target release warrants them. A documentation-only edit should not trigger unrelated application fixtures. Use supported target platforms to choose matrix dimensions.
- Keep required check names compatible with branch-protection rules. A required workflow skipped by top-level path filters can remain pending; prefer a stable lightweight gate with conditional work inside it, or make optional jobs non-required.
- Consolidate checks that test the same invariant. When removing a workflow, check whether it owns a unique regression assertion and retire obsolete tests or scripts alongside superseded jobs.
- Pin executable/container dependencies to a reproducible version or digest when the threat and maintenance model justify it. Verify that image builds and published artifacts support target architectures; do not hardcode version literals into unrelated regression checks.
- Avoid periodic CI that reruns unchanged source without a meaningful external drift risk. Treat readiness, permissions, network isolation, and restore behavior as deployed-state claims when source checks cannot prove them.
- Keep one-time migration and cutover gates only while the operation remains relevant. Preserve reusable restoration or safety checks where the underlying operation is still supported.

## Neutral calibration scenarios

- A service requires network isolation. One source-level policy check may guard its configuration, while an authorized deployment test is needed to prove runtime traffic cannot escape. Repeating the same configuration assertion across workflows adds cost, not assurance.
- A library change affects native binary support for two target architectures. Verify on those architectures or inspect the published artifact accordingly; skip the matrix for unrelated text changes.
- A database migration includes a restore procedure. Keep the restore verification while the migration is active, then remove temporary cutover automation without removing still-useful recovery coverage.
- An API validates input format; a database `UNIQUE` constraint protects integrity under concurrent requests. These checks guard different failure modes and should both remain.

## Errors, trust, and operational risk

Enforce relevant invariants at the boundaries where they can fail, including external inputs, persisted legacy data, and concurrent state changes. Avoid logging secrets, personal data, or whole payloads; keep errors actionable.

Before modifying schemas, durable data, messages, public APIs, permissions, or infrastructure:

1. Identify current consumers and compatibility or ordering constraints.
2. Define the migration/release sequence and recovery or rollback limits.
3. Select preflight, post-change, and stop conditions.
4. Follow local authorization rules. Never use live systems as speculative test environments.

## Honest completion report

Distinguish observed verification (command and result), reasoned analysis, and unverified runtime assumptions. Never claim CI, deployment safety, or recovery success from source inspection alone.

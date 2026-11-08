# AGENT-06 acceptance evidence

| Acceptance criterion | Test | State |
| --- | --- | --- |
| Apply retains context, task completion and CLI state invariants | `tests/ci/openspec-harness-contract.test.mjs` apply case | CI pending |
| Archive retains in-flight sync, sync verification and advisory path guardrails | `tests/ci/openspec-harness-contract.test.mjs` archive case | CI pending |
| Harness syntax and generated content unmodified | Diff inspection | Author-side checked; independent review pending |
| No application behavior changes | Diff limited to Node test, CI, OpenSpec and audit | Author-side checked |

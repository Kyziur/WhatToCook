# PERF-03 test traceability

| Requirement scenario | Automated verification | State |
| --- | --- | --- |
| Selective substring plans at 1k/10k | `RecipeSubstringSearchDiagnosticsTests.SubstringSearch_ComparePlansAtOneAndTenThousandRecipes` | VERIFIED in CI 42483387937 (full run formatting failed; fix pending) |
| Stable result counts and ordering | Same test: baseline vs indexed `AssertSameResults` | VERIFIED in CI 42483387937 (full run formatting failed; fix pending) |
| Short/common phrase diagnostics | Same test: `domowe` and `zu` probes | VERIFIED in CI 42483387937 (full run formatting failed; fix pending) |
| Production schema unaffected | Index DDL exists only in test fixture and no migrations are changed | VERIFIED: no application migration added; CI 42483387937 |

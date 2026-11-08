# PERF-03 test traceability

| Requirement scenario | Automated verification | State |
| --- | --- | --- |
| Selective substring plans at 1k/10k | `RecipeSubstringSearchDiagnosticsTests.SubstringSearch_ComparePlansAtOneAndTenThousandRecipes` | IMPLEMENTED, CI pending |
| Stable result counts and ordering | Same test: baseline vs indexed `AssertSameResults` | IMPLEMENTED, CI pending |
| Short/common phrase diagnostics | Same test: `domowe` and `zu` probes | IMPLEMENTED, CI pending |
| Production schema unaffected | Index DDL exists only in test fixture and no migrations are changed | REVIEW pending |

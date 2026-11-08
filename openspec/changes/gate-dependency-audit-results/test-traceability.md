# SEC-08 test traceability

| Scenario | Evidence |
| --- | --- |
| High/Critical NuGet advisory blocks (direct and transitive) | `tests/ci/nuget-audit-gate.test.mjs` high/critical test; CI npm exit policy (verified in PR CI run 42465836173) |
| Low/Moderate NuGet advisory is reported but not blocking | `tests/ci/nuget-audit-gate.test.mjs` warning-only test |
| Malformed NuGet report fails closed | `tests/ci/nuget-audit-gate.test.mjs` malformed test |
| npm audit command failure cannot be hidden | PR workflow shell exit status (verified in PR CI run 42465836173) |
| Available reports retained | GitHub Actions `upload-artifact` with `if: always()` (verified in PR CI run 42465836173) |

# PERF-05 test traceability

| Requirement | Verification | Evidence status |
| --- | --- | --- |
| Standard ASP.NET Core 10 circuit/render/SignalR meters registered | Web `Program.cs` OpenTelemetry registration + profiler receives live built-in metrics | Initial run 42505345186: 5 and 20 circuits confirmed; discovered runtime render histogram suffix `.duration`; compatibility fix and final CI pending |
| No production probe/HTTP endpoint | Development-only `AddHostedService` guard, static review of routes | Review pending |
| Five and twenty interactive circuits | `tests/e2e/blazor-circuit-profile.spec.ts` Playwright real tab cohorts | CI pending |
| Connected count drains after five tabs | Profile test checks `ConnectedCircuits == 0` between phases | CI pending |
| Memory/GC, browser search-to-DOM p50/p95 and available server render p50/p95 without thresholds | `BlazorCircuitProfiler` JSONL artifact + Playwright console | CI pending |

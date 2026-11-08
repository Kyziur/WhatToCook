# PERF-05 test traceability

| Requirement | Verification | Evidence status |
| --- | --- | --- |
| Standard ASP.NET Core 10 circuit/render/SignalR meters registered | Web `Program.cs` OpenTelemetry registration + profiler receives live built-in metrics | Run 42505894142: 5/20 circuit peaks and native `.duration` histogram confirmed; see `evidence.md` |
| No production probe/HTTP endpoint | Development-only `AddHostedService` guard, static review of routes | Author-side static check: Development-only registration; independent review pending |
| Five and twenty interactive circuits | `tests/e2e/blazor-circuit-profile.spec.ts` Playwright real tab cohorts | Run 42505894142 passed; see `evidence.md` |
| Connected count drains after five tabs | Profile test checks `ConnectedCircuits == 0` between phases | Run 42505894142 passed; see `evidence.md` |
| Memory/GC, browser search-to-DOM p50/p95 and available server render p50/p95 without thresholds | `BlazorCircuitProfiler` JSONL artifact + Playwright console | Run 42505894142 passed; see `evidence.md` |

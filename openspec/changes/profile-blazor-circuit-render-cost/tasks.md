## Implementation and verification

- [x] Subscribe to existing Blazor/SignalR meter sources in the Web OpenTelemetry setup.
- [x] Add an opt-in Development-only local probe for render diff, circuit counts and process memory/GC.
- [x] Add a dedicated Playwright 5/20-tab diagnostic, distinct from routine regression smoke, plus CI artifact.
- [ ] Run CI, correct any reliability issues, verify actual metric output and record a measured decision.
- [ ] Review all changes and mark the audit PERF-05 appropriately; merge only with green final-head CI.

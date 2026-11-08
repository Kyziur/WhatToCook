## Implementation and verification

- [x] Subscribe to existing Blazor/SignalR meter sources in the Web OpenTelemetry setup.
- [x] Add an opt-in Development-only local probe for render diff, circuit counts and process memory/GC.
- [x] Add a dedicated Playwright 5/20-tab diagnostic, distinct from routine regression smoke, plus CI artifact.
- [x] Run CI, correct any reliability issues, verify actual metric output and record a measured decision.
- [x] Review implementation risk and mark PERF-05 IN REVIEW in the audit; keep operational/independent review outstanding.
- [ ] Merge only after green **final-head** CI and update the merged audit entry.

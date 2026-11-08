## Why

PERF-05 observed long-lived Interactive Server circuit state in the recipe library but no evidence that render cost, GC, memory or concurrent session counts are a problem. .NET 10 has built-in circuit and render metrics not yet subscribed by the Web OpenTelemetry host.

## What Changes

- Subscribe only to existing built-in Blazor/SignalR meters in the Web app's OpenTelemetry pipeline.
- Add an explicit Development-only, file-based meter probe, with no public HTTP endpoint or production background listener.
- Profile five and twenty simultaneously connected browser tabs using the existing real PostgreSQL API/Web Playwright stack. Record connected/active circuits, process memory, approximate allocation/GC and server render-diff p50/p95.
- Verify core interactive UI navigation and title search at both session counts, without setting performance-based CI thresholds.
- Document evidence and a no-speculative-refactor decision.

## Non-Goals

No component split, `ShouldRender` overrides, changes to circuit retention/reconnect settings, public diagnostic endpoints, benchmark-derived SLA, or production host capacity claims.

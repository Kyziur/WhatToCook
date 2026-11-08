## ADDED Requirements

### Requirement: Web host publishes built-in Blazor circuit and render metrics
The Web host SHALL subscribe to built-in .NET 10 Blazor metrics using the existing OpenTelemetry setup, without introducing an unauthenticated metrics endpoint.

#### Scenario: Blazor session and rendering are observable
- **WHEN** the Web app runs with its existing OpenTelemetry exporter enabled
- **THEN** the exported meter sources include connected/active circuit and render-diff observations

#### Scenario: No profiling listener in production
- **WHEN** the Web app runs in Production
- **THEN** the optional file-based PERF-05 profiler is never registered

### Requirement: Session profile verifies interaction behavior without arbitrary speed gates
Maintainers SHALL be able to run an explicit real-browser diagnostic with five and twenty simultaneously connected Blazor tabs.

#### Scenario: Five and twenty connected tabs
- **WHEN** the opt-in profile test loads the recipe library and performs title search in batches of 5 and 20 tabs
- **THEN** every tab remains interactive, connected-circuit counts reach the expected cohort size, and summaries include active circuits, working set, allocation/GC deltas, browser search-to-DOM p50/p95 and available server render p50/p95 (or an explicit missing-sample marker)

#### Scenario: Disconnect does not imply immediate circuit disposal
- **WHEN** the five-tab cohort is closed
- **THEN** connected circuits return to zero before the next cohort; active circuits may remain retained for reconnection

#### Scenario: No timing thresholds
- **WHEN** the profile runs on a shared CI runner
- **THEN** correctness/observability checks are gating, but memory consumption and timing figures are recorded without arbitrary pass/fail thresholds

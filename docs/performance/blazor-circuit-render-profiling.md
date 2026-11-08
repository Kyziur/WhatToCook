# PERF-05 — Blazor circuit and render profiling

## Scope and no-premature-optimization rule

The Web host uses .NET 10 Blazor Interactive Server. Component state lives in a **server-side circuit per tab**. The existing recipe library is paginated and uses `@key="recipe.Id"` for its result cards. There is no demonstrated excessive re-render or memory failure that justifies a component split or `ShouldRender` override.

The Web host now subscribes to built-in ASP.NET Core meters through its existing OpenTelemetry pipeline: `Microsoft.AspNetCore.Components`, `Microsoft.AspNetCore.Components.Lifecycle`, `Microsoft.AspNetCore.Components.Server.Circuits`, and `Microsoft.AspNetCore.Http.Connections`. The regular OTLP exporter remains controlled by `OTEL_EXPORTER_OTLP_ENDPOINT`; no public metrics HTTP endpoint is created.

## Reproduce on an isolated development environment

Run after building the solution, with Docker available:

```sh
npm ci
npx playwright install chromium
PERF05_PROFILE=1 PLAYWRIGHT_DOTNET_CONFIGURATION=Release npx playwright test --config=playwright.config.ts --project=desktop-chromium --grep "@perf05"
```

`npm ci` installs the lockfile Playwright version, `playwright install chromium` provisions the browser, and the final command starts the existing ephemeral PostgreSQL/API/Web E2E stack and explicitly runs only the circuit probe. On Windows PowerShell, set the two environment variables with `$env:PERF05_PROFILE="1"` and `$env:PLAYWRIGHT_DOTNET_CONFIGURATION="Release"` first; the normal existing E2E commands still work unchanged.

The browser test opens **five then twenty tabs** with isolated Blazor circuits against the local server, waits for interactive library rendering, and triggers a title search in each tab. These are **20 tabs in one browser context**, not 20 independent desktop processes, nor a production load test. It checks the Blazor `connected` metric reaches each cohort size. Connected circuits must drain after the first cohort; **active circuits can remain in memory** for reconnection, so a higher active count after closing tabs is not a leak by itself.

The opt-in, Development-only `BlazorCircuitProfiler` uses `MeterListener` to sample built-in `aspnetcore.components.circuit.active`, `aspnetcore.components.circuit.connected`, and `aspnetcore.components.render_diff`. It writes one **local JSONL snapshot per second** at `.tmp/playwright-e2e/perf05-metrics.jsonl`, under explicit `PERF05_PROFILE_PATH` and `PERF05_PHASE_PATH`. It never runs in Production and does not expose an unauthenticated diagnostic endpoint.

Each snapshot includes currently connected/active circuits, server process working set, approximate allocated bytes, GC collection increments since that phase began, and median/p95 server render-diff duration. Playwright prints a compact summary per cohort. CI also attaches the raw JSONL file.

## Interpretation

- `RenderP50Ms`/`RenderP95Ms` describe **server render-diff batches** observed by the .NET runtime. They are **not** end-to-end browser paint, network latency, or user-visible interaction p95.
- Process working set and process-wide allocation counters include API HTTP-client infrastructure, runtime warmup and unrelated work. `WorkingSetBytes` is a process snapshot, not memory per circuit.
- Snapshots are 1-second samples and the renderer histogram samples are captured with minimal test-only overhead. Measurements are noisy and do not gate CI on arbitrary millisecond/MB limits.
- No forced GC occurs; disconnected circuits may remain retained by Blazor for reconnection. A larger working set with 20 tabs does not establish a memory leak.
- Real target hardware, reverse proxy, browser devices, and family usage patterns must be observed before changing component structure or deciding on a circuit budget.

## Next decision

Review at least three repeat runs on a runner and, if possible, an equivalent 5/20 tab sample on the homelab host. If median/p95 render diff or per-session footprint is materially elevated, profile the specific interaction/component before adding a conditional render or extracting more components. Otherwise retain the existing UI; monitor `circuit.connected`, `circuit.active` and render histogram via the existing OTel collector.

References: [ASP.NET Core 10 built-in Blazor metrics](https://learn.microsoft.com/en-us/aspnet/core/log-mon/metrics/built-in-components?view=aspnetcore-10.0), [Blazor performance guidance](https://learn.microsoft.com/en-us/aspnet/core/blazor/performance?view=aspnetcore-10.0).

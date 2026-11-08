## Why

OBS-05 identifies recipe title and internal photo path values in structured application logs exported through OpenTelemetry. These fields are unnecessary for operational diagnosis, can contain household content and increase telemetry cardinality.

## What Changes

- Replace title and normalized-title fields in recipe editor API logs with stable recipe IDs or bounded event outcome categories.
- Keep photo upload and failed cleanup diagnostics but do not log storage paths or exception text that may contain host paths.
- Add an HTTP integration regression asserting user-controlled recipe titles are absent from rendered and structured API log values.

## Non-Goals

This is **application-side log minimization**, not a complete OBS-05 closure. It does not change API results, request/response bodies, telemetry exporter, collector policy, retention, redaction of other components, or homelab configuration. Collector-level inspection remains a separate operational verification.

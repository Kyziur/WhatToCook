# PERF-06 — Browser-level verification of the opt-in image probe

## Why
The previous PERF-06 PR tested pure aggregation and the CLI's JavaScript syntax but did not execute the probe against a browser. A mistaken Playwright API call, network cache handling or image-selection predicate could silently leave the tool unusable despite green helper tests.

## Changes
- Use a local HTTP-only test fixture with one decodable synthetic image and an ETag/304 revalidation path. Invoke the **real CLI in Chromium** to verify first and repeat visit reporting, including absence of sensitive URLs in JSON.
- Cover a photo-free library with an actual browser process: the CLI must reject it instead of providing a fake zero-transfer baseline.
- Add a bounded configurable photo-load timeout for deterministic CI and slow local hosts.
- Run this focused browser contract in PR CI *after* the existing Chromium installation.

## Non-goals
No production API/Web/DB changes, no thumbnails or transcodes, no automatic access to household photos, no synthetic benchmark presented as performance data, no deployment or live-host assumptions.

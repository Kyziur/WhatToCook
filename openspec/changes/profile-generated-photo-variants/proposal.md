# PERF-06 — Temporary generated photo transfer comparison

## Why
The existing PERF-06 probe works with real browser/ETag fixtures but no browser-executed comparison for realistic *volumes* of image payloads, source dimensions and 320/640 px alternatives. Household images are not necessary to validate the measurement flow or size sensitivity.

## Change
- Deterministically draw 36 varied, food-like raster images using Chromium Canvas; encode JPEG, PNG and WebP. No external downloads or source photographs.
- Create 36 temporary recipes in the existing isolated E2E PostgreSQL environment and upload originals, then 640 and 320 px replacements via the actual API.
- For desktop/mobile viewport emulation capture first/repeat transferred bytes across both 24/12-result library pages, explicitly scrolling through lazy-loaded images.
- Publish aggregate-only JSON evidence as a CI artifact; archive recipes and remove per-run image storage after the E2E harness ends.
- Gate on correctness (valid uploads, decoded images, full resource coverage and smaller encoded fixture variants), not absolute timings. Do not change production image handling.

## Non-goals
No real household dataset, new image pipeline, CDN, storage migrations, measured p95 or mobile handset CPU/network evidence. The synthetic images are deliberately not claimed to be photographs.

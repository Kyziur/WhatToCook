# PERF-06 — Recipe image transfers and browser cache

## Current fix (partial)

The API keeps original uploaded JPEG, PNG, GIF and WebP bytes, bounded by existing 5 MiB / 20 MP upload checks. The Web route `/recipe-images/{filePath}` proxies them from the API.

The Web proxy now forwards only `If-None-Match` and `If-Modified-Since` to the API, passes ETag and Last-Modified back, and handles the API's **304 Not Modified** response. `Cache-Control: private, no-cache` allows private browser storage while **requiring validation** before reuse. This prevents an unchanged response body on successful conditional validation, but still incurs a lightweight request. It does not change the original bytes or add a CDN.

An existing Playwright recipe upload scenario verifies first GET (200 + ETag) and conditional GET (304, no body). This is a **transport correctness regression test**, not evidence of faster production page loads.

## Real-image baseline before making thumbnails

Use a device/network representative of everyday use and a library with representative **actual user photos**, not the sample seed (which cannot establish real transfer cost). On browser DevTools **Network**:

1. Use **Disable cache** and load the library once. Record the number of image requests, response bytes for `/recipe-images/`, widths (`naturalWidth` vs displayed `clientWidth`) and LCP. Keep conditions and viewport consistent.
2. Re-enable cache, revisit the same list and details. Record whether requests carry `If-None-Match` / `If-Modified-Since` and return 304 with zero image bytes. Record total image transferred bytes again.
3. Repeat under representative Wi-Fi and throttled mobile profiles. A candidate for image variants exists only if substantial over-resolution/bytes persist on first visits; a browser cache fix alone does not solve first-load cost.
4. Before implementing variants, document baseline page/phone, photo counts, source dimensions and formats, initial/return navigation transfer totals, LCP, and the intended threshold. Test quality/fallback, deletion lifecycle and storage cost after implementation.

Do not interpret an ETag/304 successful CI test or a synthetic one-pixel test image as a performance benchmark. Do not enable speculative thumbnail generation based on these fixtures.

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

## Optional reproducible browser probe (not a production result)

From the repository checkout, with the Web application reachable and **real user photos** in the library:

```sh
npm ci
npx playwright install chromium
node scripts/perf/measure-image-transfer.mjs --url http://127.0.0.1:5180/ --mobile --output .tmp/perf06-mobile.json
```

`npm ci` installs the lockfile Playwright version; `playwright install chromium` installs its browser; the last command opens an isolated mobile-sized Chromium context, records the first library visit and repeats it in the same context, then writes JSON locally. Replace the URL with the **Web** address reachable from the machine where the probe runs. No data is sent to the project CI, and no recipe titles or photo paths are included in the report. Omit `--mobile` for a desktop viewport; use `--settle-ms 3000` only when a repeatable longer load settle is needed.

The report lists image-resource count, `PerformanceResourceTiming.transferSize` (includes response headers), encoded body size reported by the browser, zero-transfer entries, loaded photos visible in the viewport, source-width-to-displayed-width ratio adjusted for device pixel ratio, and one LCP observation for each visit. The probe samples **only first-viewport lazy-loaded images**; scroll/paginated pages, network throttling, browser cache policies, HTTP 304 status, phone GPU speed, and production p95 are **not proven** by this script. Do not interpret the `encodedBodyBytesReported` of a cached/304 resource as bytes downloaded on revisit; use `transferBytesIncludingHeaders` for the byte comparison. A zero-transfer entry may be served from browser memory/cache; it does not itself prove a 304 response.

**Decision gate:** run this against actual household images on both normal Wi-Fi and a representative slower device/network. Compare first-visit transfer bytes, visible over-resolution count, and LCP observations across repeat runs. Only consider 320/640 px variants when these measurements and observed image quality justify storage/complexity. This probe does not change production image storage or caching.

## Browser probe verification (CI fixture)

The PR CI suite also runs `node --test tests/ci/image-transfer-browser.test.mjs` after installing Playwright Chromium. This launches the **actual measurement CLI** against an isolated, local HTTP fixture with a 200 image response, ETag and conditional 304 revalidation, and verifies the generated report excludes image paths and host URLs. A negative fixture without photos must fail rather than produce an invalid zero-transfer report.

The fixture browser path is verified by [PR #48 CI 42616962766](https://github.com/Kyziur/WhatToCook/actions/runs/42616962766) (2/2 probe tests passed). This verifies the browser-and-script path, **not real-photo performance**. The manual CLI accepts optional `--load-timeout-ms 30000` for a slower host (default 15000, range 100–60000); this only changes how long the probe waits for a loaded photo. Actual household measurements still require a host-reachable Web URL and representative device/network conditions.

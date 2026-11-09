import { chromium } from "@playwright/test";
import { mkdir, writeFile } from "node:fs/promises";
import path from "node:path";
import { parseProbeArgs, summarizeImageCapture } from "./image-transfer-metrics.mjs";

async function capture(page, url, settleMs, reload) {
  if (reload) {
    await page.reload({ waitUntil: "domcontentloaded", timeout: 30000 });
  } else {
    await page.goto(url, { waitUntil: "domcontentloaded", timeout: 30000 });
  }

  // The report requires real photos in the loaded library, not a seeded placeholder.
  await page.locator(".recipe-card img[src*='/recipe-images/']").first().waitFor({
    state: "attached",
    timeout: 15000,
  });
  await page.waitForTimeout(settleMs);

  const raw = await page.evaluate(() => {
    const resources = performance.getEntriesByType("resource").map((resource) => ({
      name: resource.name,
      transferSize: resource.transferSize,
      encodedBodySize: resource.encodedBodySize,
    }));
    const images = [...document.querySelectorAll("img")].map((image) => {
      const rect = image.getBoundingClientRect();
      const source = image.currentSrc || image.src;
      return {
        source,
        naturalWidth: image.naturalWidth,
        displayedWidth: rect.width,
        loaded: image.complete && image.naturalWidth > 0,
        visible:
          rect.width > 0 &&
          rect.height > 0 &&
          rect.bottom > 0 &&
          rect.top < window.innerHeight,
      };
    }).filter((image) => new URL(image.source, location.href).pathname.startsWith("/recipe-images/"));
    return {
      resources,
      images,
      dpr: window.devicePixelRatio,
      lcpMs: window.__whatToCookPerf06LcpMs ?? null,
    };
  });

  return summarizeImageCapture(raw);
}

async function main() {
  const options = parseProbeArgs(process.argv.slice(2));
  const browser = await chromium.launch({ headless: true });

  try {
    const context = await browser.newContext(
      options.mobile
        ? { viewport: { width: 390, height: 844 }, deviceScaleFactor: 3, isMobile: true, hasTouch: true }
        : { viewport: { width: 1440, height: 900 }, deviceScaleFactor: 1 }
    );
    const page = await context.newPage();
    await page.addInitScript(() => {
      window.__whatToCookPerf06LcpMs = null;
      try {
        new PerformanceObserver((list) => {
          for (const entry of list.getEntries()) {
            window.__whatToCookPerf06LcpMs = entry.startTime;
          }
        }).observe({ type: "largest-contentful-paint", buffered: true });
      } catch {
        // LCP unsupported: report null rather than inventing a measurement.
      }
    });

    // A new, isolated browser context starts with an empty cache.
    const firstVisit = await capture(page, options.url, options.settleMs, false);
    const repeatVisit = await capture(page, options.url, options.settleMs, true);
    const report = {
      probe: "PERF-06",
      mode: options.mobile ? "mobile-viewport" : "desktop-viewport",
      settleMs: options.settleMs,
      firstVisit,
      repeatVisit,
      limitations:
        "Browser ResourceTiming transferSize includes headers and is not exact image payload. " +
        "LCP is a single browser observation, not p95. Lazy-loaded images below the fold are excluded. " +
        "Results require a real photo library and do not prove thumbnails are necessary.",
    };
    const json = JSON.stringify(report, null, 2) + "\n";
    if (options.output) {
      await mkdir(path.dirname(options.output), { recursive: true });
      await writeFile(options.output, json);
    }
    process.stdout.write(json);
    await context.close();
  } finally {
    await browser.close();
  }
}

main().catch((error) => {
  console.error(`PERF-06 probe failed: ${error.message}`);
  process.exitCode = 1;
});

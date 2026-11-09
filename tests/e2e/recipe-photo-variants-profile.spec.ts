import fs from "node:fs";
import path from "node:path";
import { randomUUID } from "node:crypto";
import { expect, test, type APIRequestContext, type Browser, type Locator, type Page } from "@playwright/test";
import { summarizeImageCapture } from "../../scripts/perf/image-transfer-metrics.mjs";
import { gotoAndWaitForApp } from "./helpers";

const apiUrl = "http://127.0.0.1:5181";
const outputPath = path.join(__dirname, "..", "..", ".tmp", "playwright-e2e", "perf06-generated-metrics.json");
const imageLimitBytes = 5 * 1024 * 1024;
const pageSize = 24;

type Variant = "original" | "640" | "320";
type GeneratedPhoto = {
  mimeType: string;
  extension: string;
  original: Buffer;
  "640": Buffer;
  "320": Buffer;
};
type Recipe = { id: string; image: GeneratedPhoto };
type Visit = {
  transferredBytes: number;
  encodedBodyBytesReported: number;
  resourceCount: number;
  visibleLoadedImageCount: number;
  overResolutionCount: number;
  largestDimensionRatio: number | null;
  firstPageLcpMs: number | null;
};
type Profile = {
  variant: Variant;
  viewport: "desktop" | "mobile";
  fixtureBodyBytes: number;
  firstVisit: Visit;
  repeatVisit: Visit;
};

// Browser Canvas is already available through the installed Playwright Chromium.
// These procedural food-like scenes are deterministic and have varied texture,
// pixel counts and JPEG/WebP/PNG compression. They are NOT actual food photographs.
async function generatePhoto(page: Page, index: number): Promise<GeneratedPhoto> {
  const encoded = await page.evaluate((seedIndex) => {
    const dimensions = [
      [1600, 1000],
      [2048, 1536],
      [3000, 2000],
      [1800, 2400],
    ];
    const [width, height] = dimensions[seedIndex % dimensions.length];
    const mimeType = ["image/jpeg", "image/webp", "image/png"][seedIndex % 3];
    const canvas = document.createElement("canvas");
    canvas.width = width;
    canvas.height = height;
    const ctx = canvas.getContext("2d");
    if (!ctx) throw new Error("Canvas 2D context unavailable");

    let seed = (seedIndex + 17) * 2654435761;
    const random = () => {
      seed ^= seed << 13;
      seed ^= seed >>> 17;
      seed ^= seed << 5;
      return (seed >>> 0) / 4294967296;
    };

    const base = ctx.createLinearGradient(0, 0, width, height);
    base.addColorStop(0, "#ae835c");
    base.addColorStop(0.6, "#6d4637");
    base.addColorStop(1, "#302723");
    ctx.fillStyle = base;
    ctx.fillRect(0, 0, width, height);

    // Large plate/bowl and varied colourful ingredients. Non-uniform textured
    // content avoids the misleading transfer sizes of single-colour placeholders.
    ctx.save();
    ctx.translate(width * 0.5, height * 0.5);
    ctx.rotate((random() - 0.5) * 0.45);
    const radius = Math.min(width, height) * 0.42;
    ctx.beginPath();
    ctx.ellipse(0, 0, radius * 1.3, radius, 0, 0, Math.PI * 2);
    ctx.fillStyle = "#d8cdb4";
    ctx.fill();
    ctx.beginPath();
    ctx.ellipse(0, 0, radius * 1.12, radius * 0.86, 0, 0, Math.PI * 2);
    ctx.fillStyle = "#54402a";
    ctx.fill();
    const palette = ["#d64f27", "#d2b04a", "#3e792c", "#8ca342", "#c3a26e", "#e4dd9b", "#792d22"];
    for (let n = 0; n < 650; n++) {
      const angle = random() * Math.PI * 2;
      const distance = Math.sqrt(random()) * radius * 0.78;
      const r = radius * (0.006 + random() * 0.065);
      ctx.beginPath();
      ctx.ellipse(
        Math.cos(angle) * distance,
        Math.sin(angle) * distance,
        r,
        r * (0.4 + random()),
        random() * Math.PI,
        0,
        Math.PI * 2
      );
      ctx.fillStyle = palette[Math.floor(random() * palette.length)];
      ctx.globalAlpha = 0.45 + random() * 0.5;
      ctx.fill();
    }
    ctx.restore();
    ctx.globalAlpha = 1;

    const encode = (targetWidth: number) => {
      const scale = targetWidth / width;
      const target = document.createElement("canvas");
      target.width = targetWidth;
      target.height = Math.max(1, Math.round(height * scale));
      const targetCtx = target.getContext("2d");
      if (!targetCtx) throw new Error("Thumbnail canvas unavailable");
      targetCtx.imageSmoothingEnabled = true;
      targetCtx.imageSmoothingQuality = "high";
      targetCtx.drawImage(canvas, 0, 0, target.width, target.height);
      const dataUrl = target.toDataURL(mimeType, 0.86);
      if (!dataUrl.startsWith("data:" + mimeType + ";base64,")) {
        throw new Error("Browser encoder does not support " + mimeType);
      }
      return dataUrl.split(",")[1];
    };
    const original = canvas.toDataURL(mimeType, 0.86);
    if (!original.startsWith("data:" + mimeType + ";base64,")) {
      throw new Error("Browser encoder does not support " + mimeType);
    }
    return { mimeType, original: original.split(",")[1], "640": encode(640), "320": encode(320) };
  }, index);

  const extension = { "image/jpeg": "jpg", "image/webp": "webp", "image/png": "png" }[encoded.mimeType];
  if (!extension) throw new Error("Unexpected generated image format");
  const photo = {
    mimeType: encoded.mimeType,
    extension,
    original: Buffer.from(encoded.original, "base64"),
    "640": Buffer.from(encoded["640"], "base64"),
    "320": Buffer.from(encoded["320"], "base64"),
  };
  for (const variant of ["original", "640", "320"] as const) {
    if (photo[variant].length === 0 || photo[variant].length > imageLimitBytes) {
      throw new Error("Generated " + variant + " image violates API file size limit");
    }
  }
  return photo;
}

async function createRecipe(request: APIRequestContext, title: string): Promise<string> {
  const response = await request.post(apiUrl + "/api/recipes", {
    data: {
      title,
      servings: 4,
      ingredients: [{ name: "Pomidor", quantityText: "1", unit: "szt" }],
      steps: ["Przygotuj składniki"],
      tags: ["perf06"],
    },
  });
  expect(response.status(), "Creating isolated benchmark recipe").toBe(201);
  const created = await response.json();
  expect(created.id).toMatch(/^[0-9a-f-]{36}$/i);
  return created.id;
}

async function uploadVariant(request: APIRequestContext, recipe: Recipe, variant: Variant): Promise<void> {
  const response = await request.post(apiUrl + "/api/recipes/" + recipe.id + "/photo", {
    multipart: {
      file: {
        name: "generated." + recipe.image.extension,
        mimeType: recipe.image.mimeType,
        buffer: recipe.image[variant],
      },
    },
  });
  expect(response.ok(), "Uploading " + variant + " variant through real API").toBeTruthy();
  const result = await response.json();
  expect(result.mainPhotoPath).toMatch(/^[0-9a-f]+\.(jpg|png|webp)$/);
}

async function loadAllPhotos(page: Page, images: Locator, expectedCount: number): Promise<void> {
  await expect(images).toHaveCount(expectedCount);
  const dimensions = await page.evaluate(() => ({
    height: document.documentElement.scrollHeight,
    viewport: window.innerHeight,
  }));
  const step = Math.max(1, Math.floor(dimensions.viewport * 0.6));
  for (let y = 0; y <= dimensions.height; y += step) {
    await page.evaluate((scrollY) => window.scrollTo(0, scrollY), y);
    await page.waitForTimeout(35);
  }
  await expect
    .poll(
      async () =>
        images.evaluateAll((elements) =>
          elements.filter((element) => (element as HTMLImageElement).complete && (element as HTMLImageElement).naturalWidth > 0).length
        ),
      { timeout: 30_000 }
    )
    .toBe(expectedCount);
  await page.evaluate(() => window.scrollTo(0, 0));
}

async function captureLibraryPage(page: Page, expected: number, firstPage: boolean) {
  const images = page.locator(".recipe-card img[src*='/recipe-images/']");
  await loadAllPhotos(page, images, expected);
  const snapshot = await page.evaluate(() => {
    const resources = performance.getEntriesByType("resource").map((entry) => ({
      name: entry.name,
      transferSize: (entry as PerformanceResourceTiming).transferSize,
      encodedBodySize: (entry as PerformanceResourceTiming).encodedBodySize,
    }));
    const images = Array.from(document.querySelectorAll<HTMLImageElement>(".recipe-card img[src*='/recipe-images/']")).map((image) => {
      const rect = image.getBoundingClientRect();
      return {
        source: image.currentSrc || image.src,
        naturalWidth: image.naturalWidth,
        displayedWidth: rect.width,
        loaded: image.complete && image.naturalWidth > 0,
        visible: rect.width > 0 && rect.height > 0 && rect.bottom > 0 && rect.top < window.innerHeight,
      };
    });
    return {
      dpr: window.devicePixelRatio,
      resources,
      images,
      lcpMs: (window as typeof window & { __perf06LcpMs?: number | null }).__perf06LcpMs ?? null,
    };
  });
  const summary = summarizeImageCapture(snapshot);
  expect(summary.resourceCount, "Each requested library photo must produce a browser resource entry").toBe(expected);
  return { ...summary, lcpMs: firstPage ? summary.lcpMs : null };
}

async function runVisit(page: Page, prefix: string, count: number): Promise<Visit> {
  await gotoAndWaitForApp(page, "/?q=" + encodeURIComponent(prefix));
  const firstPage = await captureLibraryPage(page, Math.min(pageSize, count), true);

  let secondPage = null;
  if (count > pageSize) {
    // Blazor changes the catalog without a document navigation; separate the
    // second page's browser resource timing from the first page's images.
    await page.evaluate(() => performance.clearResourceTimings());
    await page.locator("#library-next-page").click();
    secondPage = await captureLibraryPage(page, count - pageSize, false);
  }

  return {
    transferredBytes: firstPage.transferBytesIncludingHeaders + (secondPage?.transferBytesIncludingHeaders ?? 0),
    encodedBodyBytesReported: firstPage.encodedBodyBytesReported + (secondPage?.encodedBodyBytesReported ?? 0),
    resourceCount: firstPage.resourceCount + (secondPage?.resourceCount ?? 0),
    visibleLoadedImageCount: firstPage.visibleLoadedImageCount,
    overResolutionCount: firstPage.overResolutionCount,
    largestDimensionRatio: firstPage.largestDimensionRatio,
    firstPageLcpMs: firstPage.lcpMs,
  };
}

async function profileVariant(browser: Browser, prefix: string, count: number, variant: Variant, fixtureBodyBytes: number): Promise<Profile[]> {
  const profiles: Profile[] = [];
  for (const viewport of ["desktop", "mobile"] as const) {
    const context = await browser.newContext(
      viewport === "mobile"
        ? { viewport: { width: 390, height: 844 }, deviceScaleFactor: 3, isMobile: true, hasTouch: true }
        : { viewport: { width: 1440, height: 900 }, deviceScaleFactor: 1 }
    );
    try {
      const page = await context.newPage();
      await page.addInitScript(() => {
        (window as typeof window & { __perf06LcpMs?: number | null }).__perf06LcpMs = null;
        try {
          new PerformanceObserver((list) => {
            for (const entry of list.getEntries()) {
              (window as typeof window & { __perf06LcpMs?: number | null }).__perf06LcpMs = entry.startTime;
            }
          }).observe({ type: "largest-contentful-paint", buffered: true });
        } catch {
          // LCP is optional; a missing observation is reported as null.
        }
      });
      const firstVisit = await runVisit(page, prefix, count);
      const repeatVisit = await runVisit(page, prefix, count);
      expect(firstVisit.resourceCount).toBe(count);
      expect(repeatVisit.resourceCount).toBe(count);
      expect(firstVisit.transferredBytes).toBeGreaterThan(0);
      profiles.push({ variant, viewport, fixtureBodyBytes, firstVisit, repeatVisit });
    } finally {
      await context.close();
    }
  }
  return profiles;
}

test("@perf06-generated compare real Web transfers of procedural original/640/320 photo uploads", async ({ browser, request }) => {
  test.skip(process.env.PERF06_PROFILE !== "1", "Run through the isolated PERF06_PROFILE=1 workflow");
  test.setTimeout(720_000);
  const count = Number(process.env.PERF06_SAMPLE_COUNT ?? "36");
  expect(Number.isInteger(count) && count >= 30 && count <= 50, "Use 30-50 photos per profile").toBeTruthy();

  const prefix = "PERF06" + randomUUID().replaceAll("-", "").slice(0, 10);
  const createdIds: string[] = [];
  const recipes: Recipe[] = [];
  const profiles: Profile[] = [];
  const generatorContext = await browser.newContext();
  try {
    const generator = await generatorContext.newPage();
    for (let index = 0; index < count; index++) {
      const image = await generatePhoto(generator, index);
      const id = await createRecipe(request, prefix + " " + String(index + 1).padStart(2, "0"));
      createdIds.push(id);
      recipes.push({ id, image });
    }

    for (const variant of ["original", "640", "320"] as const) {
      for (const recipe of recipes) {
        await uploadVariant(request, recipe, variant);
      }
      const fixtureBodyBytes = recipes.reduce((total, recipe) => total + recipe.image[variant].length, 0);
      profiles.push(...await profileVariant(browser, prefix, count, variant, fixtureBodyBytes));
    }

    // Report only aggregates; never include recipe titles, file names, image paths or host URLs.
    const report = {
      scenario: "PERF-06-generated-variants",
      fixtureCount: count,
      formats: ["JPEG", "WebP", "PNG"],
      variants: ["original", "640", "320"],
      profiles,
      limitations:
        "Procedural food-like raster images, not photographs. Browser-based JPEG/WebP/PNG encoding, isolated PostgreSQL/storage, " +
        "single-host Chromium, device emulation and single LCP observation. Page 2 is a Blazor navigation; LCP applies only to page 1. " +
        "TransferSize includes headers and conditional revalidation; encodedBodySize is not repeat-visit network bytes. " +
        "No quality evaluation, hardware/mobile network p95 or evidence of a production thumbnail speedup.",
    };
    fs.mkdirSync(path.dirname(outputPath), { recursive: true });
    fs.writeFileSync(outputPath, JSON.stringify(report, null, 2) + "\n", { flag: "w" });
    console.log("PERF-06 generated profile: " + JSON.stringify({
      photos: count,
      profiles: profiles.map((item) => ({
        variant: item.variant,
        viewport: item.viewport,
        fixtureBodyBytes: item.fixtureBodyBytes,
        firstVisitBytes: item.firstVisit.transferredBytes,
        repeatVisitBytes: item.repeatVisit.transferredBytes,
        firstPageLcpMs: item.firstVisit.firstPageLcpMs,
      })),
    }));
    const originals = profiles.filter((item) => item.variant === "original");
    const smaller = profiles.filter((item) => item.variant !== "original");
    expect(originals).toHaveLength(2);
    expect(smaller).toHaveLength(4);
    for (const viewport of ["desktop", "mobile"]) {
      const original = originals.find((item) => item.viewport === viewport)!;
      for (const item of smaller.filter((profile) => profile.viewport === viewport)) {
        expect(item.fixtureBodyBytes, "Encoded variants must be smaller for these known generated fixtures")
          .toBeLessThan(original.fixtureBodyBytes);
      }
    }
  } finally {
    await generatorContext.close();
    // Archiving prevents pollution even if a profile assertion fails. The E2E
    // harness also removes the entire dedicated image directory on shutdown.
    const failures: string[] = [];
    for (const id of createdIds) {
      try {
        const response = await request.post(apiUrl + "/api/recipes/" + id + "/archive");
        if (!response.ok()) failures.push(id);
      } catch {
        failures.push(id);
      }
    }
    expect(failures, "Temporary benchmark recipes must archive successfully").toEqual([]);
  }
});

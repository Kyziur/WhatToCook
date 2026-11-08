import fs from "fs";
import path from "path";
import { expect, test, type BrowserContext, type Page } from "@playwright/test";
import { gotoAndWaitForApp, visibleLibrarySearch } from "./helpers";

const profileDirectory = path.join(__dirname, "..", "..", ".tmp", "playwright-e2e");
const profilePath = path.join(profileDirectory, "perf05-metrics.jsonl");
const phasePath = path.join(profileDirectory, "perf05-phase.txt");

type Snapshot = {
  Phase: string;
  ActiveCircuits: number;
  ConnectedCircuits: number;
  WorkingSetBytes: number;
  AllocatedBytes: number;
  Gen0Collections: number;
  Gen1Collections: number;
  Gen2Collections: number;
  RenderSamples: number;
  RenderP50Ms: number | null;
  RenderP95Ms: number | null;
  PublishedInstruments: string[];
};

function snapshots(phase: string): Snapshot[] {
  if (!fs.existsSync(profilePath)) return [];
  return fs.readFileSync(profilePath, "utf8")
    .split("\n")
    .filter((line) => line.trim().endsWith("}"))
    .flatMap((line) => {
      try {
        return [JSON.parse(line) as Snapshot];
      } catch {
        return [];
      }
    })
    .filter((sample) => sample.Phase === phase);
}

async function runSessionGroup(context: BrowserContext, phase: string, sessions: number) {
  fs.writeFileSync(phasePath, phase);

  await expect.poll(() => snapshots(phase).length, { timeout: 15_000 }).toBeGreaterThan(0);
  const pages: Page[] = [];
  const searchLatenciesMs: number[] = [];
  try {
    // Small launch waves avoid confusing browser startup with server-side render cost.
    for (let index = 0; index < sessions; index += 4) {
      const batch = await Promise.all(
        Array.from({ length: Math.min(4, sessions - index) }, async () => {
          const page = await context.newPage();
          pages.push(page);
          await gotoAndWaitForApp(page, "/");
          await expect(page.locator(".recipe-card").first()).toBeVisible();
          return page;
        }),
      );
      await Promise.all(batch.map(async (page) => {
        const search = await visibleLibrarySearch(page);
        const started = performance.now();
        await search.fill("zupa");
        await expect(page.getByRole("link", { name: "Zupa pomidorowa", exact: true })).toBeVisible();
        searchLatenciesMs.push(performance.now() - started);
      }));
    }

    // Verify genuine Blazor server circuits rather than just opened browser tabs.
    await expect.poll(() => {
      const values = snapshots(phase);
      return Math.max(0, ...values.map((value) => value.ConnectedCircuits));
    }, { timeout: 20_000 }).toBeGreaterThanOrEqual(sessions);

    // Missing native render samples are reported rather than invented.
    // Browser search-to-DOM latency remains measurable independently.

    // All readings are diagnostic; do not gate on arbitrary latency/memory thresholds.
    const observed = snapshots(phase);
    const peak = Math.max(...observed.map((sample) => sample.ConnectedCircuits));
    const final = observed.at(-1)!;
    const searchTimes = [...searchLatenciesMs].sort((a, b) => a - b);
    const percentile = (q: number) => searchTimes[Math.ceil(q * searchTimes.length) - 1];
    console.log("PERF-05 " + JSON.stringify({
      sessions,
      connectedPeak: peak,
      activeAtPeak: Math.max(...observed.map((sample) => sample.ActiveCircuits)),
      workingSetMB: +(final.WorkingSetBytes / (1024 * 1024)).toFixed(1),
      allocatedMB: +(final.AllocatedBytes / (1024 * 1024)).toFixed(1),
      gen0: final.Gen0Collections,
      gen1: final.Gen1Collections,
      gen2: final.Gen2Collections,
      renderCount: final.RenderSamples,
      renderP50Ms: final.RenderP50Ms,
      renderP95Ms: final.RenderP95Ms,
      searchToDomP50Ms: +percentile(0.5).toFixed(1),
      searchToDomP95Ms: +percentile(0.95).toFixed(1),
      availableInstruments: final.PublishedInstruments,
    }));
  } finally {
    await Promise.all(pages.map((page) => page.close()));
    fs.writeFileSync(phasePath, `after-${phase}`);
  }
}

test("@perf05 Blazor Interactive Server reports circuit and render metrics with 5 and 20 tabs", async ({
  browser,
}) => {
  test.skip(process.env.PERF05_PROFILE !== "1", "Run only through the explicit PERF-05 profile workflow");
  test.setTimeout(240_000);

  fs.mkdirSync(profileDirectory, { recursive: true });
  fs.writeFileSync(phasePath, "setup");
  const context = await browser.newContext({ viewport: { width: 1440, height: 900 } });
  try {
    await runSessionGroup(context, "five", 5);
    // Connected circuits must drain before checking the second cohort.
    await expect.poll(() => snapshots("after-five").at(-1)?.ConnectedCircuits ?? 1, {
      timeout: 20_000,
    }).toBe(0);
    await runSessionGroup(context, "twenty", 20);
  } finally {
    await context.close();
  }
});

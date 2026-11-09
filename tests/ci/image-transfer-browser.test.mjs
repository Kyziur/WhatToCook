import assert from "node:assert/strict";
import { execFile } from "node:child_process";
import { once } from "node:events";
import { createServer } from "node:http";
import { test } from "node:test";
import { promisify } from "node:util";
import { fileURLToPath } from "node:url";

const execFileAsync = promisify(execFile);
const cli = fileURLToPath(new URL("../../scripts/perf/measure-image-transfer.mjs", import.meta.url));
const photo = `<svg xmlns="http://www.w3.org/2000/svg" width="800" height="512"><rect width="800" height="512" fill="#ddd"/></svg>`;

async function withLocalLibrary({ includePhoto }, check) {
  const hits = { first: 0, conditional: 0 };
  const server = createServer((request, response) => {
    if (request.url === "/recipe-images/fixture.svg") {
      const headers = { "Cache-Control": "private, no-cache", ETag: '"fixture-v1"' };
      if (request.headers["if-none-match"] === '"fixture-v1"') {
        hits.conditional++;
        response.writeHead(304, headers);
        response.end();
      } else {
        hits.first++;
        response.writeHead(200, {
          ...headers,
          "Content-Type": "image/svg+xml",
          "Content-Length": Buffer.byteLength(photo),
        });
        response.end(photo);
      }
      return;
    }
    if (request.url === "/") {
      response.writeHead(200, { "Content-Type": "text/html; charset=utf-8" });
      response.end(`<!doctype html><html><head><meta name="viewport" content="width=device-width,initial-scale=1"></head><body>
        <article class="recipe-card">
        ${includePhoto ? '<img src="/recipe-images/fixture.svg" width="100" height="64" alt="fixture">' : "No recipe photo"}
        </article></body></html>`);
      return;
    }
    response.writeHead(404);
    response.end();
  });

  server.listen(0, "127.0.0.1");
  await once(server, "listening");
  try {
    const address = server.address();
    assert.ok(address && typeof address !== "string");
    await check(`http://127.0.0.1:${address.port}/`, hits);
  } finally {
    await new Promise((resolve, reject) =>
      server.close((error) => (error ? reject(error) : resolve()))
    );
  }
}

test("PERF-06 probe captures first and conditional repeat visits in real Chromium", async () => {
  await withLocalLibrary({ includePhoto: true }, async (url, hits) => {
    const { stdout } = await execFileAsync(
      process.execPath,
      [cli, "--url", url, "--settle-ms", "100", "--load-timeout-ms", "3000"],
      { timeout: 30000 }
    );
    const report = JSON.parse(stdout);
    assert.equal(report.probe, "PERF-06");
    assert.equal(report.mode, "desktop-viewport");
    assert.equal(report.firstVisit.resourceCount, 1);
    assert.ok(report.firstVisit.transferBytesIncludingHeaders > 0);
    assert.equal(report.firstVisit.visibleLoadedImageCount, 1);
    assert.ok(report.firstVisit.largestDimensionRatio > 2);
    assert.equal(report.repeatVisit.resourceCount, 1);
    assert.equal(report.repeatVisit.visibleLoadedImageCount, 1);
    assert.ok(hits.first >= 1, "first request must download the fixture");
    assert.ok(hits.conditional >= 1, "repeat request must send a cache validator");
    assert.ok(!stdout.includes("fixture.svg"), "report must not leak image file paths");
    assert.ok(!stdout.includes("127.0.0.1"), "report must not leak the probed host");
  });
});

test("PERF-06 probe fails instead of reporting zero transfer for a library without photos", async () => {
  await withLocalLibrary({ includePhoto: false }, async (url) => {
    await assert.rejects(
      () =>
        execFileAsync(
          process.execPath,
          [cli, "--url", url, "--settle-ms", "0", "--load-timeout-ms", "300"],
          { timeout: 30000 }
        ),
      (error) => {
        assert.equal(error.code, 1);
        assert.equal(error.stdout, "");
        assert.match(error.stderr, /PERF-06 probe failed/);
        return true;
      }
    );
  });
});

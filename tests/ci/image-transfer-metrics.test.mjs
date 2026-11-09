import assert from "node:assert/strict";
import { test } from "node:test";
import {
  parseProbeArgs,
  summarizeImageCapture,
} from "../../scripts/perf/image-transfer-metrics.mjs";

test("probe arguments require a valid HTTP(S) library URL and bound the settle delay", () => {
  assert.equal(parseProbeArgs(["--url", "http://localhost:5180/", "--mobile"]).mobile, true);
  assert.equal(parseProbeArgs(["--url", "https://example.test/", "--settle-ms", "0"]).settleMs, 0);
  assert.equal(parseProbeArgs(["--url", "https://example.test/"]).loadTimeoutMs, 15000);
  assert.equal(parseProbeArgs(["--url", "https://example.test/", "--load-timeout-ms", "100"]).loadTimeoutMs, 100);
  for (const args of [
    [],
    ["--url", "file:///etc/passwd"],
    ["--url", "https://name:secret@example.test/"],
    ["--url", "http://localhost/", "--settle-ms", "-1"],
    ["--url", "http://localhost/", "--settle-ms", "30001"],
    ["--url", "http://localhost/", "--load-timeout-ms", "99"],
    ["--url", "http://localhost/", "--load-timeout-ms", "60001"],
    ["--url", "http://localhost/", "--load-timeout-ms", "1.5"],
    ["--url", "http://localhost/", "--output"],
    ["--url", "http://localhost/", "--unknown"],
  ]) {
    assert.throws(() => parseProbeArgs(args));
  }
});

test("metrics ignore unrelated resources and distinguish transferred vs cached entries", () => {
  const result = summarizeImageCapture({
    dpr: 2,
    lcpMs: 440,
    resources: [
      {
        name: "https://example.test/recipe-images/one.jpg",
        transferSize: 4200,
        encodedBodySize: 4000,
      },
      {
        name: "https://example.test/recipe-images/two.jpg",
        transferSize: 0,
        encodedBodySize: 1000,
      },
      { name: "https://example.test/_framework/app.js", transferSize: 50000 },
    ],
    images: [
      { source: "/recipe-images/one.jpg", naturalWidth: 1600, displayedWidth: 200, visible: true, loaded: true },
      { source: "/recipe-images/two.jpg", naturalWidth: 400, displayedWidth: 200, visible: true, loaded: true },
      { source: "/recipe-images/three.jpg", naturalWidth: 3000, displayedWidth: 100, visible: false, loaded: true },
    ],
  });
  assert.deepEqual(result, {
    resourceCount: 2,
    transferBytesIncludingHeaders: 4200,
    encodedBodyBytesReported: 5000,
    zeroTransferEntries: 1,
    visibleLoadedImageCount: 2,
    overResolutionCount: 1,
    largestDimensionRatio: 4,
    lcpMs: 440,
  });
});

test("no photo observation reports unavailable ratio without fabricating values", () => {
  const result = summarizeImageCapture({
    dpr: 1,
    lcpMs: null,
    resources: [],
    images: [],
  });
  assert.equal(result.resourceCount, 0);
  assert.equal(result.largestDimensionRatio, null);
  assert.equal(result.lcpMs, null);
});

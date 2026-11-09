const imagePathPrefix = "/recipe-images/";

export function parseProbeArgs(args) {
  const options = { url: null, mobile: false, settleMs: 2000, output: null };
  for (let index = 0; index < args.length; index++) {
    const arg = args[index];
    if (arg === "--mobile") {
      options.mobile = true;
    } else if (["--url", "--settle-ms", "--output"].includes(arg)) {
      const value = args[++index];
      if (!value || value.startsWith("--")) {
        throw new Error(`Missing value for ${arg}`);
      }
      if (arg === "--url") options.url = value;
      if (arg === "--output") options.output = value;
      if (arg === "--settle-ms") {
        if (!/^\d+$/.test(value)) throw new Error("--settle-ms must be an integer");
        options.settleMs = Number(value);
      }
    } else {
      throw new Error(`Unknown argument: ${arg}`);
    }
  }

  if (!options.url) throw new Error("--url is required (recipe library page)");
  const url = new URL(options.url);
  if (!["http:", "https:"].includes(url.protocol) || url.username || url.password) {
    throw new Error("--url must be an HTTP(S) URL without embedded credentials");
  }
  if (!Number.isSafeInteger(options.settleMs) || options.settleMs > 30000) {
    throw new Error("--settle-ms must be between 0 and 30000");
  }
  options.url = url.href;
  return options;
}

export function summarizeImageCapture(capture) {
  const resources = capture.resources.filter((resource) =>
    new URL(resource.name, "https://invalid.local").pathname.startsWith(imagePathPrefix)
  );
  const total = (property) =>
    resources.reduce((sum, item) => sum + (Number(item[property]) || 0), 0);
  const visible = capture.images.filter((image) => image.visible && image.loaded);
  const oversized = visible.filter(
    (image) => image.naturalWidth > image.displayedWidth * capture.dpr * 2
  );

  return {
    resourceCount: resources.length,
    transferBytesIncludingHeaders: total("transferSize"),
    encodedBodyBytesReported: total("encodedBodySize"),
    zeroTransferEntries: resources.filter((resource) => resource.transferSize === 0).length,
    visibleLoadedImageCount: visible.length,
    overResolutionCount: oversized.length,
    largestDimensionRatio:
      visible.length === 0
        ? null
        : Math.round(
            Math.max(
              ...visible.map(
                (image) => image.naturalWidth / Math.max(1, image.displayedWidth * capture.dpr)
              )
            ) * 100
          ) / 100,
    lcpMs: capture.lcpMs,
  };
}

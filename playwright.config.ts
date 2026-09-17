import { defineConfig, devices } from "@playwright/test";
import fs from "fs";
import path from "path";

const reuseExistingServer = false;
const repoRoot = __dirname;
const e2eTempDir = path.join(repoRoot, ".tmp", "playwright-e2e");
fs.mkdirSync(e2eTempDir, { recursive: true });

export default defineConfig({
  testDir: "./tests/e2e",
  timeout: 60_000,
  expect: {
    timeout: 10_000,
  },
  fullyParallel: false,
  workers: 1,
  reporter: [
    ["list"],
    ["html", { open: "never", outputFolder: "tests/e2e/artifacts/playwright-report" }],
  ],
  outputDir: "tests/e2e/artifacts/test-results",
  use: {
    baseURL: "http://127.0.0.1:5180",
    trace: "retain-on-failure",
    screenshot: "only-on-failure",
    video: "retain-on-failure",
  },
  webServer: [
    {
      command: "node tests/e2e/scripts/run-api-with-postgres.cjs",
      url: "http://127.0.0.1:5181/health",
      timeout: 120_000,
      reuseExistingServer,
      env: {
        ASPNETCORE_ENVIRONMENT: "Development",
        ASPNETCORE_DETAILEDERRORS: "true",
        "SeedData__Enabled": "true",
      },
    },
    {
      command: "dotnet run --project src/WhatToCook.Web --no-build --urls http://127.0.0.1:5180",
      url: "http://127.0.0.1:5180",
      timeout: 120_000,
      reuseExistingServer,
      env: {
        ASPNETCORE_ENVIRONMENT: "Development",
        ASPNETCORE_DETAILEDERRORS: "true",
        "Api__BaseUrl": "http://127.0.0.1:5181",
      },
    },
  ],
  projects: [
    {
      name: "desktop-chromium",
      use: { ...devices["Desktop Chrome"], viewport: { width: 1440, height: 900 } },
    },
    {
      name: "mobile-chromium",
      use: { ...devices["Pixel 7"] },
    },
  ],
});

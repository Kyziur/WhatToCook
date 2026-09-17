import { expect, test } from "@playwright/test";
import fs from "fs";
import path from "path";
import { gotoAndWaitForApp } from "./helpers";

test.describe("OpenSpec: self-hosted-web-platform", () => {
  test("Open application in browser and navigate recipe experience", async ({ page }) => {
    await gotoAndWaitForApp(page, "/");

    await expect(page.getByRole("heading", { name: "Biblioteka przepisów" })).toBeVisible();
    await page.getByRole("link", { name: "Planer" }).click();
    await expect(page).toHaveURL(/\/planning/);
    await expect(page.getByRole("heading", { name: "Plan posiłków" })).toBeVisible();
  });

  test("Open application from another route and keep responsive shell navigation", async ({ page }) => {
    await gotoAndWaitForApp(page, "/shopping");

    await expect(page.getByRole("heading", { name: "Lista zakupów" })).toBeVisible();
    await expect(page.getByRole("navigation")).toContainText("Biblioteka");
    await expect(page.getByRole("navigation")).toContainText("Planer");
    await expect(page.getByRole("navigation")).toContainText("Zakupy");
  });

  test("Container deployment assets exist for Linux self-hosted startup", async () => {
    const composePath = path.resolve(__dirname, "../../deploy/docker-compose.yml");
    const envSamplePath = path.resolve(__dirname, "../../deploy/.env.example");

    await expect(fs.existsSync(composePath)).toBeTruthy();
    await expect(fs.existsSync(envSamplePath)).toBeTruthy();
  });

  test("Self-hosted deployment documentation exists", async () => {
    const docsPath = path.resolve(__dirname, "../../docs/self-hosted-deployment.md");
    await expect(fs.existsSync(docsPath)).toBeTruthy();
  });
});

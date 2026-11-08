import { expect, test } from "@playwright/test";
import { gotoAndWaitForApp } from "./helpers";

test.describe("UX-02: focused keyboard accessibility smoke", () => {
  test("Mobile library filters can be operated with Enter and Space", async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await gotoAndWaitForApp(page, "/");

    const toggle = page.locator("#library-filter-toggle");
    await expect(toggle).toBeVisible();
    await expect(toggle).toHaveAttribute("aria-controls", "library-filter-panel");
    await expect(toggle).toHaveAttribute("aria-expanded", "false");

    await toggle.focus();
    await expect(toggle).toBeFocused();
    await page.keyboard.press("Enter");
    await expect(toggle).toHaveAttribute("aria-expanded", "true");

    await toggle.focus();
    await page.keyboard.press("Space");
    await expect(toggle).toHaveAttribute("aria-expanded", "false");
  });

  test("Recipe editor title and servings are sequential keyboard fields", async ({ page }) => {
    await gotoAndWaitForApp(page, "/recipes/new");

    const title = page.getByRole("textbox", { name: "Tytuł przepisu" });
    const servings = page.locator("#recipe-servings");
    await expect(title).toBeVisible();
    await expect(servings).toBeVisible();

    await title.focus();
    await expect(title).toBeFocused();
    await page.keyboard.press("Tab");
    await expect(servings).toBeFocused();
  });
});

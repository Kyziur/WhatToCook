import { expect, type Page, test } from "@playwright/test";
import { attachPageSnapshot, gotoAndWaitForApp, uniqueSuffix } from "./helpers";

test.describe("OpenSpec: recipe-app-shell", () => {
  test("Enter the application after launch -> lands in recipe library", async ({ page }, testInfo) => {
    await gotoAndWaitForApp(page, "/");

    await expect(page.getByRole("heading", { name: "Biblioteka przepisów" })).toBeVisible();
    await expect(page.getByRole("navigation")).toContainText("Biblioteka");
    await expect(page.getByRole("navigation")).toContainText("Planer");
    await expect(page.getByRole("navigation")).toContainText("Zakupy");

    await attachPageSnapshot(page, testInfo, "library-landing");
  });

  test("Core shell fits phone, tablet and desktop viewports", async ({ page }) => {
    const viewports = [
      { width: 375, height: 812 },
      { width: 768, height: 1024 },
      { width: 1440, height: 900 },
    ];

    for (const viewport of viewports) {
      await page.setViewportSize(viewport);
      const routes = viewport.width === 375 ? ["/", "/recipes/new", "/planning", "/shopping"] : ["/"];

      for (const route of routes) {
        await gotoAndWaitForApp(page, route);
        const dimensions = await page.evaluate(() => ({
          clientWidth: document.documentElement.clientWidth,
          scrollWidth: document.documentElement.scrollWidth,
        }));
        expect(dimensions.scrollWidth, `${route} at ${viewport.width}px`).toBeLessThanOrEqual(
          dimensions.clientWidth,
        );
      }

      await gotoAndWaitForApp(page, "/");
      if (viewport.width === 375) {
        await expect(page.locator(".shell-mobile-actions")).toBeVisible();
        await expect(page.locator(".shell-desktop-actions")).toBeHidden();
      } else {
        await expect(page.locator(".shell-mobile-actions")).toBeHidden();
        await expect(page.locator(".shell-desktop-actions")).toBeVisible();
      }
    }

    await page.setViewportSize({ width: 375, height: 812 });
    await gotoAndWaitForApp(page, "/");
    const undersizedShellTargets = await page
      .locator(".shell-header a:visible, .shell-header button:visible, .shell-footer a:visible")
      .evaluateAll((elements) =>
        elements
          .map((element) => ({
            label: element.getAttribute("aria-label") ?? element.textContent?.trim() ?? "",
            height: element.getBoundingClientRect().height,
          }))
          .filter((target) => target.height < 44),
      );
    expect(undersizedShellTargets).toEqual([]);
  });

  test("Long library keeps desktop navigation and phone search reachable", async ({ page }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await gotoAndWaitForApp(page, "/");
    await expect(page.locator(".recipe-card").first()).toBeVisible();
    await page.evaluate(() => window.scrollTo(0, document.documentElement.scrollHeight));
    await expect(page.locator(".shell-desktop-nav")).toBeVisible();
    await expect(page.locator(".shell-desktop-nav").getByRole("link", { name: "Planer" })).toBeVisible();
    await expect(page.locator(".shell-footer")).toBeHidden();

    await page.setViewportSize({ width: 375, height: 812 });
    await gotoAndWaitForApp(page, "/");
    await expect(page.locator(".recipe-card").first()).toBeVisible();
    await page.evaluate(() => window.scrollTo(0, document.documentElement.scrollHeight));
    await expect.poll(() => page.evaluate(() => window.scrollY)).toBeGreaterThan(0);
    await expect(page.locator("#library-search")).toBeVisible();
  });

  test("Start creating a recipe from the main shell", async ({ page }) => {
    await gotoAndWaitForApp(page, "/");
    const mobileActions = page.locator(".shell-mobile-actions");
    if (await mobileActions.isVisible()) {
      await mobileActions.getByRole("button", { name: "Dodaj lub importuj przepis" }).click();
      await mobileActions.getByRole("link", { name: "Dodaj przepis" }).click();
    } else {
      await page.getByRole("link", { name: "Dodaj przepis" }).first().click();
    }
    await expect(page).toHaveURL(/\/recipes\/new/);
    await expect(page.getByRole("heading", { name: "Edytor przepisu" })).toBeVisible();
  });

  test("Attempt to leave editor with unsaved changes shows warning", async ({ page }) => {
    await gotoAndWaitForApp(page, "/recipes/new");
    await page.locator("#recipe-title").fill("Niezapisany test");

    await page.getByRole("navigation").getByRole("link", { name: "Planer" }).click();
    await expect(page).toHaveURL(/\/recipes\/new/);
  });

  test("Open planner from shell", async ({ page }, testInfo) => {
    await gotoAndWaitForApp(page, "/");
    await page.getByRole("navigation").getByRole("link", { name: "Planer" }).click();
    await expect(page).toHaveURL(/\/planning/);
    await expect(page.getByRole("heading", { name: "Plan posiłków" })).toBeVisible();
    await attachPageSnapshot(page, testInfo, "planner-screen");
  });

  test("Open planner and expose a collapsible create path", async ({ page }) => {
    await gotoAndWaitForApp(page, "/planning");
    await expect(page.getByRole("heading", { name: "Plan posiłków" })).toBeVisible();
    await ensureCreatePlanForm(page);
    await expect(page.locator("#new-date-from")).toBeVisible();
    await expect(page.getByRole("button", { name: "Utwórz plan" })).toBeVisible();
  });

  test("Save changed plan with existing shopping list requires regeneration decision", async ({ page }) => {
    const planName = `Shell planner test ${uniqueSuffix()}`;
    await gotoAndWaitForApp(page, "/planning");
    await expect(page.getByText("Ładowanie planu posiłków")).toHaveCount(0);
    await ensureCreatePlanForm(page);
    await page.locator("#new-date-from").fill("2026-05-20");
    await page.locator("#new-date-to").fill("2026-05-26");
    await page.locator("#new-plan-name").fill(planName);
    await page.getByRole("button", { name: "Utwórz plan" }).click();

    await expect(page.locator("#plan-selector option:checked")).toHaveText(planName);
    await expect(page.locator("#plan-name")).toHaveCount(0);
    await openDinnerSlot(page, "2026-05-20");
    await page
      .locator(".planner-recipe-drawer")
      .getByRole("button", { name: /Grecka sałatka/ })
      .click();
    await page.getByRole("button", { name: "Zapisz plan" }).click();
    await expect(page.locator("#plan-selector option:checked")).toHaveText(planName);

    await page.locator("button:visible").filter({ hasText: /^Edytuj/ }).first().click();
    await expect(page.locator("#plan-name")).toHaveValue(planName);
    const renamedPlan = `${planName} v2`;
    await page.locator("#plan-name").fill(renamedPlan);
    await page.getByRole("button", { name: "Zapisz plan" }).click();
    await expect(page.getByRole("heading", { name: "Lista zakupów wymaga regeneracji" })).toBeVisible();
    await page.getByRole("button", { name: "Potwierdź regenerację" }).click();

    await expect(page.locator("#plan-selector option:checked")).toHaveText(renamedPlan);
    await openDinnerSlot(page, "2026-05-21");
    await page.locator(".planner-recipe-drawer").getByPlaceholder("Szukaj w przepisach...").fill("Zupa pomidorowa");
    await page.locator(".planner-recipe-drawer").getByRole("button", { name: /Zupa pomidorowa/ }).click();
    await page.getByRole("button", { name: "Zapisz plan" }).click();
    await expect(page.getByRole("heading", { name: "Lista zakupów wymaga regeneracji" })).toBeVisible();
    await page.getByRole("button", { name: "Potwierdź regenerację" }).click();

    const shoppingLink = page.getByRole("link", { name: "Otwórz", exact: true });
    if (await shoppingLink.isVisible()) {
      await shoppingLink.click();
    } else {
      const planId = await page.locator("#plan-selector option:checked").getAttribute("value");
      if (!planId) throw new Error("Selected planner option has no plan id.");
      await page.goto(`/shopping?planId=${planId}`);
    }
    await expect(page).toHaveURL(/\/shopping\?planId=/);
    await expect(page.getByText("bulion")).toBeVisible();

    await gotoAndWaitForApp(page, "/planning");
    await page.locator("#plan-selector").selectOption({ label: renamedPlan });
    await expect(page.locator("#plan-selector option:checked")).toHaveText(renamedPlan);
    const finalShoppingLink = page.getByRole("link", { name: "Otwórz", exact: true });
    if (await finalShoppingLink.isVisible()) {
      await finalShoppingLink.click();
    } else {
      const finalPlanId = await page.locator("#plan-selector option:checked").getAttribute("value");
      if (!finalPlanId) throw new Error("Selected planner option has no plan id.");
      await page.goto(`/shopping?planId=${finalPlanId}`);
    }
    await expect(page).toHaveURL(/\/shopping\?planId=/);
    await expect(page.getByText("bulion")).toBeVisible();
  });
});

async function openDinnerSlot(page: Page, date: string): Promise<void> {
  const desktopSlot = page.locator(`.planner-slot-cell[data-day='${date}'][data-slot='Dinner']:visible`);
  if ((await desktopSlot.count()) > 0) {
    await desktopSlot.getByRole("button", { name: /Dodaj/ }).click();
  } else {
    await page.locator(`.planner-mobile-day-picker button[data-day-target='${date}']`).click();
    await page
      .locator(".planner-mobile-slot")
      .filter({ hasText: "Obiad" })
      .getByRole("button", { name: /Dodaj/ })
      .click();
  }
  await expect(page.locator(".planner-recipe-drawer")).toBeVisible();
}
async function ensureCreatePlanForm(page: Page): Promise<void> {
  const form = page.locator("#new-date-from");
  const newPlanButton = page.locator("button:visible").filter({ hasText: /^Nowy plan$/ });
  await expect(form.or(newPlanButton).first()).toBeVisible();
  if (!(await form.isVisible())) {
    await expect(newPlanButton).toHaveCount(1);
    await newPlanButton.click();
  }
  await expect(form).toBeVisible();
}

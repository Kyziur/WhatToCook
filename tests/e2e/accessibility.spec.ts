import AxeBuilder from "@axe-core/playwright";
import { expect, type Locator, type Page, test } from "@playwright/test";
import {
  createPlanWithRecipe,
  createRecipe,
  gotoAndWaitForApp,
  uniqueSuffix,
} from "./helpers";

const criticalRoutes = [
  { name: "library", path: "/", heading: "Biblioteka przepisów" },
  { name: "import", path: "/recipes/import", heading: "Import przepisu z URL" },
];

for (const route of criticalRoutes) {
  test(`@a11y ${route.name}: critical content has no detectable WCAG 2.2 A/AA violations`, async ({
    page,
  }) => {
    await gotoAndWaitForApp(page, route.path);
    await expect(page.getByRole("heading", { name: route.heading, exact: true })).toBeVisible();
    if (route.name === "library") {
      await expect(page.locator("#library-search")).toHaveAttribute("data-interactive", "true");
    } else {
      await expect(
        page.getByRole("button", { name: "Utwórz szkic importu" }),
      ).toHaveAttribute("data-interactive", "true");
    }
    await expect(page.locator("main [aria-busy='true']")).toHaveCount(0);
    await expectAxeAuditToBeClean(page);
  });
}

type ShoppingFixture = { planId: string; planName: string };

const shoppingAccessibilityTest = test.extend<{ shoppingFixture: ShoppingFixture }>({
  shoppingFixture: async ({ page }, use, testInfo) => {
    const suffix = uniqueSuffix();
    const recipeTitle = `UX-02 axe przepis ${suffix}`;
    const planName = `UX-02 axe plan ${testInfo.project.name} ${suffix}`;
    await createRecipe(page, recipeTitle, "Pomidor", "2", "szt");
    const planId = await createPlanWithRecipe(
      page,
      planName,
      recipeTitle,
      "2027-01-04",
    );
    await use({ planId, planName });
    // The Playwright server fixture removes this run-scoped PostgreSQL container at suite shutdown.
  },
});

shoppingAccessibilityTest(
  "@a11y shopping: generated plan content has no detectable WCAG 2.2 A/AA violations",
  async ({ page, shoppingFixture }) => {
    await gotoAndWaitForApp(page, `/shopping?planId=${shoppingFixture.planId}`);
    await expect(page.getByRole("heading", { name: "Lista zakupów", exact: true })).toBeVisible();
    await expect(
      page.getByRole("heading", { name: shoppingFixture.planName, exact: true }),
    ).toBeVisible();
    await expect(page.getByRole("heading", { name: "Brak planów posiłków" })).toHaveCount(0);
    await expect(page.locator(".shopping-workspace")).toBeVisible();
    await expect(
      page.locator(".shopping-plan-strip button[aria-pressed='true']"),
    ).toBeEnabled();
    const fixtureItem = page.locator(".shopping-item").filter({ hasText: "Pomidor" }).first();
    await expect(fixtureItem).toBeVisible();
    const startingPressed = await fixtureItem.getAttribute("aria-pressed");
    if (startingPressed !== "true" && startingPressed !== "false") {
      throw new Error("Generated shopping item has no aria-pressed state.");
    }
    const toggledPressed = startingPressed === "true" ? "false" : "true";
    await fixtureItem.click();
    await expect(fixtureItem).toHaveAttribute("aria-pressed", toggledPressed);
    await expect(fixtureItem).toHaveAttribute("aria-disabled", "false");
    await fixtureItem.click();
    await expect(fixtureItem).toHaveAttribute("aria-pressed", startingPressed);
    await expect(fixtureItem).toHaveAttribute("aria-disabled", "false");
    await expect(page.locator("main [aria-busy='true']")).toHaveCount(0);
    await expectAxeAuditToBeClean(page);
  },
);

test("@a11y library: search is keyboard-reachable and labelled", async ({ page }) => {
  await gotoAndWaitForApp(page, "/");
  const search = page.locator("#library-search:visible, #library-sidebar-search:visible");
  await expect(search).toBeVisible();
  await expect(search).toHaveAttribute("data-interactive", "true");
  await expect(search).toHaveAccessibleName("Szukaj po tytule");
  const heading = page.getByRole("heading", { name: "Biblioteka przepisów", exact: true });
  await tabToControl(page, heading, search);
  await page.keyboard.type("pomidor", { delay: 120 });
  await expect(search).toHaveValue("pomidor");
});

test("@a11y import: URL form is usable without pointer", async ({ page }) => {
  await gotoAndWaitForApp(page, "/recipes/import");
  const url = page.getByRole("textbox", { name: "Adres przepisu" });
  await expect(url).toBeVisible();
  const submit = page.getByRole("button", { name: "Utwórz szkic importu" });
  await expect(submit).toHaveAttribute("data-interactive", "true");
  const heading = page.getByRole("heading", { name: "Import przepisu z URL", exact: true });
  await tabToControl(page, heading, url);
  await page.keyboard.type("https://example.com/recipe");
  await expect(url).toHaveValue("https://example.com/recipe");
  await expect(submit).toBeEnabled();
  await page.keyboard.press("Tab");
  await expect(submit).toBeFocused();
});

async function expectAxeAuditToBeClean(page: Page): Promise<void> {
  const scan = await new AxeBuilder({ page })
    .withTags(["wcag2a", "wcag2aa", "wcag21a", "wcag21aa", "wcag22aa"])
    .analyze();
  expect(
    scan.violations.map(({ id, impact, nodes }) => ({
      id,
      impact,
      targets: nodes.map((node) => node.target),
    })),
  ).toEqual([]);
}

async function tabToControl(page: Page, start: Locator, control: Locator): Promise<void> {
  await expect(start).toBeFocused();
  for (let tabCount = 0; tabCount < 40; tabCount += 1) {
    await page.keyboard.press("Tab");
    if (await control.evaluate((element) => element === document.activeElement)) {
      return;
    }
  }
  throw new Error("Keyboard Tab navigation did not reach the requested control.");
}

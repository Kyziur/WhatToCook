import AxeBuilder from "@axe-core/playwright";
import { expect, test } from "@playwright/test";

const criticalRoutes = [
  { name: "library", path: "/", heading: "Biblioteka przepisów" },
  { name: "import", path: "/recipes/import", heading: "Import przepisu z URL" },
  { name: "shopping", path: "/shopping", heading: "Lista zakupów" },
];

for (const route of criticalRoutes) {
  test(`@a11y ${route.name}: critical content has no detectable WCAG 2.2 A/AA violations`, async ({
    page,
  }) => {
    await page.goto(route.path);
    await expect(page.getByRole("heading", { name: route.heading, exact: true })).toBeVisible();
    await expect(page.locator("main [aria-busy='true']")).toHaveCount(0);
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
  });
}

test("@a11y library: search is keyboard-reachable and labelled", async ({ page }) => {
  await page.goto("/");
  const search = page.getByRole("textbox", { name: "Szukaj po tytule" }).first();
  await expect(search).toBeVisible();
  await search.focus();
  await expect(search).toBeFocused();
  await search.pressSequentially("pomidor", { delay: 120 });
  await expect(search).toHaveValue("pomidor");
});

test("@a11y import: URL form is usable without pointer", async ({ page }) => {
  await page.goto("/recipes/import");
  const url = page.getByRole("textbox", { name: "Adres przepisu" });
  await expect(url).toBeVisible();
  await url.focus();
  await page.keyboard.type("https://example.com/recipe");
  await expect(url).toHaveValue("https://example.com/recipe");
  const submit = page.getByRole("button", { name: "Utwórz szkic importu" });
  await expect(submit).toBeEnabled();
  await url.press("Tab");
  await expect(submit).toBeFocused();
});

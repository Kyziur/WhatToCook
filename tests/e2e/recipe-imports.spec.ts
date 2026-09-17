import { expect, test } from "@playwright/test";
import { gotoAndWaitForApp, uniqueSuffix } from "./helpers";

test.describe("OpenSpec: recipe-imports", () => {
  test("Create URL import draft and finalize into recipe", async ({ page }) => {
    const title = `Import URL ${uniqueSuffix()}`;

    await gotoAndWaitForApp(page, "/recipes/import");
    const createDraftButton = page.getByRole("button", { name: "Utwórz szkic importu" });
    await expect(createDraftButton).toHaveAttribute("data-interactive", "true");

    const fixtureUrl = new URL(
      `/import-fixtures/recipe-import/basic?title=${encodeURIComponent(title)}&servings=4%20porcje`,
      page.url()
    ).toString();

    await page.locator("#recipe-import-url").fill(fixtureUrl);
    await expect(page.locator("#recipe-import-url")).toHaveValue(fixtureUrl);
    await expect(createDraftButton).toBeEnabled();
    await page.locator("#recipe-import-url").press("Enter");

    await expect(page).toHaveURL(/\/recipes\/imports\/[0-9a-f-]+/);
    await expect(page.locator("#import-title")).toHaveValue(title);
    await expect(page.locator("#import-servings")).toHaveValue("4");
    await expect(page.locator("select.ingredient-unit").first()).toBeVisible();

    await page.getByRole("button", { name: "Finalizuj przepis" }).click();

    await expect(page).toHaveURL(/\/recipes\/[0-9a-f-]+/);
    await expect(page.getByRole("heading", { name: title })).toBeVisible();
    await expect(page.locator("ul li").first()).toContainText("makaron");
  });

  test("Correct blocked import draft before finalization", async ({ page }) => {
    const title = `Import blocked ${uniqueSuffix()}`;

    await gotoAndWaitForApp(page, "/recipes/import");
    const createDraftButton = page.getByRole("button", { name: "Utwórz szkic importu" });
    await expect(createDraftButton).toHaveAttribute("data-interactive", "true");

    const fixtureUrl = new URL(
      `/import-fixtures/recipe-import/missing-servings?title=${encodeURIComponent(title)}`,
      page.url()
    ).toString();

    await page.locator("#recipe-import-url").fill(fixtureUrl);
    await expect(page.locator("#recipe-import-url")).toHaveValue(fixtureUrl);
    await expect(createDraftButton).toBeEnabled();
    await createDraftButton.click();

    await expect(page).toHaveURL(/\/recipes\/imports\/[0-9a-f-]+/);
    await expect(page.getByText(/Liczba porcji/i)).toBeVisible();
    await expect(page.locator("select.ingredient-unit").first()).toBeVisible();

    await expect(page.locator("textarea")).toHaveCount(2);
    await page.getByRole("button", { name: "Złącz z krokiem wyżej" }).click();
    await expect(page.locator("textarea")).toHaveCount(1);

    await page.locator("#import-servings").fill("6");
    await page.getByRole("button", { name: "Zapisz szkic" }).click();
    await expect(page.getByText("Szkic importu zostal zapisany.")).toBeVisible();

    await page.getByRole("button", { name: "Finalizuj przepis" }).click();

    await expect(page).toHaveURL(/\/recipes\/[0-9a-f-]+/);
    await expect(page.getByRole("heading", { name: title })).toBeVisible();
    await page.getByRole("link", { name: "Edytuj", exact: true }).click();
    await expect(page.locator("#recipe-servings")).toHaveValue("6");
  });
});

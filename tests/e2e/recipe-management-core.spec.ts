import { expect, test } from "@playwright/test";
import {
  attachPageSnapshot,
  clickChip,
  fillRecipeEditorWithMinimumValidData,
  gotoAndWaitForApp,
  openDetailsCard,
  uniqueSuffix,
  visibleLibrarySearch,
} from "./helpers";

test.describe("OpenSpec: recipe-management-core", () => {
  test("Browse recipes alphabetically on library screen", async ({ page }) => {
    await gotoAndWaitForApp(page, "/");
    const titles = await page.locator(".recipe-title-link").allTextContents();
    const normalized = titles.map((x) => x.trim()).filter(Boolean);
    const sorted = [...normalized].sort((a, b) => a.localeCompare(b));
    expect(normalized).toEqual(sorted);
  });

  test("Phone library leads with search and keeps advanced filters disclosed on demand", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await gotoAndWaitForApp(page, "/");

    await expect(page.locator("#library-search")).toBeVisible();
    await expect(page.locator("#library-filter-toggle")).toHaveAttribute("aria-expanded", "false");
    await expect(page.locator("#library-filter-panel")).toBeHidden();
    await expect(page.locator(".recipe-card").first()).toBeVisible();
    const initialRecipeCount = await page.locator(".recipe-card").count();

    const positions = await page.evaluate(() => ({
      searchTop: document.querySelector("#library-search")?.getBoundingClientRect().top ?? Infinity,
      firstResultTop: document.querySelector(".recipe-card")?.getBoundingClientRect().top ?? Infinity,
    }));
    expect(positions.searchTop).toBeLessThan(positions.firstResultTop);
    expect(positions.firstResultTop).toBeLessThan(812);

    await page.locator("#library-filter-toggle").click();
    await expect(page.locator("#library-filter-panel")).toBeVisible();
    await clickChip(page, "szybkie");
    await expect(page.locator(".selected-filter-summary")).toContainText("szybkie");
    await expect.poll(() => page.locator(".recipe-card").count()).toBeLessThan(initialRecipeCount);
  });

  test("Search by title, tag and all selected ingredients", async ({ page }) => {
    await gotoAndWaitForApp(page, "/");

    await (await visibleLibrarySearch(page)).fill("zupa");
    await expect(page.locator(".library-results-toolbar h2")).toHaveText("1 przepis");
    await expect(page.getByRole("link", { name: "Zupa pomidorowa", exact: true })).toBeVisible();
    await expect(page.getByRole("link", { name: "Grecka sałatka", exact: true })).toHaveCount(0);

    await (await visibleLibrarySearch(page)).fill("");
    await clickChip(page, "szybkie");
    await expect(page.getByRole("link", { name: "Grecka sałatka", exact: true })).toBeVisible();
    await expect(page.getByRole("link", { name: "Zupa pomidorowa", exact: true })).toHaveCount(0);

    await clickChip(page, "szybkie");
    await clickChip(page, "pomidor");
    await clickChip(page, "feta");
    await expect(page.getByRole("link", { name: "Grecka sałatka", exact: true })).toBeVisible();
    await expect(page.getByRole("link", { name: "Zupa pomidorowa", exact: true })).toHaveCount(0);
  });

  test("Switch ingredient filter between all and any matching", async ({ page }) => {
    await gotoAndWaitForApp(page, "/");
    await clickChip(page, "banan");
    await clickChip(page, "feta");
    await expect(
      page.getByRole("heading", { name: "Brak pasujących przepisów", exact: true }),
    ).toBeVisible();

    await page.getByRole("button", { name: "Dowolny", exact: true }).click();
    await expect(
      page.getByRole("link", { name: "Smoothie bowl jabłko-banan", exact: true }),
    ).toBeVisible();
    await expect(page.getByRole("link", { name: "Grecka sałatka", exact: true })).toBeVisible();
  });

  test("Create recipe with complete fields and uploaded photo", async ({ page }, testInfo) => {
    const suffix = uniqueSuffix();
    const title = `E2E Pomidorowa ${suffix}`;

    await gotoAndWaitForApp(page, "/recipes/new");
    await fillRecipeEditorWithMinimumValidData(page, title);
    await page.locator("#recipe-source").fill("https://example.com/e2e");

    await page.getByRole("button", { name: "Dodaj składnik" }).click();
    const ingredientRows = page.locator(".inline-list-row");
    await expect(ingredientRows).toHaveCount(2);
    const secondIngredientRow = ingredientRows.nth(1);
    await secondIngredientRow.locator("input[placeholder='np. pomidor']").fill("Cebula");
    await secondIngredientRow.locator("input[placeholder='ilość (opcjonalnie)']").fill("1");
    await secondIngredientRow.locator("select").selectOption("szt");

    await page.getByRole("button", { name: "Dodaj krok" }).click();
    const stepTextareas = page.locator(".step-row textarea");
    await expect(stepTextareas).toHaveCount(2);
    await stepTextareas.nth(1).fill("Podsmaż cebulę.");

    await page.locator("input[placeholder='np. szybkie']").fill("obiad");
    await page.getByRole("button", { name: "Dodaj tag" }).click();

    await page.locator("input[type='file']").setInputFiles({
      name: "photo.gif",
      mimeType: "image/gif",
      buffer: Buffer.from("R0lGODlhAQABAIAAAAAAAP///ywAAAAAAQABAAACAUwAOw==", "base64"),
    });

    await page.getByRole("button", { name: "Zapisz przepis" }).click();
    await expect(page).toHaveURL(/\/recipes\/[0-9a-f-]+/);
    await expect(page.getByRole("heading", { name: title })).toBeVisible();
    await expect(page.locator("img.recipe-photo-image")).toBeVisible();
    await expect(page.locator("ol li")).toHaveCount(2);

    await attachPageSnapshot(page, testInfo, "details-after-create-with-photo");
  });

  test("Create recipe without photo uses placeholder in details", async ({ page }) => {
    const suffix = uniqueSuffix();
    const title = `E2E Bez Zdjęcia ${suffix}`;

    await gotoAndWaitForApp(page, "/recipes/new");
    await fillRecipeEditorWithMinimumValidData(page, title);
    await page.getByRole("button", { name: "Zapisz przepis" }).click();

    await expect(page.getByRole("heading", { name: title })).toBeVisible();
    await expect(page.getByText("Brak zdjęcia przepisu")).toBeVisible();
  });

  test("Invalid editor submission focuses the first field and keeps save reachable", async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await gotoAndWaitForApp(page, "/recipes/new");

    await expect(page.locator("details.editor-anchor-section")).toHaveCount(4);
    await page.getByRole("button", { name: "Zapisz przepis" }).click();

    await expect(page.locator("#recipe-title")).toBeFocused();
    await expect(page.locator("#recipe-title")).toHaveAttribute("aria-invalid", "true");
    await expect(page.locator("#recipe-title")).toHaveAttribute(
      "aria-describedby",
      "recipe-title-error",
    );
  });

  test("Display source as clickable URL and plain text", async ({ page }) => {
    const linkTitle = `E2E Link Source ${uniqueSuffix()}`;
    await gotoAndWaitForApp(page, "/recipes/new");
    await fillRecipeEditorWithMinimumValidData(page, linkTitle);
    await page.locator("#recipe-source").fill("https://example.com/source-link");
    await page.getByRole("button", { name: "Zapisz przepis" }).click();
    await expect(page.locator("a[href='https://example.com/source-link']")).toBeVisible();

    const textTitle = `E2E Text Source ${uniqueSuffix()}`;
    await gotoAndWaitForApp(page, "/recipes/new");
    await fillRecipeEditorWithMinimumValidData(page, textTitle);
    await page.locator("#recipe-source").fill("Przepis od babci");
    await page.getByRole("button", { name: "Zapisz przepis" }).click();
    await expect(page.getByText("Przepis od babci")).toBeVisible();
    await expect(page.locator("a[href='Przepis od babci']")).toHaveCount(0);
  });

  test("Edit existing recipe and preserve updated step order", async ({ page }) => {
    const suffix = uniqueSuffix();
    const title = `E2E Edycja ${suffix}`;

    await gotoAndWaitForApp(page, "/recipes/new");
    await fillRecipeEditorWithMinimumValidData(page, title);
    await page.getByRole("button", { name: "Dodaj krok" }).click();
    const stepTextareas = page.locator(".step-row textarea");
    await expect(stepTextareas).toHaveCount(2);
    await stepTextareas.nth(1).fill("Drugi krok");
    await page.getByRole("button", { name: "Zapisz przepis" }).click();
    await expect(page).toHaveURL(/\/recipes\/[0-9a-f-]+/);
    const detailsUrl = page.url();

    await page.getByRole("link", { name: "Edytuj", exact: true }).click();
    await page.locator("#recipe-title").fill(`${title} Updated`);
    await page.getByRole("button", { name: "W górę" }).nth(1).click();
    await page.getByRole("button", { name: "Zapisz przepis" }).click();
    await expect(page.getByText("Zmiany przepisu zostały zapisane.")).toBeVisible();
    await gotoAndWaitForApp(page, detailsUrl.replace("http://127.0.0.1:5180", ""));
    await expect(page.getByRole("heading", { name: `${title} Updated` })).toBeVisible();
    await expect(page.locator("ol li").first()).toHaveText("Drugi krok");
  });

  test("Reject duplicate title", async ({ page }) => {
    await gotoAndWaitForApp(page, "/recipes/new");
    await fillRecipeEditorWithMinimumValidData(page, "Zupa pomidorowa");
    await page.getByRole("button", { name: "Zapisz przepis" }).click();
    await expect(page.getByText("title must be unique", { exact: false })).toBeVisible();
  });

  test("Add ingredient without quantity is accepted", async ({ page }) => {
    const title = `E2E Bez Ilości ${uniqueSuffix()}`;
    await gotoAndWaitForApp(page, "/recipes/new");
    await fillRecipeEditorWithMinimumValidData(page, title);
    await page.locator("input[placeholder='ilość (opcjonalnie)']").first().fill("");
    await page.getByRole("button", { name: "Zapisz przepis" }).click();
    await expect(page.getByRole("heading", { name: title })).toBeVisible();
    await expect(page.locator("ul li").first()).toContainText("pomidor");
  });

  test("Open recipe details from library", async ({ page }) => {
    await gotoAndWaitForApp(page, "/");
    await openDetailsCard(page, "Grecka sałatka");
    await expect(page.getByRole("heading", { name: "Grecka sałatka" })).toBeVisible();
    await expect(page.getByRole("heading", { name: "Składniki" })).toBeVisible();
    await expect(page.getByRole("heading", { name: "Kroki" })).toBeVisible();
  });

  test("Return from details preserves title, tag and ingredient filters", async ({ page }) => {
    await gotoAndWaitForApp(page, "/");
    await (await visibleLibrarySearch(page)).fill("zupa");
    await expect(page.locator(".library-results-toolbar h2")).toHaveText("1 przepis");
    await clickChip(page, "obiad");
    await clickChip(page, "pomidor");

    await page.getByRole("link", { name: "Zupa pomidorowa" }).first().click();
    await expect(page.getByRole("heading", { name: "Zupa pomidorowa" })).toBeVisible();
    await page.getByRole("link", { name: "Powrót" }).click();

    await expect(page).toHaveURL(/q=zupa/);
    await expect(page).toHaveURL(/tags=obiad/);
    await expect(page).toHaveURL(/ingredients=pomidor/);
    await expect(await visibleLibrarySearch(page)).toHaveValue("zupa");
    await expect(page.getByRole("button", { name: "Usuń filtr tagu obiad" })).toBeVisible();
    await expect(page.getByRole("button", { name: "Usuń filtr składnika pomidor" })).toBeVisible();
  });

  test("Archive recipe from UI", async ({ page }) => {
    const title = `E2E Archiwum ${uniqueSuffix()}`;
    await gotoAndWaitForApp(page, "/recipes/new");
    await fillRecipeEditorWithMinimumValidData(page, title);
    await page.getByRole("button", { name: "Zapisz przepis" }).click();
    await expect(page.getByRole("heading", { name: title })).toBeVisible();

    page.once("dialog", async (dialog) => {
      await dialog.accept();
    });
    await page.getByRole("button", { name: "Archiwizuj przepis" }).click();
    await expect(page).toHaveURL(/\/$/);
    await expect(page.getByRole("link", { name: title })).toHaveCount(0);
  });

  test("Ingredient suggestions during editing", async ({ page }) => {
    await gotoAndWaitForApp(page, "/recipes/new");
    await page.locator("input[placeholder='np. pomidor']").first().fill("pom");
    await expect
      .poll(async () => {
        const values = await page
          .locator("#ingredient-suggestion-list option")
          .evaluateAll((options) =>
            options.map((option) => option.getAttribute("value")?.toLowerCase() ?? ""),
          );
        return values.includes("pomidor");
      })
      .toBe(true);
  });

  test("Tag suggestions during editing", async ({ page }) => {
    await gotoAndWaitForApp(page, "/recipes/new");
    await page.locator("input[placeholder='np. szybkie']").fill("szy");
    await expect(page.locator("#tag-suggestion-list option[value='szybkie']")).toHaveCount(1);
  });

  test("Filter by recipe metadata (PENDING owner: design-recipe-metadata-filter)", async () => {
    test.skip(true, "PENDING: metadata filtering is a design-only change until its contract and implementation tasks are complete.");
  });
});

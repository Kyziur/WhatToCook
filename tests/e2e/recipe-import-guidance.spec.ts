import { expect, test } from "@playwright/test";

test("@ux05 invalid recipe URL explains what to correct", async ({ page }) => {
  await page.goto("/recipes/import");
  await page.getByRole("textbox", { name: "Adres przepisu" }).fill("not-a-valid-url");
  const submit = page.getByRole("button", { name: "Utwórz szkic importu" });
  await expect(submit).toBeEnabled();
  await submit.click();
  await expect(page.getByRole("alert")).toContainText("Sprawdź adres URL przepisu");
});

test("@ux05 unsupported recipe site offers a next step", async ({ page }) => {
  await page.goto("/recipes/import");
  await page.getByRole("textbox", { name: "Adres przepisu" }).fill("https://example.com/recipe");
  await page.getByRole("button", { name: "Utwórz szkic importu" }).click();
  await expect(page.getByRole("alert")).toContainText("Ta witryna nie jest obsługiwana");
});

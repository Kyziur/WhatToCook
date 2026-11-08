import { expect, type Page, test } from "@playwright/test";
import {
  fillRecipeEditorWithMinimumValidData,
  gotoAndWaitForApp,
  uniqueSuffix,
} from "./helpers";

async function interruptCircuitThenReconnect(page: Page): Promise<void> {
  const dialog = page.locator("#components-reconnect-modal");
  await page.context().setOffline(true);
  try {
    // Offline emulation severs browser WebSockets without killing the test server or database.
    await expect(dialog).toBeVisible({ timeout: 20_000 });
    await page.waitForTimeout(10_000);
  } finally {
    // Always restore connectivity, including when an assertion fails.
    await page.context().setOffline(false);
  }
  await expect(dialog).toBeHidden({ timeout: 25_000 });
}

test.describe("OpenSpec: accessible-async-feedback (UX-03 SignalR reconnect)", () => {
  test.setTimeout(90_000);

  test("Recipe editor preserves unsaved input and creates exactly one recipe after reconnect", async ({
    page,
    request,
  }) => {
    const title = `Reconnect recipe ${uniqueSuffix()}`;
    await gotoAndWaitForApp(page, "/recipes/new");
    await fillRecipeEditorWithMinimumValidData(page, title);

    await interruptCircuitThenReconnect(page);

    await expect(page).toHaveURL(/\/recipes\/new$/);
    await expect(page.locator("#recipe-title")).toHaveValue(title);
    await expect(page.locator("#recipe-servings")).toHaveValue("4");
    await expect(page.locator("input[placeholder='np. pomidor']").first()).toHaveValue("Pomidor");
    await expect(page.locator(".step-row textarea").first()).toHaveValue("Pokrój pomidory");

    await page.getByRole("button", { name: "Zapisz przepis" }).click();
    await expect(page).toHaveURL(/\/recipes\/[0-9a-f-]+$/);
    const response = await request.get(
      `http://127.0.0.1:5181/api/recipes/search?title=${encodeURIComponent(title)}`
    );
    expect(response.ok()).toBeTruthy();
    const matches = (await response.json()) as Array<{ title: string }>;
    expect(matches.filter((recipe) => recipe.title === title)).toHaveLength(1);
  });

  test("Planner preserves an unsaved new-plan form and creates a single plan after reconnect", async ({
    page,
  }) => {
    const name = `Reconnect plan ${uniqueSuffix()}`;
    await gotoAndWaitForApp(page, "/planning");
    const nameField = page.locator("#new-plan-name");
    if (!(await nameField.isVisible())) {
      await page.locator("button:visible").filter({ hasText: /^Nowy plan$/ }).first().click();
    }
    await expect(nameField).toBeVisible();
    await expect(page.getByRole("button", { name: "Utwórz plan" })).toHaveAttribute(
      "data-interactive",
      "true"
    );

    await page.locator("#new-date-from").fill("2027-03-01");
    await page.locator("#new-date-to").fill("2027-03-07");
    await nameField.fill(name);

    await interruptCircuitThenReconnect(page);

    await expect(nameField).toHaveValue(name);
    await expect(page.locator("#new-date-from")).toHaveValue("2027-03-01");
    await expect(page.locator("#new-date-to")).toHaveValue("2027-03-07");
    await page.getByRole("button", { name: "Utwórz plan" }).click();
    await expect(page.getByText("Plan utworzony.")).toBeVisible();
    await expect(page.locator("#plan-selector option").filter({ hasText: name })).toHaveCount(1);
  });
});

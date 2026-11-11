import { expect, type Page, test } from "@playwright/test";
import {
  addRecipeToSlot,
  createPlan,
  createPlanWithRecipe,
  createRecipe,
  gotoAndWaitForApp,
  savePlan,
  uniqueSuffix,
} from "./helpers";

test.describe("OpenSpec: shopping-list-management", () => {
  test("Generate shopping list for a plan", async ({ page }) => {
    const suffix = uniqueSuffix();
    const recipeTitle = `Zakupy pomidor ${suffix}`;
    const planName = `Plan zakupy ${suffix}`;

    await createRecipe(page, recipeTitle, "Pomidor", "2", "szt");
    await createPlanWithRecipe(page, planName, recipeTitle, "2026-07-01");

    await openShoppingFromPlanner(page);
    await expect(page).toHaveURL(/\/shopping\?planId=/);
    await expectShoppingItem(page, "Pomidor", "2 szt");
  });

  test("Show no generated list for an empty newly created plan", async ({ page }) => {
    const suffix = uniqueSuffix();
    const planName = `Plan bez generacji ${suffix}`;

    await createPlan(page, planName, "2026-07-02", "2026-07-08");
    await gotoAndWaitForApp(page, "/shopping");
    await selectShoppingPlan(page, planName);

    await expect(page.getByRole("button", { name: "Generuj listę zakupów", exact: true })).toBeEnabled();
    await expect(page.locator(".shopping-item")).toHaveCount(0);
  });

  test("Aggregate matching ingredient and unit", async ({ page }) => {
    const suffix = uniqueSuffix();
    const recipeA = `Aggr A ${suffix}`;
    const recipeB = `Aggr B ${suffix}`;
    const planName = `Plan aggr ${suffix}`;

    await createRecipe(page, recipeA, "Pomidor", "2", "szt");
    await createRecipe(page, recipeB, "Pomidor", "3", "szt");
    await createPlan(page, planName, "2026-07-03", "2026-07-09");
    await addRecipeToSlot(page, "2026-07-03", recipeA);
    await addRecipeToSlot(page, "2026-07-03", recipeB);
    await savePlan(page);

    await openShoppingFromPlanner(page);
    await expectShoppingItem(page, "Pomidor", "5 szt");
    await expect(page.locator(".shopping-item").filter({ has: page.locator(".shopping-item-copy strong", { hasText: "Pomidor" }) })).toHaveCount(1);
  });

  test("Keep separate items for different units", async ({ page }) => {
    const suffix = uniqueSuffix();
    const recipeA = `Jednostki kg ${suffix}`;
    const recipeB = `Jednostki szt ${suffix}`;
    const planName = `Plan units ${suffix}`;

    await createRecipe(page, recipeA, "Pomidor", "1", "kg");
    await createRecipe(page, recipeB, "Pomidor", "2", "szt");
    await createPlan(page, planName, "2026-07-10", "2026-07-16");
    await addRecipeToSlot(page, "2026-07-10", recipeA);
    await addRecipeToSlot(page, "2026-07-11", recipeB);
    await savePlan(page);

    await openShoppingFromPlanner(page);
    await expectShoppingItem(page, "Pomidor", "1 kg");
    await expectShoppingItem(page, "Pomidor", "2 szt");
  });

  test("Include ingredient without quantity", async ({ page }) => {
    const suffix = uniqueSuffix();
    const recipeTitle = `Bez ilości ${suffix}`;
    const planName = `Plan bez ilości ${suffix}`;

    await createRecipe(page, recipeTitle, "Sól do smaku");
    await createPlanWithRecipe(page, planName, recipeTitle, "2026-07-17");
    await openShoppingFromPlanner(page);

    await expect(page.getByText("Sól do smaku")).toBeVisible();
  });

  test("Toggle item as MAM and NIE MAM", async ({ page }) => {
    const suffix = uniqueSuffix();
    const recipeTitle = `Toggle ${suffix}`;
    const planName = `Plan toggle ${suffix}`;

    await createRecipe(page, recipeTitle, "Feta", "1", "opakowanie");
    await createPlanWithRecipe(page, planName, recipeTitle, "2026-07-18");
    await openShoppingFromPlanner(page);

    const item = shoppingItem(page, "Feta");
    await expect(item).toHaveAttribute("aria-pressed", "false");
    await item.focus();
    await expect(item).toBeFocused();
    await page.keyboard.press("Space");
    await expect(item).toHaveAttribute("aria-pressed", "true");
    await expect(item).toBeFocused();
    await page.keyboard.press("Enter");
    await expect(item).toHaveAttribute("aria-pressed", "false");
  });

  test("Add manual text item", async ({ page }) => {
    const suffix = uniqueSuffix();
    const recipeTitle = `Manual ${suffix}`;
    const planName = `Plan manual ${suffix}`;

    await createRecipe(page, recipeTitle, "Makaron", "300", "g");
    await createPlanWithRecipe(page, planName, recipeTitle, "2026-07-19");
    await openShoppingFromPlanner(page);

    await openManualEntry(page);
    await page.locator("#manual-item-input").fill("Papier do pieczenia");
    await page.locator("#manual-item-input").press("Enter");
    await expect(page.getByText("Papier do pieczenia")).toBeVisible();
    await page.reload();
    await expect(shoppingItem(page, "Papier do pieczenia")).toBeVisible();
  });

  test("Saving a changed plan requires replacement confirmation and preserves shopping data", async ({ page }) => {
    const suffix = uniqueSuffix();
    const recipeTitle = `Regen ${suffix}`;
    const planName = `Plan regen ${suffix}`;

    await createRecipe(page, recipeTitle, "Pomidor", "1", "szt");
    await createPlanWithRecipe(page, planName, recipeTitle, "2026-07-20");
    await openShoppingFromPlanner(page);
    await shoppingItem(page, "Pomidor").click();
    await openManualEntry(page);
    await page.locator("#manual-item-input").fill("Mleko");
    const addManualItemButton = page.getByRole("button", { name: "Dodaj", exact: true });
    await expect(addManualItemButton).toBeEnabled();
    await addManualItemButton.click();
    await expect(shoppingItem(page, "Mleko")).toBeVisible();

    await gotoAndWaitForApp(page, "/planning");
    await selectPlannerPlan(page, planName);
    await page.getByRole("button", { name: /Edytuj/ }).first().click();
    await expect(page.locator("#plan-name")).toHaveValue(planName);
    await page.locator("#plan-name").fill(`${planName} v2`);
    await increaseRecipeMultiplier(page, recipeTitle);
    await page.getByRole("button", { name: "Zapisz plan" }).click();
    await expect(page.getByRole("heading", { name: "Lista zakupów wymaga regeneracji" })).toBeVisible();
    await page.getByRole("button", { name: "Potwierdź regenerację" }).click();
    await expect(page.getByText("Plan zapisany, a lista zakupów została uaktualniona.")).toBeVisible();

    await gotoAndWaitForApp(page, "/shopping");
    await selectShoppingPlan(page, `${planName} v2`);
    await expect(shoppingItem(page, "Pomidor")).toHaveAttribute("aria-pressed", "false");
    await expect(page.getByText("Ilość uległa zmianie — sprawdź dostępność.")).toBeVisible();
    await expect(page.getByText("Mleko")).toBeVisible();
    await expect(shoppingItem(page, "Pomidor").locator(".shopping-item-quantity")).toContainText("2 szt");
  });

  test("Direct shopping regeneration preserves saved shopping work", async ({ page }) => {
    const suffix = uniqueSuffix();
    const recipeTitle = `Bezpieczna regen ${suffix}`;
    const planName = `Plan bezpieczna regen ${suffix}`;

    await createRecipe(page, recipeTitle, "Pomidor", "1", "szt");
    await createPlanWithRecipe(page, planName, recipeTitle, "2026-07-22");
    await openShoppingFromPlanner(page);
    const firstItem = shoppingItem(page, "Pomidor");
    await firstItem.click();
    await expect(firstItem).toHaveAttribute("aria-pressed", "true");
    await openManualEntry(page);
    await page.locator("#manual-item-input").fill("Mleko");
    await page.getByRole("button", { name: "Dodaj", exact: true }).click();
    await expect(page.getByText("Mleko")).toBeVisible();

    await page.getByRole("button", { name: "Regeneruj listę zakupów" }).click();
    await expect(page.getByText("Mleko")).toBeVisible();
    await expect(shoppingItem(page, "Pomidor")).toHaveAttribute("aria-pressed", "true");
    await page.reload();
    await expect(page.getByText("Mleko")).toBeVisible();
    await expect(shoppingItem(page, "Pomidor")).toHaveAttribute("aria-pressed", "true");
    await expect(page.getByRole("button", { name: "Regeneruj listę zakupów" })).toBeVisible();
  });
  test("Copy shopping list as plain text", async ({ page }) => {
    const suffix = uniqueSuffix();
    const recipeTitle = `Copy ${suffix}`;
    const planName = `Plan copy ${suffix}`;

    await createRecipe(page, recipeTitle, "Pomidor", "2", "szt");
    await createPlanWithRecipe(page, planName, recipeTitle, "2026-07-21");
    await openShoppingFromPlanner(page);

    await page.getByRole("button", { name: "Kopiuj jako tekst" }).click();
    await expect(page.getByText("Podgląd kopiowania")).toBeVisible();
    await expect(page.locator("textarea")).toHaveValue(new RegExp(`Lista zakupów - ${planName}`));
    await expect(page.locator("textarea")).toHaveValue(/2 szt pomidor/i);
  });
});

async function selectPlannerPlan(page: Page, name: string): Promise<void> {
  const selector = page.locator("#plan-selector");
  await expect(selector).toBeVisible();
  await selector.selectOption({ label: name });
  await expect(selector.locator("option:checked")).toHaveText(name);
}

async function openShoppingFromPlanner(page: Page): Promise<void> {
  const link = page.getByRole("link", { name: "Otwórz", exact: true });
  if (await link.isVisible()) {
    await link.click();
  } else {
    const planId = await page.locator("#plan-selector option:checked").getAttribute("value");
    if (!planId) throw new Error("Selected planner option has no plan id.");
    await page.goto(`/shopping?planId=${planId}`);
  }
  await expect(page).toHaveURL(/\/shopping\?planId=/);
}

async function selectShoppingPlan(page: Page, planName: string): Promise<void> {
  const plan = page.getByRole("button", { name: new RegExp(planName) });
  await plan.click();
  await expect(plan).toHaveAttribute("aria-pressed", "true");
}

function shoppingItem(page: Page, name: string, quantity?: string) {
  const item = page.locator(".shopping-item").filter({
    has: page.locator(".shopping-item-copy strong").filter({ hasText: name }),
  });
  return quantity
    ? item.filter({ has: page.locator(".shopping-item-quantity").filter({ hasText: quantity }) })
    : item;
}

async function expectShoppingItem(page: Page, name: string, quantity: string): Promise<void> {
  const item = shoppingItem(page, name, quantity);
  await expect(item.locator(".shopping-item-copy strong")).toHaveText(name, { ignoreCase: true });
  await expect(item.locator(".shopping-item-quantity")).toHaveText(quantity);
}

async function openManualEntry(page: Page): Promise<void> {
  const details = page.locator("details.shopping-manual-bar");
  if (!(await details.locator("#manual-item-input").isVisible())) {
    await details.locator("summary").click();
  }
  await expect(details.locator("#manual-item-input")).toBeVisible();
}

async function increaseRecipeMultiplier(page: Page, recipeTitle: string): Promise<void> {
  const card = page.locator(".planner-meal-card:visible").filter({ hasText: recipeTitle }).first();
  await card.getByRole("button", { name: `Opcje posiłku ${recipeTitle}` }).click();
  await card.getByRole("button", { name: `Zwiększ mnożnik ${recipeTitle}` }).click();
}

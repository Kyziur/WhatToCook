import { expect, type Page, test } from "@playwright/test";
import { gotoAndWaitForApp, uniqueSuffix } from "./helpers";

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
    await item.click();
    await expect(item).toHaveAttribute("aria-pressed", "true");
    await item.click();
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
    await expect(shoppingItem(page, "Pomidor")).toHaveAttribute("aria-pressed", "true");
    await expect(page.getByText("Mleko")).toBeVisible();
    await expectShoppingItem(page, "Pomidor", "2 szt");
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

async function createRecipe(
  page: Page,
  title: string,
  ingredient: string,
  quantity?: string,
  unit?: string,
): Promise<void> {
  await gotoAndWaitForApp(page, "/recipes/new");
  await page.locator("#recipe-title").fill(title);
  await page.locator("#recipe-servings").fill("4");
  await page.locator("input[placeholder='np. pomidor']").first().fill(ingredient);
  await page.locator("input[placeholder='ilość (opcjonalnie)']").first().fill(quantity ?? "");
  if (unit) {
    await page.locator("select").first().selectOption(unit);
  } else {
    await page.locator("select").first().selectOption("");
  }
  await page.locator(".step-row textarea").first().fill("Krok 1");
  await page.getByRole("button", { name: "Zapisz przepis" }).click();
  await expect(page).toHaveURL(/\/recipes\/[0-9a-f-]+/);
}

async function createPlan(page: Page, name: string, from: string, to: string): Promise<void> {
  await gotoAndWaitForApp(page, "/planning");
  await expect(page.getByText("Ładowanie planu posiłków")).toHaveCount(0);
  const form = page.locator("#new-date-from");
  const newPlanButton = page.locator("button:visible").filter({ hasText: /^Nowy plan$/ });
  await expect(form.or(newPlanButton).first()).toBeVisible();
  if (!(await form.isVisible())) {
    await expect(newPlanButton).toHaveCount(1);
    await newPlanButton.click();
  }
  await expect(form).toBeVisible();
  await expect(page.getByRole("button", { name: "Utwórz plan" })).toHaveAttribute("data-interactive", "true");
  await page.locator("#new-date-from").fill(from);
  await page.locator("#new-date-to").fill(to);
  await page.locator("#new-plan-name").fill(name);
  await page.getByRole("button", { name: "Utwórz plan" }).click();
  await expect(page.locator("#plan-selector option:checked")).toHaveText(name);
}

async function selectPlannerPlan(page: Page, name: string): Promise<void> {
  const selector = page.locator("#plan-selector");
  await expect(selector).toBeVisible();
  await selector.selectOption({ label: name });
  await expect(selector.locator("option:checked")).toHaveText(name);
}

async function addRecipeToSlot(page: Page, date: string, recipeTitle: string): Promise<void> {
  const cell = page.locator(`.planner-slot-cell[data-day='${date}'][data-slot='Dinner']`).first();
  if ((await cell.count()) > 0 && (await cell.isVisible())) {
    await cell.getByRole("button", { name: /Dodaj/ }).click();
  } else {
    await page.locator(`.planner-mobile-day-picker button[data-day-target='${date}']`).click();
    await page.locator(".planner-mobile-slot").filter({ hasText: "Obiad" }).getByRole("button", { name: /Dodaj/ }).click();
  }
  const drawer = page.locator(".planner-recipe-drawer");
  await expect(drawer).toBeVisible();
  await drawer.getByPlaceholder("Szukaj w przepisach...").fill(recipeTitle);
  await drawer.getByRole("button", { name: new RegExp(recipeTitle) }).click();
  if ((await cell.count()) > 0 && (await cell.isVisible())) {
    await expect(cell.getByText(recipeTitle).first()).toBeVisible();
  } else {
    await expect(page.locator(".planner-mobile-slot").filter({ hasText: "Obiad" }).getByText(recipeTitle).first()).toBeVisible();
  }
}


async function savePlan(page: Page): Promise<void> {
  await page.getByRole("button", { name: "Zapisz plan" }).click();
  await expect(page.getByText("Plan zapisany, a lista zakupów została uaktualniona.")).toBeVisible();
}

async function createPlanWithRecipe(page: Page, planName: string, recipeTitle: string, firstDay: string): Promise<void> {
  const to = new Date(new Date(firstDay).getTime() + 6 * 24 * 60 * 60 * 1000).toISOString().slice(0, 10);
  await createPlan(page, planName, firstDay, to);
  await addRecipeToSlot(page, firstDay, recipeTitle);
  await savePlan(page);
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

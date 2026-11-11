import type { Locator, Page, TestInfo } from "@playwright/test";
import { expect } from "@playwright/test";

export function uniqueSuffix(): string {
  return `${Date.now()}-${Math.floor(Math.random() * 10000)}`;
}

export async function visibleLibrarySearch(page: Page): Promise<Locator> {
  const isDesktop = await page.evaluate(() => window.matchMedia("(min-width: 900px)").matches);
  const search = page.locator(isDesktop ? "#library-sidebar-search" : "#library-search");
  await expect(search).toHaveAttribute("data-interactive", "true");
  return search;
}

export async function clickChip(page: Page, label: string): Promise<void> {
  await expandLibraryFilters(page);
  const controls = libraryFilterControls(page, label);
  await searchForFilter(page, label, controls);
  await expandFilterLists(page, controls);

  if ((await controls.tagChip.count()) > 0) {
    await controls.tagChip.first().click();
  } else {
    await controls.ingredientCheckbox.first().click();
  }
}

function libraryFilterControls(page: Page, label: string): { tagChip: Locator; ingredientCheckbox: Locator } {
  return {
    tagChip: page.getByRole("button", {
      name: new RegExp(`^${escapeRegex(label)}(?:\\s+\\d+)?$`, "i"),
    }),
    ingredientCheckbox: page.getByRole("checkbox", {
      name: new RegExp(`^${escapeRegex(label)}$`, "i"),
    }),
  };
}

async function expandLibraryFilters(page: Page): Promise<void> {
  const toggle = page.locator("#library-filter-toggle");
  if ((await toggle.count()) > 0 && (await toggle.isVisible()) && (await toggle.getAttribute("aria-expanded")) === "false") {
    await toggle.click();
  }
}

async function searchForFilter(
  page: Page,
  label: string,
  controls: { tagChip: Locator; ingredientCheckbox: Locator },
): Promise<void> {
  if (await hasFilterControl(controls)) return;
  await page.getByPlaceholder("Szukaj tagów...").fill(label);
  if (await hasFilterControl(controls)) return;
  await page.getByPlaceholder("Szukaj składników...").fill(label);
}

async function expandFilterLists(
  page: Page,
  controls: { tagChip: Locator; ingredientCheckbox: Locator },
): Promise<void> {
  const expandButtons = page.getByRole("button", { name: /Pokaż wszystkie/i });
  while (!(await hasFilterControl(controls))) {
    await expect
      .poll(
        async () =>
          (await hasFilterControl(controls))
          || ((await expandButtons.count()) > 0 && (await expandButtons.first().isVisible())),
      )
      .toBe(true);

    if (await hasFilterControl(controls)) {
      return;
    }

    try {
      await expandButtons.first().click({ timeout: 2_000 });
    } catch (error) {
      if (!(await hasFilterControl(controls))) {
        throw error;
      }
    }
  }
}

async function hasFilterControl(controls: { tagChip: Locator; ingredientCheckbox: Locator }): Promise<boolean> {
  return (await controls.tagChip.count()) > 0 || (await controls.ingredientCheckbox.count()) > 0;
}

export async function attachPageSnapshot(
  page: Page,
  testInfo: TestInfo,
  name: string
): Promise<void> {
  await testInfo.attach(name, {
    body: await page.screenshot({ fullPage: true }),
    contentType: "image/png",
  });
}

export async function fillRecipeEditorWithMinimumValidData(
  page: Page,
  title: string
): Promise<void> {
  await expect(page.getByRole("button", { name: "Zapisz przepis" })).toHaveAttribute(
    "data-interactive",
    "true",
  );
  await page.locator("#recipe-title").fill(title);
  await expect(page.locator("#recipe-title")).toHaveValue(title);

  await page.locator("#recipe-servings").fill("4");
  await expect(page.locator("#recipe-servings")).toHaveValue("4");

  const firstIngredient = page.locator("input[placeholder='np. pomidor']").first();
  await firstIngredient.fill("Pomidor");
  await expect(firstIngredient).toHaveValue("Pomidor");

  const firstStep = page.locator(".step-row textarea").first();
  await firstStep.fill("Pokrój pomidory");
  await expect(firstStep).toHaveValue("Pokrój pomidory");
}

export async function gotoAndWaitForApp(page: Page, url: string): Promise<void> {
  const waitForBlazorNegotiate = page
    .waitForResponse(
      (response) =>
        response.request().method() === "POST" &&
        response.url().includes("/_blazor/negotiate") &&
        response.ok(),
      { timeout: 10_000 }
    )
    .catch(() => null);
  const waitForBlazorSocket = page
    .waitForEvent("websocket", {
      predicate: (socket) => socket.url().includes("/_blazor"),
      timeout: 10_000,
    })
    .catch(() => null);

  await page.goto(url);
  await page.waitForLoadState("domcontentloaded");
  await waitForBlazorNegotiate;
  await waitForBlazorSocket;
  await expect(page.locator(".screen, .planner-workspace").first()).toBeVisible();
}

export async function openDetailsCard(page: Page, recipeTitle: string): Promise<void> {
  await page.getByRole("link", { name: recipeTitle }).click();
}

export async function recipeCards(page: Page): Promise<Locator> {
  return page.locator(".recipe-card");
}

function escapeRegex(value: string): string {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

export async function createRecipe(
  page: Page,
  title: string,
  ingredient: string,
  quantity?: string,
  unit?: string,
): Promise<void> {
  await gotoAndWaitForApp(page, "/recipes/new");
  await expect(page.getByRole("button", { name: "Zapisz przepis" })).toHaveAttribute(
    "data-interactive",
    "true",
  );
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

export async function createPlan(
  page: Page,
  name: string,
  from: string,
  to: string,
): Promise<string> {
  await gotoAndWaitForApp(page, "/planning");
  await expect(page.getByText("Ładowanie planu posiłków")).toHaveCount(0);
  await expect(
    page.locator(".planner-workspace button:not(:disabled):visible").first(),
  ).toBeVisible();
  const emptyState = page.locator(".planner-empty-layout");
  const existingPlanState = page.locator(".planner-period-toolbar");
  const stablePlannerState = page.locator(".planner-empty-layout, .planner-period-toolbar");
  const form = page.locator("#new-date-from");
  const newPlanButton = page.locator("button:visible").filter({ hasText: /^Nowy plan$/ });

  await expect(stablePlannerState).toHaveCount(1);
  await expect(stablePlannerState).toBeVisible();
  if ((await emptyState.count()) === 1) {
    await expect(form).toBeVisible();
  } else {
    await expect(existingPlanState).toBeVisible();
    await expect(newPlanButton).toBeVisible();
    await newPlanButton.click();
  }
  await expect(form).toBeVisible();
  await expect(page.getByRole("button", { name: "Utwórz plan" })).toHaveAttribute(
    "data-interactive",
    "true",
  );
  await page.locator("#new-date-from").fill(from);
  await page.locator("#new-date-to").fill(to);
  await page.locator("#new-plan-name").fill(name);
  await page.getByRole("button", { name: "Utwórz plan" }).click();
  await expect(page.getByText("Plan utworzony.")).toBeVisible();
  const selectedPlan = page.locator("#plan-selector option:checked");
  await expect(selectedPlan).toHaveText(name);
  const planId = await selectedPlan.getAttribute("value");
  if (!planId) {
    throw new Error("Created planner option has no plan id.");
  }
  return planId;
}

export async function addRecipeToSlot(
  page: Page,
  date: string,
  recipeTitle: string,
): Promise<void> {
  const cell = page.locator(`.planner-slot-cell[data-day='${date}'][data-slot='Dinner']`).first();
  if ((await cell.count()) > 0 && (await cell.isVisible())) {
    await cell.getByRole("button", { name: /Dodaj/ }).click();
  } else {
    await page.locator(`.planner-mobile-day-picker button[data-day-target='${date}']`).click();
    await page
      .locator(".planner-mobile-slot")
      .filter({ hasText: "Obiad" })
      .getByRole("button", { name: /Dodaj/ })
      .click();
  }
  const drawer = page.locator(".planner-recipe-drawer");
  await expect(drawer).toBeVisible();
  await drawer.getByPlaceholder("Szukaj w przepisach...").fill(recipeTitle);
  await drawer.getByRole("button", { name: new RegExp(recipeTitle) }).click();
  if ((await cell.count()) > 0 && (await cell.isVisible())) {
    await expect(cell.getByText(recipeTitle).first()).toBeVisible();
  } else {
    const mobileDinnerSlot = page.locator(".planner-mobile-slot").filter({ hasText: "Obiad" });
    await expect(mobileDinnerSlot.getByText(recipeTitle).first()).toBeVisible();
  }
}

export async function savePlan(page: Page): Promise<void> {
  await page.getByRole("button", { name: "Zapisz plan" }).click();
  await expect(page.getByText("Plan zapisany, a lista zakupów została uaktualniona.")).toBeVisible();
}

export async function createPlanWithRecipe(
  page: Page,
  planName: string,
  recipeTitle: string,
  firstDay: string,
): Promise<string> {
  const to = new Date(
    new Date(firstDay).getTime() + 6 * 24 * 60 * 60 * 1000,
  ).toISOString().slice(0, 10);
  const planId = await createPlan(page, planName, firstDay, to);
  await addRecipeToSlot(page, firstDay, recipeTitle);
  await savePlan(page);
  return planId;
}

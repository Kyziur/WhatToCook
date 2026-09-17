import { expect, type Page, test } from "@playwright/test";
import { gotoAndWaitForApp, uniqueSuffix } from "./helpers";

test.describe("OpenSpec: planning-workspace-ui", () => {
  test("Create and edit a plan", async ({ page }) => {
    const name = `Plan ${uniqueSuffix()}`;
    await gotoAndWaitForApp(page, "/planning");
    await createPlan(page, "2026-05-01", "2026-05-07", name);

    await page.getByRole("button", { name: /Edytuj/ }).first().click();
    await expect(page.locator("#plan-name")).toHaveValue(name);
    await page.locator("#plan-name").fill(`${name} rodzinny`);
    await page.getByRole("button", { name: "Zapisz plan" }).click();
    await expect(page.getByText("Plan zapisany, a lista zakupów została uaktualniona.")).toBeVisible();
  });

  test("Search drawer and assign a recipe to dinner slot", async ({ page }) => {
    await gotoAndWaitForApp(page, "/planning");
    await createPlan(page, "2026-05-01", "2026-05-07", `Slot ${uniqueSuffix()}`);

    await openSlotPicker(page, "2026-05-01", "Dinner");
    const drawer = page.locator(".planner-recipe-drawer");
    await drawer.getByPlaceholder("Szukaj w przepisach...").fill("gre");
    await expect(drawer.getByRole("button", { name: /Grecka sałatka/ })).toHaveCount(1);
    await drawer.getByRole("button", { name: /Grecka sałatka/ }).click();

    await expectRecipeInDinnerSlot(page, "Grecka sałatka", "2026-05-01");
  });

  test("Prevent duplicate recipe in one day and allow it on another", async ({ page }) => {
    await gotoAndWaitForApp(page, "/planning");
    await createPlan(page, "2026-05-01", "2026-05-07", `Duplikat ${uniqueSuffix()}`);

    await addRecipeToSlot(page, "2026-05-01", "Grecka sałatka");
    await openSlotPicker(page, "2026-05-01", "Dinner");
    await expect(page.locator(".planner-recipe-drawer").getByRole("button", { name: /Grecka sałatka/ })).toHaveCount(0);
    await page.locator(".planner-recipe-drawer").getByRole("button", { name: "Zamknij wybór przepisu" }).click();

    await addRecipeToSlot(page, "2026-05-02", "Grecka sałatka");
    await expectRecipeInDinnerSlot(page, "Grecka sałatka", "2026-05-02");
  });

  test("Initial plan save generates shopping list without a replacement dialog", async ({ page }) => {
    await gotoAndWaitForApp(page, "/planning");
    await createPlan(page, "2026-05-08", "2026-05-14", `Zakupy ${uniqueSuffix()}`);
    await addRecipeToSlot(page, "2026-05-08", "Grecka sałatka");

    await page.getByRole("button", { name: "Zapisz plan" }).click();
    await expect(page.getByText("Plan zapisany, a lista zakupów została uaktualniona.")).toBeVisible();
    await expect(page.getByRole("heading", { name: "Lista zakupów wymaga regeneracji" })).toHaveCount(0);
  });

  test("Show one selected day and usable slots on mobile", async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await gotoAndWaitForApp(page, "/planning");
    await createPlan(page, "2026-11-01", "2026-11-07", `Mobile ${uniqueSuffix()}`);

    await expect(page.locator(".planner-mobile-day-picker button")).toHaveCount(7);
    await expect(page.locator(".planner-mobile-slot")).toHaveCount(4);
    await expect(page.locator(".planner-desktop-week")).toHaveCSS("display", "none");
    await page.locator(".planner-mobile-slot").filter({ hasText: "Obiad" }).getByRole("button", { name: /Dodaj/ }).click();
    await expect(page.locator(".planner-recipe-drawer")).toBeVisible();
  });
});

async function createPlan(page: Page, from: string, to: string, name: string): Promise<void> {
  await expect(page.getByText("Ładowanie planu posiłków")).toHaveCount(0);
  await expect(page.locator(".planner-workspace button:not(:disabled):visible").first()).toBeVisible();
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
  await expect(page.getByRole("button", { name: "Utwórz plan" })).toHaveAttribute("data-interactive", "true");
  await page.locator("#new-date-from").fill(from);
  await page.locator("#new-date-to").fill(to);
  await page.locator("#new-plan-name").fill(name);
  await page.getByRole("button", { name: "Utwórz plan" }).click();
  await expect(page.getByText("Plan utworzony.")).toBeVisible();
}

async function openSlotPicker(page: Page, date: string, slot: "Breakfast" | "SecondBreakfast" | "Dinner" | "Supper"): Promise<void> {
  const cell = page.locator(`.planner-slot-cell[data-day='${date}'][data-slot='${slot}']`).first();
  if ((await cell.count()) > 0 && (await cell.isVisible())) {
    await cell.getByRole("button", { name: /Dodaj/ }).click();
  } else {
    await page.locator(`.planner-mobile-day-picker button[data-day-target='${date}']`).click();
    await page.locator(".planner-mobile-slot").filter({ hasText: slot === "Dinner" ? "Obiad" : "Śniadanie" }).getByRole("button", { name: /Dodaj/ }).click();
  }
  await expect(page.locator(".planner-recipe-drawer")).toBeVisible();
}

async function addRecipeToSlot(page: Page, date: string, recipeTitle: string): Promise<void> {
  await openSlotPicker(page, date, "Dinner");
  const drawer = page.locator(".planner-recipe-drawer");
  await drawer.getByPlaceholder("Szukaj w przepisach...").fill(recipeTitle);
  await drawer.getByRole("button", { name: new RegExp(recipeTitle) }).click();
  await expectRecipeInDinnerSlot(page, recipeTitle, date);
}

async function expectRecipeInDinnerSlot(page: Page, recipeTitle: string, date: string): Promise<void> {
  const desktopSlot = page.locator(`.planner-slot-cell[data-day='${date}'][data-slot='Dinner']:visible`);
  if ((await desktopSlot.count()) > 0) {
    await expect(desktopSlot.getByText(recipeTitle).first()).toBeVisible();
  } else {
    await expect(page.locator(".planner-mobile-slot").filter({ hasText: "Obiad" }).getByText(recipeTitle).first()).toBeVisible();
  }
}

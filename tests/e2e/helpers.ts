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

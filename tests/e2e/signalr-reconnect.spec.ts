import { expect, type Page, type WebSocketRoute, test } from "@playwright/test";
import {
  fillRecipeEditorWithMinimumValidData,
  gotoAndWaitForApp,
  uniqueSuffix,
} from "./helpers";

async function installBlazorNetworkInterruption(page: Page): Promise<() => Promise<void>> {
  let activeSocket: WebSocketRoute | undefined;
  let blockNewSockets = false;

  // Route before navigation: Playwright cannot retroactively intercept an established WebSocket.
  await page.routeWebSocket(/\/_blazor(?:\?|$)/, (socket) => {
    if (blockNewSockets) {
      void socket.close({ code: 1001, reason: "Simulated connection loss" });
      return;
    }
    activeSocket = socket;
    socket.connectToServer();
  });

  return async () => {
    if (!activeSocket) {
      throw new Error("Blazor WebSocket was not intercepted before navigation.");
    }
    const dialog = page.locator("#components-reconnect-modal");
    blockNewSockets = true;
    await page.context().setOffline(true);
    try {
      // Offline emulation alone may not close an already established Chromium WebSocket.
      // Explicitly end the routed socket and block reconnect attempts for ten seconds.
      await activeSocket.close({ code: 1001, reason: "Simulated connection loss" });
      await expect(dialog).toBeVisible({ timeout: 15_000 });
      await page.waitForTimeout(10_000);
    } finally {
      blockNewSockets = false;
      await page.context().setOffline(false);
    }
    await expect(dialog).toBeHidden({ timeout: 30_000 });
  };
}

test.describe("OpenSpec: accessible-async-feedback (UX-03 SignalR reconnect)", () => {
  test.setTimeout(90_000);

  test("Recipe editor preserves unsaved input and creates exactly one recipe after reconnect", async ({
    page,
    request,
  }) => {
    const title = `Reconnect recipe ${uniqueSuffix()}`;
    const interruptConnection = await installBlazorNetworkInterruption(page);
    await gotoAndWaitForApp(page, "/recipes/new");
    await fillRecipeEditorWithMinimumValidData(page, title);

    await interruptConnection();

    await expect(page).toHaveURL(/\/recipes\/new$/);
    await expect(page.locator("#recipe-title")).toHaveValue(title);
    await expect(page.locator("#recipe-servings")).toHaveValue("4");
    await expect(page.locator("input[placeholder='np. pomidor']").first()).toHaveValue("Pomidor");
    await expect(page.locator(".step-row textarea").first()).toHaveValue("Pokrój pomidory");

    await page.getByRole("button", { name: "Zapisz przepis" }).click();
    try {
      await expect(page).toHaveURL(/\/recipes\/[0-9a-f-]+(?:\?|$)/);
    } catch (error) {
      // Persist precise UI evidence in CI output before adjusting reconnect behavior.
      console.error(
        "UX-03 recipe save after reconnect:",
        JSON.stringify({
          url: page.url(),
          alerts: await page.locator("[role='alert'], .validation-message, .inline-warning").allTextContents(),
          title: await page.locator("#recipe-title").inputValue(),
          servings: await page.locator("#recipe-servings").inputValue(),
          ingredient: await page.locator("input[placeholder='np. pomidor']").first().inputValue(),
          step: await page.locator(".step-row textarea").first().inputValue(),
        })
      );
      throw error;
    }
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
    const interruptConnection = await installBlazorNetworkInterruption(page);
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

    await interruptConnection();

    await expect(nameField).toHaveValue(name);
    await expect(page.locator("#new-date-from")).toHaveValue("2027-03-01");
    await expect(page.locator("#new-date-to")).toHaveValue("2027-03-07");
    await page.getByRole("button", { name: "Utwórz plan" }).click();
    await expect(page.getByText("Plan utworzony.")).toBeVisible();
    await expect(page.locator("#plan-selector option").filter({ hasText: name })).toHaveCount(1);
  });
});

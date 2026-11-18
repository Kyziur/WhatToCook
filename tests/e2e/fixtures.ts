import { test as base, type Page } from "@playwright/test";

declare global {
  interface Window {
    Blazor?: { disconnect?: () => void };
  }
}

const disconnectPath = "/_blazor/disconnect";
const disconnectTimeoutMs = 10_000;

/**
 * Terminates the page's Blazor circuit via the public `Blazor.disconnect()` and waits for the server to
 * acknowledge the `/_blazor/disconnect` beacon, so the circuit is released rather than retained in the
 * server's disconnected-circuit cache. Must run while the page is still open.
 * Pages without Blazor (or already closed) are skipped.
 */
export async function disconnectBlazorCircuit(page: Page): Promise<void> {
  if (page.isClosed()) {
    return;
  }
  const hasBlazor = await page.evaluate(() => typeof window.Blazor?.disconnect === "function");
  if (!hasBlazor) {
    return;
  }

  const beacon = page.waitForResponse(
    (response) => new URL(response.url()).pathname === disconnectPath && response.request().method() === "POST",
    { timeout: disconnectTimeoutMs },
  );
  await page.evaluate(() => window.Blazor?.disconnect?.());
  const response = await beacon;
  await response.finished();
}

// Every test page gets its Blazor circuit closed before Playwright tears the context down.
export const test = base.extend<{ page: Page }>({
  page: async ({ page }, use) => {
    await use(page);
    await disconnectBlazorCircuit(page);
  },
});

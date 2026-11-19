import { test as base, type Page, type Response } from "@playwright/test";

declare global {
  interface Window {
    Blazor?: { disconnect?: () => void };
  }
}

const disconnectPath = "/_blazor/disconnect";
const disconnectTimeoutMs = 10_000;
const disconnectDeadlineMs = disconnectTimeoutMs + 5_000;

function isDisconnectBeacon(response: Response): boolean {
  return new URL(response.url()).pathname === disconnectPath && response.request().method() === "POST";
}

/**
 * Best-effort: asks the page's Blazor circuit to disconnect and waits, bounded, for the server to acknowledge the
 * `/_blazor/disconnect` beacon with a 2xx response, so the circuit is released rather than retained in the server's
 * disconnected-circuit cache. Must run while the page is still open.
 *
 * Cleanup must not mask a test failure or prevent the caller from closing the page. Pages without a circuit
 * or already closed are skipped; unexpected cleanup failures are reported as warnings.
 */
export async function disconnectBlazorCircuit(page: Page): Promise<void> {
  if (page.isClosed()) {
    return;
  }

  const { promise: deadline, reject } = Promise.withResolvers<never>();
  const timer = setTimeout(
    () => reject(new Error(`Blazor circuit disconnect exceeded ${disconnectDeadlineMs} ms`)),
    disconnectDeadlineMs,
  );

  const disconnect = async (): Promise<void> => {
    if (!await page.evaluate(() => typeof window.Blazor?.disconnect === "function")) {
      return;
    }

    const [response] = await Promise.all([
      page.waitForResponse(isDisconnectBeacon, { timeout: disconnectTimeoutMs }),
      page.evaluate(() => window.Blazor?.disconnect?.()),
    ]);
    if (!response.ok()) {
      throw new Error(`disconnect beacon returned HTTP ${response.status()}`);
    }
  };

  try {
    await Promise.race([disconnect(), deadline]);
  } catch (error) {
    if (!page.isClosed()) {
      console.warn(`Blazor circuit cleanup failed: ${error instanceof Error ? error.message : String(error)}`);
    }
  } finally {
    clearTimeout(timer);
  }
}

// Ask the server to release the circuit before Playwright tears down the page.
export const test = base.extend<{ page: Page }>({
  page: async ({ page }, use) => {
    await use(page);
    await disconnectBlazorCircuit(page);
  },
});

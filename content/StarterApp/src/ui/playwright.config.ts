import { defineConfig, devices } from "@playwright/test";

/**
 * Playwright configuration for the StarterApp end-to-end and accessibility tests.
 *
 * The dev server serves over plain HTTP unless you drop a key/cert pair into
 * src/ui/ssl (see vite.config.ts). If you do enable local HTTPS, change
 * baseURL and webServer.url below to https://localhost:3000.
 *
 * Reports: the HTML report lands in playwright-report/ (opened automatically on
 * a local failure). In CI a JUnit file and a JSON summary are written to
 * test-results/ as well, which is what .github/workflows/e2e.yml publishes.
 *
 * @see https://playwright.dev/docs/test-configuration
 */
const isCI = !!process.env.CI;

export default defineConfig({
  testDir: "./e2e",
  fullyParallel: true,
  forbidOnly: isCI,
  retries: isCI ? 2 : 0,
  workers: isCI ? 1 : undefined,
  reporter: isCI
    ? [
        ["list"],
        ["github"],
        ["html", { open: "never" }],
        ["junit", { outputFile: "test-results/junit.xml" }],
        ["json", { outputFile: "test-results/results.json" }],
      ]
    : [["list"], ["html", { open: "on-failure" }]],

  use: {
    baseURL: process.env.E2E_BASE_URL || "http://localhost:3000",
    ignoreHTTPSErrors: true,
    trace: "on-first-retry",
    screenshot: "only-on-failure",
  },

  projects: [
    {
      name: "chromium",
      use: { ...devices["Desktop Chrome"] },
    },
  ],

  webServer: {
    command: "pnpm dev",
    url: process.env.E2E_BASE_URL || "http://localhost:3000",
    ignoreHTTPSErrors: true,
    reuseExistingServer: !isCI,
    timeout: 120_000,
  },
});

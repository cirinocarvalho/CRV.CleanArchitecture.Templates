import { test, expect } from "@playwright/test";

/**
 * Smoke tests for the generated app shell. These deliberately assert only what
 * the template ships with, so they pass on a fresh project — extend them as you
 * add real screens.
 */
test.describe("App shell", () => {
  test("serves the app and renders the header", async ({ page }) => {
    await page.goto("/");

    // Both index.html's mount point and App.vue's root carry id="app", so
    // there are two matches once Vue has mounted — assert on the outer one.
    await expect(page.locator("#app").first()).toBeVisible();
    await expect(page.locator("header").first()).toBeVisible();
  });

  //#if (useAuth)
  test("redirects an unauthenticated visitor to the login page", async ({
    page,
  }) => {
    await page.goto("/");

    await expect(page).toHaveURL(/\/login/);
  });
  //#else
  test("renders the home page", async ({ page }) => {
    await page.goto("/");

    await expect(page.getByRole("heading", { name: "Welcome" })).toBeVisible();
  });
  //#endif

  // The help center is deliberately reachable without signing in: someone who
  // cannot get in still needs to read it.
  test("serves the help center to an unauthenticated visitor", async ({
    page,
  }) => {
    await page.goto("/help");

    await expect(page).toHaveURL(/\/help$/);
    await expect(
      page.getByRole("heading", { level: 1, name: "Help" }),
    ).toBeVisible();
    await expect(
      page.locator("#sidebar").getByRole("link", { name: "FAQ" }),
    ).toBeVisible();
  });

  test("navigates from the help sidebar to a topic", async ({ page }) => {
    await page.goto("/help");
    await page.locator("#sidebar").getByRole("link", { name: "FAQ" }).click();

    await expect(page).toHaveURL(/\/help\/faq$/);
    await expect(
      page.getByRole("heading", { name: "Frequently Asked Questions" }),
    ).toBeVisible();
  });
});

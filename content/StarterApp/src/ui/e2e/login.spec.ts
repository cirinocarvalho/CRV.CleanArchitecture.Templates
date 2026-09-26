import { test, expect } from "@playwright/test";

/**
 * Login screen tests. Present only when the project is generated with JWT
 * authentication.
 */
test.describe("Login page", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/login");
  });

  test("displays the sign-in form", async ({ page }) => {
    await expect(page.getByRole("heading", { name: "Sign In" })).toBeVisible();
    await expect(page.getByLabel("Email address")).toBeVisible();
    await expect(page.getByLabel("Password")).toBeVisible();
  });

  test("accepts input in the credential fields", async ({ page }) => {
    await page.getByLabel("Email address").fill("test@example.com");
    await page.getByLabel("Password").fill("not-a-real-password");

    await expect(page.getByLabel("Email address")).toHaveValue(
      "test@example.com",
    );
  });
});

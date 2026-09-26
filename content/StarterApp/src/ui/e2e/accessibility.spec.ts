import { test, expect } from "@playwright/test";
import { checkAccessibility } from "./helpers/accessibility";

const escapeRegExp = (value: string): string =>
  value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");

/**
 * WCAG 2.2 AA / ADA accessibility audit of every page a fresh project serves
 * without signing in. Each page is loaded, allowed to render, then scanned with
 * axe-core (see helpers/accessibility.ts for the rule set and what it does and
 * does not prove).
 *
 * As you add screens, add them here. Pages behind login need an authenticated
 * session first — see https://playwright.dev/docs/auth — and then the same
 * `checkAccessibility` call.
 */
const publicPages: string[] = [
  //#if (useAuth)
  "/login",
  //#endif
  //#if (useLocalIdentity)
  "/register",
  "/forgot-password",
  //#endif
  //#if (!useAuth)
  "/",
  "/attachments",
  //#endif
  "/help",
  //#if (useAuth)
  "/help/getting-started",
  //#if (useLocalIdentity)
  "/help/getting-started/account",
  //#endif
  "/help/getting-started/signing-in",
  //#endif
  "/help/attachments",
  //#if (useAuth)
  "/help/admin",
  //#endif
  "/help/faq",
  "/help/contact",
  "/help/policies",
  "/help/terms-of-use",
  "/help/accessibility",
  "/help/privacy",
];

test.describe("Accessibility (WCAG 2.2 AA)", () => {
  for (const path of publicPages) {
    test(`${path} has no detectable WCAG violations`, async (
      { page },
      testInfo,
    ) => {
      await page.goto(path);

      // Wait for the route's own content, not just the shell, so the scan sees
      // the finished page rather than a loading state.
      await expect(page).toHaveURL(new RegExp(`${escapeRegExp(path)}$`));
      await expect(page.getByRole("heading").first()).toBeVisible();

      await checkAccessibility(page, testInfo);
    });
  }

  //#if (useAuth)
  test("the unauthenticated redirect lands on an accessible page", async (
    { page },
    testInfo,
  ) => {
    await page.goto("/");

    await expect(page).toHaveURL(/\/login/);
    await expect(page.getByRole("heading").first()).toBeVisible();

    await checkAccessibility(page, testInfo);
  });
  //#endif
});

import AxeBuilder from "@axe-core/playwright";
import { expect, type Page, type TestInfo } from "@playwright/test";

/**
 * Accessibility conformance target for this project: **WCAG 2.2 Level AA**.
 *
 * axe-core tags are cumulative — a WCAG 2.2 AA audit is the union of the 2.0, 2.1
 * and 2.2 A/AA tags, so all of them are listed. Section 508 is added because the
 * US federal standard maps onto WCAG 2.0 AA; the ADA Title II web rule
 * (28 CFR Part 35, April 2024) requires WCAG 2.1 AA for state and local
 * government — both are subsets of what is asserted here, so a clean run
 * satisfies ADA, Section 508 and WCAG 2.2 AA at once.
 *
 * Only automated checks run here. axe covers roughly a third to a half of WCAG
 * success criteria (contrast, names/roles/values, structure, target size, ...);
 * keyboard operability, focus order, meaningful alt text and reading level still
 * need a manual review before go-live.
 *
 * @see https://www.w3.org/TR/WCAG22/
 * @see https://www.ada.gov/resources/web-guidance/
 * @see https://github.com/dequelabs/axe-core/blob/develop/doc/rule-descriptions.md
 */
export const WCAG_TAGS = [
  "wcag2a",
  "wcag2aa",
  "wcag21a",
  "wcag21aa",
  "wcag22aa",
  "section508",
];

/**
 * Elements the Vite dev server injects for its own tooling. They are not part of
 * the application, never reach a production build, and the DevTools launcher in
 * particular is a `div` with `aria-label` and no role — a genuine WCAG 4.1.2
 * failure, but not one you can fix here.
 */
const DEV_TOOLING_SELECTORS = [
  "#__vue-devtools-container__", // vite-plugin-vue-devtools launcher + panel
  "#vue-inspector-container", // vite-plugin-vue-devtools component inspector
  "vite-plugin-checker-error-overlay", // vite-plugin-checker type-error overlay
];

export interface AccessibilityOptions {
  /**
   * Rule ids to skip for this page only — for a documented, tracked exception
   * (e.g. a third-party widget you cannot change). Prefer fixing the markup.
   */
  disableRules?: string[];
  /** CSS selectors to leave out of the scan, e.g. an embedded map. */
  exclude?: string[];
}

/**
 * Runs an axe-core scan of the current page against {@link WCAG_TAGS}, attaches
 * the full results to the Playwright report, and fails the test with a readable
 * list of violations (rule, impact, help link, offending elements).
 *
 * @example
 * test("home page is accessible", async ({ page }, testInfo) => {
 *   await page.goto("/");
 *   await expect(page.getByRole("heading", { level: 1 })).toBeVisible();
 *   await checkAccessibility(page, testInfo);
 * });
 */
export async function checkAccessibility(
  page: Page,
  testInfo: TestInfo,
  options: AccessibilityOptions = {},
): Promise<void> {
  let builder = new AxeBuilder({ page }).withTags(WCAG_TAGS);
  if (options.disableRules?.length) {
    builder = builder.disableRules(options.disableRules);
  }
  for (const selector of [...DEV_TOOLING_SELECTORS, ...(options.exclude ?? [])]) {
    builder = builder.exclude(selector);
  }

  const results = await builder.analyze();

  // The raw scan sits next to the test in the HTML report, so a failure can be
  // inspected without re-running, and a pass still shows what was covered.
  await testInfo.attach("accessibility-scan-results", {
    body: JSON.stringify(
      {
        url: results.url,
        timestamp: results.timestamp,
        tags: WCAG_TAGS,
        passes: results.passes.length,
        incomplete: results.incomplete.map((r) => r.id),
        violations: results.violations,
      },
      null,
      2,
    ),
    contentType: "application/json",
  });

  // Asserting on the count keeps the failure output to the formatted list below
  // rather than a JSON diff of the whole result set.
  expect(results.violations.length, formatViolations(results.violations)).toBe(
    0,
  );
}

type Violations = Awaited<
  ReturnType<AxeBuilder["analyze"]>
>["violations"];

function formatViolations(violations: Violations): string {
  if (violations.length === 0) return "No accessibility violations.";

  const lines = [
    `${violations.length} accessibility rule(s) violated (WCAG 2.2 AA / Section 508):`,
    "",
  ];
  for (const v of violations) {
    lines.push(`• [${v.impact ?? "n/a"}] ${v.id} — ${v.help}`);
    lines.push(`  ${v.helpUrl}`);
    lines.push(`  tags: ${v.tags.filter((t) => t.startsWith("wcag") || t === "section508").join(", ")}`);
    for (const node of v.nodes.slice(0, 5)) {
      lines.push(`  - ${node.target.join(" ")}`);
      if (node.failureSummary) {
        lines.push(
          ...node.failureSummary
            .split("\n")
            .filter(Boolean)
            .map((l) => `      ${l.trim()}`),
        );
      }
    }
    if (v.nodes.length > 5) {
      lines.push(`  … and ${v.nodes.length - 5} more element(s)`);
    }
    lines.push("");
  }
  return lines.join("\n");
}

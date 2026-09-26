# StarterApp UI (front end)

The Vue 3 single-page app. Part of [StarterApp](../../README.md).

---

## What's in here

- **Vue 3** (Composition API, `<script setup>`) + **TypeScript**, built with **Vite**.
- **Pinia** for state, **Vue Router** with route guards.
- **Axios** client with a single base URL and centralised error-message mapping.
- **vee-validate** for form validation, with wrapper components in `src/components`.
- **Bulma** for styling, with **Font Awesome Free** icons. The app shell (`AppHeader`,
  `AppFooter`, `AccountMenu`) and form fields (`vee-*`) are plain components in
  `src/components` — restyle or replace them freely.
- **Playwright** end-to-end tests in `e2e/`, including a **WCAG 2.2 AA accessibility
  audit** (axe-core) that CI runs on every pull request, push and release — see below.
- A **help center** under `/help` — see below.

---

## Prerequisites

- **Node.js 20.19+ or 22.12+** (Vite 7's floor) and **pnpm**

> **On a network that inspects TLS** (common on corporate networks), Node rejects the intercepted
> certificate with `SELF_SIGNED_CERT_IN_CHAIN` on any package it has to download. Node does
> not read the Windows certificate store by default — opt in with
> `NODE_OPTIONS=--use-system-ca` (Node 22.15+/24+) rather than disabling `strict-ssl`.

---

## Running it

```bash
cd src/ui
cp .env.example .env.local     # then edit as needed
pnpm install --frozen-lockfile
pnpm dev
```

The app runs at **`http://localhost:3000`** and proxies `/api` to the back end at
`https://localhost:7170` (override with `VITE_DEV_API_PROXY`). Start the
[API](../api/README.md) first, or the proxied calls have nothing to reach.

> **Local HTTPS is opt-in.** Drop a `key.pem` / `cert.pem` pair into `ssl/` and the dev
> server switches to HTTPS automatically. Without them it serves plain HTTP, which is why
> `playwright.config.ts` targets `http://localhost:3000`.

---

## Editing the UI

**Open `src/ui` directly in VS Code.** That is where this half of the solution is best worked
on — the Vue language tooling (Vue - Official/Volar), ESLint, and Vite's HMR all key off the
folder, and `pnpm dev` belongs in a terminal next to them. Visual Studio can edit these files
but gives you nothing extra for them.

**In Visual Studio, the `src/ui` solution folder is not the whole SPA.** It lists a handful of
top-level config files and nothing else — solution folders are flat, hand-maintained lists
with no notion of a directory on disk, so they cannot show a tree and do not pick up files you
add. To browse everything, use Solution Explorer's **Switch Views** button (the one at the top
of the pane) and choose **Folder View**, which shows the real directory contents and updates
as files come and go.

> **A UI file missing from the solution is cosmetic only.** Nothing in the build reads the
> solution for this folder: Vite compiles from disk, Playwright globs `e2e/` from disk, and CI
> runs `pnpm`. Adding a new `.vue` file to the solution folder changes the tree and nothing
> else — there is no step you have to remember.

---

## Scripts

| Script | Description |
| ------ | ----------- |
| `pnpm dev` | Start the Vite dev server on port 3000 |
| `pnpm build` | Production build into `dist/` |
| `pnpm build:dev` / `pnpm build:prod` | Build with the `dev` / `prod` env mode |
| `pnpm typecheck` | Run `vue-tsc` type checking |
| `pnpm lint:fix` | Lint and auto-fix `.ts` / `.vue` files |
| `pnpm e2e` | Run the Playwright end-to-end and accessibility tests |

---

## End-to-end and accessibility tests

`e2e/` holds a Playwright suite that CI runs in `.github/workflows/e2e.yml` on every pull
request, every push to `main` / `production`, and every published release:

| File | What it covers |
| ---- | -------------- |
| `e2e/smoke.spec.ts` | The app shell loads, the auth redirect works, the help center is reachable. |
| `e2e/login.spec.ts` | The sign-in form renders and accepts input (JWT projects only). |
| `e2e/accessibility.spec.ts` | Every public page passes an axe-core audit against **WCAG 2.2 Level AA** (plus Section 508). |
| `e2e/helpers/accessibility.ts` | The audit itself: the rule set, the dev-tooling exclusions, and the failure formatting. |

**The conformance target is WCAG 2.2 AA**, which is a superset of what the ADA Title II
web rule (WCAG 2.1 AA, 28 CFR Part 35) and Section 508 (WCAG 2.0 AA) require. A failing
test prints the rule, its impact, a Deque University link, and the offending elements;
the HTML report attaches the raw axe scan to every accessibility test, pass or fail.

Running locally — Playwright starts the dev server for you and reuses one that is already
running:

```bash
pnpm exec playwright install chromium   # once
pnpm e2e                                # report opens on failure; `pnpm exec playwright show-report` any time
```

Three things to know:

- **Add your pages to `publicPages` in `accessibility.spec.ts` as you build them.** Pages
  behind login need an authenticated session first — see
  [Playwright's authentication guide](https://playwright.dev/docs/auth) — then the same
  `checkAccessibility(page, testInfo)` call. The shipped tests deliberately need no API,
  which is why the workflow does not start one; when yours do, add a step to `e2e.yml`
  that runs the API (it needs PostgreSQL, e.g. a `postgres:17` service container) and
  point `VITE_DEV_API_PROXY` at it.
- **Automated checks are evidence, not a conformance claim.** axe-core detects roughly a
  third to a half of WCAG criteria: contrast, names/roles/values, document structure,
  target size. Keyboard operability, focus order, alternative-text quality, and plain
  language still need a manual review before go-live.
- **Exceptions are per page and explicit.** `checkAccessibility` accepts `disableRules`
  and `exclude` for a third-party widget you genuinely cannot fix. Prefer fixing the
  markup; if you must skip, leave a comment saying why and who owns it.

CI also writes a pass/fail table to the run's Summary tab and uploads the
`playwright-report` artifact (screenshots, traces on retry, and the axe results). On a
release the same report is attached to the GitHub Release as a `.zip`.

---

## Help center

A static help section at `/help`, made of three files plus one route list:

| File | What it holds |
| ---- | ------------- |
| `src/assets/help-content.ts` | The copy for every topic, keyed by route path. |
| `src/assets/help-sidebar.ts` | The left-hand menu — one entry per topic. |
| `src/views/Help.vue` | Renders whichever topic matches the current path. |
| `src/router/index.ts` | The list of `/help/*` paths, near the bottom. |

Adding a topic means editing all four: a path in the router, an entry in the sidebar, and
a page in the content file. An unrecognised `/help/*` path falls back to the overview, so
a stale link lands somewhere useful instead of on a blank page.

Two things to know before you go live:

- **The shipped copy is placeholder text** describing the template, not your application.
  The Terms of Use, Accessibility, and Privacy pages in particular must be replaced with
  wording your organisation has approved.
- **Help pages are public on purpose** — they carry no auth guard, because someone who
  cannot sign in still needs to read them. Do not put anything sensitive in them.

---

## Security defaults worth knowing

- **`public/web.config` carries real policy, not boilerplate.** It holds the SPA's
  Content-Security-Policy, the cache split between `index.html` (never cached) and hashed
  `/assets` (cached forever), and an outbound rule that adds `HttpOnly; Secure; SameSite=Lax`
  to any `Set-Cookie`.
- **Edit the CSP as you add integrations.** Every host in it is annotated with what needs
  it. Adding `'unsafe-inline'` or `'unsafe-eval'` to `script-src` — or introducing
  `script-src-elem`, which overrides `script-src` — undoes most of its value.
- **`v-html` is used in exactly one place, and only for content you author.**
  `SimplePage.vue` renders the help topics from `src/assets/help-content.ts`, which is a
  source file, never API data or user input. Nothing else uses it: `modal-spinner.vue`
  takes **plain text** and renders it through `{{ }}` (pass `\n` for line breaks, and use
  its `details` prop for label/value rows), and table print/export escapes every cell
  (`useTableExport.ts`). Keep it that way — API error strings and user-entered values
  reach both, and anything dynamic in a help page must go through `{{ }}`.
- **API secrets never belong in `VITE_*` variables.** Vite substitutes them into the
  browser bundle at build time, so anything placed there is public. Proxy third-party
  services through an API endpoint and keep the credential in server-side configuration.

---

## Notes & conventions

- **Bulma is pinned to 0.9.x.** Do not upgrade to 1.x without a full SCSS migration — it
  breaks the color system and the build.
- A **`pnpm-lock.yaml` ships with the project** — install with `pnpm install --frozen-lockfile`
  so builds are reproducible.
- **Build-script approvals are listed twice in `pnpm-workspace.yaml`**, as `allowBuilds`
  (pnpm 11) and `onlyBuiltDependencies` (pnpm 10). Each version ignores the other's key
  *silently*, and the symptom is not an install error — esbuild's postinstall is skipped and
  the first `vite build` fails. Add new entries to both lists.
- **`App.vue` and `Home.vue` compute their variable parts in `<script setup>`** and keep the
  `<template>` free of build-time conditionals. That was a template-authoring constraint, not
  a Vue best practice — once generated, this is ordinary code and you should feel free to
  restructure it however you like.

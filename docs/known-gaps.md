# Known gaps

- **`verify.ps1` needs `.ui-deps/node_modules` to exist** before it will type-check or lint
  the generated UI — it junctions that folder into each generated project rather than
  installing per case. When it is absent the UI checks are skipped and the case still
  reports `ok` (it now prints a warning). Bootstrap it once — see
  [authoring.md](authoring.md#bootstrapping-the-ui-checks).
- **Playwright e2e tests are not exercised by `verify.ps1`.** They run in CI instead — the
  `e2e` job in `release.yml` generates a project per auth mode and runs its suite (smoke +
  WCAG 2.2 AA audit) — and can be run by hand against a generated project; see
  [authoring.md](authoring.md#running-the-e2e-suite-locally). They are not in `verify.ps1`
  because Playwright's `webServer` runs `pnpm dev`, and pnpm 11 re-verifies dependencies
  before running a script: through the junctioned `node_modules` that rewrites
  `.ui-deps/node_modules/.modules.yaml` to point at the generated project, and every later
  `pnpm install` in `.ui-deps` refuses to run until you reinstall.
- **The accessibility audit covers only what axe-core can detect.** Roughly a third to a
  half of WCAG 2.2 AA criteria are automatable (contrast, names/roles/values, structure,
  target size). Keyboard operability, focus order, meaningful alternative text and reading
  level still need a manual review before a project goes live.
- **`Microsoft.OpenApi` 2.4.1 arrives transitively with a known advisory (NU1903)**, via
  Swashbuckle. Pin a patched version once one is available.

# Authoring and maintenance

How to change the template itself. For generating projects from it, see
[using-the-template.md](using-the-template.md).

---

## Verify before you commit

```powershell
pwsh verify.ps1
```

This generates, builds, and tests a matrix of flag combinations. **It is the only real
check.** Building `content/StarterApp` directly proves nothing: the `#if` symbols
are undefined outside the template engine, so the compiler silently strips every
conditional branch and you get a false pass.

### Bootstrapping the UI checks

`verify.ps1` type-checks and lints the generated SPA by junctioning a prebuilt
`node_modules` into each generated `src/ui`. Without it those checks are skipped, so do
this once (it needs npm registry access):

```powershell
$ui = 'content\StarterApp\src\ui'
New-Item -ItemType Directory -Force .ui-deps | Out-Null
Copy-Item "$ui\package.json","$ui\pnpm-lock.yaml","$ui\pnpm-workspace.yaml" .ui-deps
cd .ui-deps; pnpm install --frozen-lockfile
```

Refresh it whenever `package.json` or the lockfile changes. Two traps make this more
particular than it looks:

- **Never run `pnpm install` inside `content/StarterApp/src/ui`.** The template engine
  enumerates every file under the source directory *before* applying `exclude`, so a
  `node_modules` there takes `dotnet new` from ~2s to ~90s — and `verify.ps1` pays that
  cost once per case. (`node_modules` is excluded from generated output regardless, so the
  damage is purely speed.)
- **Never move an existing `node_modules` into `.ui-deps`.** On Windows pnpm links packages
  with *absolute-path* junctions, so a move leaves every one dangling and the `.bin` shims
  fail with `MODULE_NOT_FOUND`. Install it where it will live.

On a network that intercepts TLS, prefix with `$env:NODE_OPTIONS='--use-system-ca'` — Node
does not read the Windows certificate store by default.

---

## Releasing

Releases are automatic. `.github/workflows/release.yml` runs on every push to `main` that
touches `content/`, the `.csproj`, `verify.ps1`, or the workflow itself, and:

1. **Computes the next version** (`.github/scripts/Get-NextVersion.ps1`, details below).
2. **Verifies** — `verify.ps1 -SkipTests` on a Windows runner, every flag combination,
   including the UI type-check and lint against a bootstrapped `.ui-deps`.
3. **Packs** the `.nupkg` with that version, installs it, and generates a project from it
   as a smoke test.
4. **Runs the generated project's Playwright suite** — one job per auth mode (`jwt`,
   `entra`, `none`), each generating a project from the packed `.nupkg`, installing its UI,
   and running `pnpm e2e`: the smoke tests plus the WCAG 2.2 AA / ADA accessibility audit
   in `src/ui/e2e/accessibility.spec.ts`. Each run writes a pass/fail table to the job
   summary and uploads `playwright-report-<mode>.zip` as an artifact.
5. **Releases** — pushes the `vX.Y.Z` tag, creates a GitHub Release with auto-generated
   notes, the `.nupkg`, and the three Playwright reports attached, and pushes the package
   to the organisation's GitHub Packages NuGet feed. A failing e2e job blocks the release.

Pull requests run steps 1–4 only, so a PR proves the template still packs, generates, and
starts accessible before it can be merged.

### Running the e2e suite locally

`verify.ps1` does not run Playwright (see [known-gaps.md](known-gaps.md) for why). To run
it against a generated project without a per-project `pnpm install`, start Vite with
`node` rather than `pnpm dev` — running any pnpm script through the junctioned
`node_modules` makes pnpm 11 relink it and breaks `.ui-deps` — and let Playwright reuse
the running server:

```powershell
dotnet new install .\content\StarterApp --force
dotnet new crv-cleanarch -n Acme.Portal -o $env:TEMP\e2e --no-restore      # add --auth entra / none to cover the other page lists
New-Item -ItemType Junction -Path $env:TEMP\e2e\src\ui\node_modules -Target (Resolve-Path .ui-deps\node_modules)
cd $env:TEMP\e2e\src\ui
Start-Process node -ArgumentList 'node_modules/vite/bin/vite.js --port 3000'
node node_modules/@playwright/test/cli.js test          # add --reporter=list for terminal output only
```

If the browser is missing, `node node_modules/@playwright/test/cli.js install chromium`
downloads it once. The HTML report is in `playwright-report/`; each accessibility test
carries the raw axe-core scan as an attachment.

The audit excludes the elements Vite's dev tooling injects (`#__vue-devtools-container__`
and friends — see `e2e/helpers/accessibility.ts`), because the DevTools launcher is itself
a WCAG 4.1.2 failure that never reaches a production build.

### How the version is decided

**Git tags are the record of what shipped.** The next version is the latest `vX.Y.Z` tag
bumped according to the commits since it, using conventional-commit subjects:

| Commits since the last tag include…                                                 | Bump  | Example       |
| ----------------------------------------------------------------------------------- | ----- | ------------- |
| `feat!:` / `fix(api)!:` — a `!` before the colon — or a body with `BREAKING CHANGE` | major | 1.4.2 → 2.0.0 |
| `feat:` or `feat(scope):`                                                           | minor | 1.4.2 → 1.5.0 |
| anything else (`fix:`, `docs:`, `chore:`, plain messages…)                          | patch | 1.4.2 → 1.4.3 |

Squash-merging PRs keeps this honest: the squash title is the one subject that decides the
bump, so write it as `feat: …` / `fix: …` / `feat!: …` accordingly.

Three things worth knowing:

- **The `.csproj` `<Version>` is not bumped by CI** and never needs a commit. It seeds the
  very first release (when no `v*` tag exists) and is otherwise ignored; the workflow
  passes the computed version with `-p:Version`. This is deliberate — a bot commit per
  release would re-trigger the workflow and clutter history.
- **Override the bump** when the commit messages under-state a change: run the workflow
  from the Actions tab (*Run workflow*) and pick `major`, `minor`, or `patch`.
- **Skip a release** for a push that should not ship by putting `[skip release]` in the
  commit message. The verification still runs.

To see what the next push would produce, run the script locally on `main`:

```powershell
pwsh .github/scripts/Get-NextVersion.ps1
```

---

## Writing conditionals

Which comment style the engine honours depends on the file type:

| File type                     | Syntax                                                     |
| ----------------------------- | ---------------------------------------------------------- |
| `.cs`                         | `#if (symbol)` / `#else` / `#endif`                        |
| `.sln`                        | `#if (symbol)` … `#endif` — bare, no comment prefix        |
| `.json`                       | `//#if (symbol)` … `//#endif`                              |
| `.ts`                         | `//#if (symbol)` … `//#endif`                              |
| `.slnx`, `.csproj`, other XML | `<!--#if (symbol) -->` … `<!--#endif -->`                  |
| `.md`                         | `<!--#if (symbol) -->` … `<!--#endif -->`                  |

**Markdown conditionals must sit between blocks, not inside one.** The marker lines vanish
from the output, but they are still HTML comments in the template source — and a comment
inside a table body or a list ends that block when GitHub renders `content/StarterApp`
itself. Gate whole paragraphs, rows-plus-heading, or sentences on their own line. For an
ordered list whose items are conditional, number every item `1.` so the surviving ones
renumber correctly whichever combination is generated.
| `.vue`                        | `//#if (symbol)` … `//#endif` — **`<script>` blocks only** |

**`.vue` templates are the sharp edge.** The engine processes a `.vue` file with C-style
line comments, so `//#if` works inside `<script setup>` but `<!--#if -->` inside
`<template>` is passed through untouched — leaving literal comment markers in the output and
markup that references variables the script no longer defines. Neither
`customOperations` nor `specialCustomOperations` changed this behaviour when tried.

So: **keep `<template>` blocks free of build-time conditionals.** Compute the variable parts
in `<script setup>` and bind them:

- Conditional markup → a separate component, imported behind `//#if`, rendered via
  `<component :is="x" v-if="x" />` (see `App.vue` and `AccountMenu.vue`).
- Conditional text → a `computed` whose branches are behind `//#if` (see `Home.vue`).
- Conditional lists → build the array in the script and `v-for` over it.

---

## Adding a new optional slice

1. Add a `bool` symbol to `symbols` in `.template.config/template.json`.
2. Add a short name for it in `dotnetcli.host.json` and a label in `ide.host.json`.
3. List the slice's files under a `(!yourSymbol)` `exclude` in `sources[0].modifiers`.
4. Wrap its DI registrations, `DbSet`s, and `using` directives in `#if (yourSymbol)`.
5. If the slice adds a **project**, add it to `StarterApp.slnx` *and* `StarterApp.sln` —
   including a GUID in `guids` in `template.json`, and entries in the `.sln`'s
   `ProjectConfigurationPlatforms` and `NestedProjects` sections. Miss the `.sln` and the
   project is invisible in Visual Studio; miss `NestedProjects` and it lands outside
   `src/api`.
6. Add a case to `verify.ps1` and run it. Note it exercises the CLI host, so it only ever
   builds the `.slnx` — the `.sln` is not covered and needs an eyeball in VS.

**`auth` drives three symbols, not one.** `useAuth` is *any* authentication and also gates
the local user/role/company store, which both authenticated modes share. `useLocalIdentity`
is specifically the password surface — registration, login, reset, the API-issued JWT, the
rate limiter that protected those anonymous endpoints. `useEntra` is the Entra wiring.

When adding an authenticated feature, ask which of the two it belongs to: anything that
touches a password, a token the API mints, or an anonymous auth endpoint is
`useLocalIdentity`; everything else is `useAuth`. Getting this wrong compiles cleanly under
`--auth jwt` and fails only under `--auth entra`, which is why both are in `verify.ps1`.

**Keep `.tf`, `.yml` and `Dockerfile` free of `#if` entirely.** They are not in the table
above, and the engine's behaviour for them is untested here — gate them at the directory
level with an `exclude` in `sources[0].modifiers` instead, as `--infra` does for
`terraform/**` and `.github/**`.

**`verify.ps1` only format-checks Terraform.** `terraform validate` requires `init`, and
`init` resolves the Fargate module from a *placeholder* registry source (`your-org`) that
does not resolve until a generated project points it at a real module. To schema-check the AWS resources after editing `ui.tf`,
copy an env directory to a scratch folder, delete `api.tf`, strip the `cloud {}` block from
`variables.tf`, then `terraform init -backend=false && terraform validate`.

**Not every project belongs in both solutions.** A project whose SDK ships with Visual
Studio rather than the .NET SDK — `docker-compose.dcproj` and
`Microsoft.VisualStudio.Docker.Sdk` is the case in hand — must go in `StarterApp.sln`
*only*. Putting it in the `.slnx` breaks `dotnet build` for every CLI user with an
SDK-not-found error, and `verify.ps1` would fail on the very case that added it.

Watch for **`using` directives that only resolve when a slice is present** — if every file
in `Domain/Entities` is excluded, the namespace stops existing and an unguarded
`using StarterApp.Domain.Entities;` fails to compile. That is why the `using` blocks in the
two `DependencyInjection.cs` files and `AppDbContext.cs` are themselves conditional.

Also avoid putting the `sourceName` inside a **type name**. A project generated as
`Acme.Portal` would turn `class StarterAppApiFactory` into `class Acme.PortalApiFactory`,
which is not a valid identifier. Namespaces are fine — dots are legal there.

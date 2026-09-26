# Using the template

`StarterApp` is the `sourceName`: the engine rewrites that string in file names, folder
names, namespaces, and file contents to whatever `-n` you pass.

---

## Installing

Every push to `main` publishes a versioned package — see the
[Releases](https://github.com/cirinocarvalho/CRV.CleanArchitecture.Templates/releases)
page. Download the `.nupkg` attached to the release you want and install it:

```bash
dotnet new install .\CRV.CleanArchitecture.Templates.1.2.3.nupkg
```

The same package is on the organisation's GitHub Packages NuGet feed. GitHub requires a
token even to *read* that feed, so this route suits CI more than a laptop:

```bash
dotnet new install CRV.CleanArchitecture.Templates --nuget-source https://nuget.pkg.github.com/cirinocarvalho/index.json
```

Upgrading later: `dotnet new update` picks up newer versions from any feed you installed
from; a downloaded `.nupkg` is upgraded by installing the newer file.

While iterating on the template itself, install from source instead:

```bash
dotnet new install .\content\StarterApp --force
```

…or pack it locally (this produces the fallback `1.0.0` version — real version numbers are
assigned by the release workflow, see [authoring.md](authoring.md#releasing)):

```bash
dotnet pack .\CRV.CleanArchitecture.Templates.csproj -o .\artifacts
dotnet new install .\artifacts\CRV.CleanArchitecture.Templates.1.0.0.nupkg
```

Then:

```bash
dotnet new crv-cleanarch -n Acme.Portal
```

In Visual Studio the template appears in **New Project** as _Clean Architecture API + Vue
SPA_, with the options below rendered as checkboxes and a dropdown.

To uninstall: `dotnet new uninstall CRV.CleanArchitecture.Templates` (or the source path
you installed from).

---

## Options

| CLI flag                  | Default | What it controls                                                                                                                                                                                                                                                                                                                                           |
| ------------------------- | ------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `--auth jwt\|entra\|none` | `jwt`   | `jwt`: Identity + API-issued JWT, registration and passwords. `entra`: Microsoft Entra ID signs in, local store keeps roles/profile. `none`: no authentication.                                                                                                                                                                                            |
| `--ui true\|false`        | `true`  | The Vue 3 + TypeScript + Vite SPA under `src/ui`, its Playwright suite (smoke + WCAG 2.2 AA accessibility audit), and `.github/workflows/e2e.yml`, which runs that suite on PRs, pushes to `main`/`production`, and releases.                                                                                                                              |
| `--tests true\|false`     | `true`  | The xUnit unit-test and integration-test projects.                                                                                                                                                                                                                                                                                                         |
| `--samples true\|false`   | `true`  | A worked `Product` slice — entity, DTOs, service, controller, unit tests — demonstrating the layering.                                                                                                                                                                                                                                                     |
| `--docker true\|false`    | `false` | A `Dockerfile`, `.dockerignore`, `docker-compose.yml` / `.override.yml`, and the Visual Studio `docker-compose.dcproj`. Registered in `.sln` only — see below.                                                                                                                                                                                             |
| `--infra true\|false`     | `false` | Terraform for `dev` and `prod` (ECR + Fargate API, S3 + CloudFront SPA) plus the GitHub Actions workflows that deploy them; the UI deploy runs the e2e workflow first and stops on failure. Also `release.yml` and `Get-NextVersion.ps1`: pushing to `production` versions, deploys, tags `vX.Y.Z` and publishes a GitHub Release. **Implies `--docker`**. |

Example:

```bash
dotnet new crv-cleanarch -n Acme.Portal --auth none --ui false --samples false
```

Two things that used to be optional now ship with every project, so there is no flag for
them:

- **The file upload slice** — file storage, upload metadata persistence, the attachments
  endpoint, and the matching UI screen. It was `--file-upload` and defaulted to off.
- **The help center** — a static `/help` section under `src/ui`, wired into the top nav
  and the footer. Its copy is placeholder text; see the
  [UI README](../content/StarterApp/src/ui/README.md) for what to replace before going
  live.

Delete either one from the generated project if you don't want it.

---

## Visual Studio behaviour

`tags` carries **both** `"type": "solution"` and `"editorTreatAs": "solution"`. Both are
required. With only `type`, Visual Studio falls back to treating the template as a _project_
template: it shows the **Place solution and project in the same directory** checkbox — an
affordance solution templates do not have — and builds a flat solution of its own. Every
`type: solution` template Microsoft ships (`aspire-starter`, `aspire-empty`,
`aspire-ts-cs-starter`) sets both; every `type: project` template sets neither.

---

## Why two solution files ship

`content/StarterApp` contains **both** `StarterApp.slnx` and `StarterApp.sln`, and a
`hostIdentifier` modifier in `sources` drops one of them so exactly one ever reaches the
output:

| Host          | Gets    | Why                                               |
| ------------- | ------- | ------------------------------------------------- |
| `dotnetcli`   | `.slnx` | The modern format; what the repo standardised on. |
| Visual Studio | `.sln`  | VS's template host does not recognise `.slnx`.    |

They must be kept in sync by hand — both list the projects and both declare the `src/api`
and `src/ui` solution folders.

**One project is deliberately not in sync:** `docker-compose.dcproj` (from `--docker true`)
is registered in `StarterApp.sln` only. It builds on `Microsoft.VisualStudio.Docker.Sdk`,
which ships with Visual Studio rather than the .NET SDK, so a CLI `dotnet build` of a
solution containing it fails with an SDK-not-found error. The Docker files themselves are
generated either way — only the VS project wrapper is solution-scoped, and `docker compose`
does not need it.

Emitting both would leave `dotnet build` with two candidate solutions in one directory and
no way to choose, so the exclusion is not optional. `verify.ps1` builds `<Name>.slnx`
because it runs as the CLI host.

The solution folders are the whole reason the `.sln` exists. Visual Studio otherwise
composes a flat solution from `primaryOutputs`, which has no syntax for solution folders —
that is what produces a bare list of six projects instead of the `src/api` + `src/ui`
grouping. Microsoft's Aspire templates accept that flat result and simply drop their `.sln`
for non-CLI hosts; this template ships one instead so the grouping survives.

Conditionals in `.sln` use bare `#if (symbol)` / `#endif` lines with no comment prefix —
the one file type in this template that works that way. Project GUIDs are listed in
`guids` in `template.json` so the engine reissues them per generated solution.

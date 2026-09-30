---
name: clean-architecture-template
description: "Maintain and extend the CRV.CleanArchitecture.Templates dotnet-new template — a .NET Clean Architecture Web API (JWT auth, API versioning, Swagger, Serilog, EF Core) with an optional Vue 3 + TypeScript + Vite SPA, driven by template.json boolean/choice symbols and #if conditionals. USE FOR: editing files under content/StarterApp, adding or removing an optional slice/symbol, writing or fixing #if conditionals in .cs/.json/.ts/.vue/.csproj/.slnx files, running or extending verify.ps1, packing/installing the template, debugging a generated project that fails to build. DO NOT USE FOR: unrelated .NET/Vue app development that has nothing to do with this template repo."
license: MIT
metadata:
  author: Cirino Carvalho
  version: "1.0.0"
---

# Clean Architecture Template Skill

This skill captures the structure and authoring rules for the `CRV.CleanArchitecture.Templates`
NuGet package: a `dotnet new` template (short name `crv-cleanarch`) that scaffolds a Clean
Architecture Web API with an optional Vue SPA. Read this before editing anything under
`content/StarterApp` or `.template.config`.

## Repo layout

```text
CRV.CleanArchitecture.Templates.csproj   # packs content/** into the NuGet template package
verify.ps1                                 # THE only real check — see below
content/StarterApp/                        # sourceName; engine rewrites "StarterApp" everywhere
├── .template.config/
│   ├── template.json        # symbols, exclude rules, primaryOutputs, postActions
│   ├── ide.host.json         # Visual Studio New Project dialog labels
│   └── dotnetcli.host.json   # CLI short names for symbols
└── src/
    ├── api/
    │   ├── StarterApp.Domain          # entities, no dependencies
    │   ├── StarterApp.Application     # DTOs, interfaces, services, DependencyInjection.cs
    │   ├── StarterApp.Infrastructure   # EF Core, Identity, external services
    │   ├── StarterApp.Api              # controllers, Program.cs, middleware
    │   ├── StarterApp.UnitTests
    │   └── StarterApp.IntegrationTests
    └── ui/                              # Vue 3 + TS + Vite + Pinia + Vue Router + Axios
```

Layer dependency direction: `Api → Application → Domain`, with `Infrastructure` implementing
`Application` interfaces. Never add a reference that points the other way.

## Template symbols (defined in `.template.config/template.json`)

| Symbol                        | CLI flag                  | Type   | Default / value                   | Gates                                                                       |
| ----------------------------- | ------------------------- | ------ | --------------------------------- | --------------------------------------------------------------------------- |
| `authMode`                    | `--auth jwt\|entra\|none` | choice | `jwt`                             | Which auth is scaffolded — drives the three computed auth symbols below     |
| `includeUi`                   | `--ui`                    | bool   | `true`                            | `src/ui/**` and the UI workflows                                            |
| `includeTests`                | `--tests`                 | bool   | `true`                            | UnitTests + IntegrationTests projects                                       |
| `includeSamples`              | `--samples`               | bool   | `true`                            | The `Product` worked example slice                                          |
| `includeDocker`               | `--docker`                | bool   | `false`                           | Dockerfile, docker-compose files, `.dcproj` (via `useDocker`)               |
| `includeInfra`                | `--infra`                 | bool   | `false`                           | `terraform/**`, deploy/release workflows, `.github/scripts/**`; implies Docker |
| `skipRestore`                 | `--no-restore`            | bool   | `false`                           | Skips the post-create restore                                               |
| `useAuth` (computed)          | —                         | bool   | `authMode != "none"`              | Identity, roles, admin, companies, account emails, login UI                 |
| `useLocalIdentity` (computed) | —                         | bool   | `authMode == "jwt"`               | Locally issued JWT branches (`#if` only — no exclude block)                 |
| `useEntra` (computed)         | —                         | bool   | `authMode == "entra"`             | `EntraClaimsTransformation`; excludes the local register/password/token files |
| `useDocker` (computed)        | —                         | bool   | `includeDocker \|\| includeInfra` | Docker files                                                                |

Every optional file lives under a `condition` + `exclude` block in `sources[0].modifiers` in
`template.json`. If a file is exclusive to a slice, it MUST be listed there, or `--auth none`
(etc.) will still ship it uncompilable.

## Writing `#if` conditionals — syntax by file type

| File type                     | Syntax                                                     |
| ----------------------------- | ---------------------------------------------------------- |
| `.cs`                         | `#if (symbol)` / `#else` / `#endif`                        |
| `.json`                       | `//#if (symbol)` … `//#endif`                              |
| `.ts`                         | `//#if (symbol)` … `//#endif`                              |
| `.slnx`, `.csproj`, other XML | `<!--#if (symbol) -->` … `<!--#endif -->`                  |
| `.vue`                        | `//#if (symbol)` … `//#endif` — **`<script>` blocks only** |

**`.vue` sharp edge:** the engine treats `.vue` as C-style comments, so `//#if` works inside
`<script setup>` but `<!--#if -->` inside `<template>` is left untouched — producing literal
comment markers and markup referencing variables the script no longer defines. Neither
`customOperations` nor `specialCustomOperations` fixes this. **Keep `<template>` free of
build-time conditionals**:

- Conditional markup → a separate component, imported behind `//#if`, rendered via
  `<component :is="x" v-if="x" />` (see `App.vue`, `AccountMenu.vue`).
- Conditional text → a `computed` whose branches are behind `//#if` (see `Home.vue`).
- Conditional lists → build the array in `<script setup>` and `v-for` over it.

## Adding a new optional slice

1. Add a `bool` symbol under `symbols` in `.template.config/template.json`.
2. Add a CLI short name in `dotnetcli.host.json` and a label in `ide.host.json`.
3. List every slice-exclusive file under a new `(!yourSymbol)` `exclude` block in
   `sources[0].modifiers`.
4. Wrap the slice's DI registrations, `DbSet`s, and `using` directives in `#if (yourSymbol)`
   in both `DependencyInjection.cs` files and `AppDbContext.cs`.
5. Add a case to `verify.ps1`'s `$cases` array and run it (see below).

**Watch for `using` directives that only resolve when the slice is present** — if every file
in a namespace (e.g. `Domain/Entities`) is excluded, an unguarded `using` fails to compile.
That's why the `using` blocks in `DependencyInjection.cs` and `AppDbContext.cs` are themselves
conditional.

**Never put `sourceName` inside a type name.** A project generated as `Acme.Portal` turns
`class StarterAppApiFactory` into `class Acme.PortalApiFactory` — not a valid identifier.
Namespaces are fine (dots are legal there); type/class names are not.

## Verification — mandatory before committing

```powershell
pwsh verify.ps1
pwsh verify.ps1 -SkipTests
```

This is **the only real check**. It installs the template from source, then for a matrix of
flag combinations it generates a project, builds it, runs tests, and (if a sibling
`node_modules` exists) type-checks and lints the Vue UI. Building `content/StarterApp`
directly proves nothing — the `#if` symbols are undefined outside the template engine, so the
compiler silently strips every conditional branch and gives a false pass. Vue/TS type errors
after a template change are the usual symptom of a botched conditional: markup surviving while
the script block that fed it was stripped.

## Manual iteration

```bash
dotnet new install .\content\StarterApp --force
dotnet new crv-cleanarch -n Acme.Portal --auth none --ui false --samples false
dotnet new uninstall CRV.CleanArchitecture.Templates   # or the source path installed from
```

Packing the NuGet package:

```bash
dotnet pack .\CRV.CleanArchitecture.Templates.csproj -o .\artifacts
dotnet new install .\artifacts\CRV.CleanArchitecture.Templates.1.0.0.nupkg
```

## Known gaps (do not silently "fix" without flagging to the user)

- After changing UI dependencies, regenerate the lockfile (`cd src/ui && pnpm install
  --lockfile-only`) and commit it under `content/StarterApp/src/ui/`.
- Playwright e2e tests are not exercised by `verify.ps1`. They run in CI (`release.yml`'s
  `e2e` job, one generated project per auth mode) and can be run by hand — see
  `docs/authoring.md#running-the-e2e-suite-locally`. Never run `pnpm dev`/`pnpm e2e` through
  the junctioned `node_modules`: pnpm 11 relinks it and breaks `.ui-deps`.
- The suite's accessibility audit (`src/ui/e2e/accessibility.spec.ts`, axe-core, WCAG 2.2 AA
  + Section 508) scans only the pages a fresh project serves without signing in; pages behind
  login need an authenticated session added to the spec. axe covers only the automatable
  criteria — it is evidence, not a conformance claim.
- `Microsoft.OpenApi` 2.4.1 arrives transitively via Swashbuckle with advisory NU1903 — pin a
  patched version once available.

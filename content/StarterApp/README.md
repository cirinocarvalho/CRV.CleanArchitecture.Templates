# StarterApp

![built with .NET 10](https://img.shields.io/badge/built%20with-.NET%2010-512BD4)
![data EF Core + PostgreSQL](https://img.shields.io/badge/data-EF%20Core%20%2B%20PostgreSQL-4169E1)
<!--#if (useLocalIdentity) -->
![auth JWT + ASP.NET Identity](https://img.shields.io/badge/auth-JWT%20%2B%20ASP.NET%20Identity-512BD4)
<!--#endif -->
<!--#if (useEntra) -->
![auth Microsoft Entra ID](https://img.shields.io/badge/auth-Microsoft%20Entra%20ID-0078D4)
<!--#endif -->
<!--#if (includeUi) -->
![front end Vue 3 + TypeScript](https://img.shields.io/badge/front%20end-Vue%203%20%2B%20TypeScript-42B883)
![bundled with Vite](https://img.shields.io/badge/bundled%20with-Vite-646CFF)
<!--#endif -->
<!--#if (useDocker) -->
![containerised with Docker](https://img.shields.io/badge/containerised%20with-Docker-2496ED)
<!--#endif -->
<!--#if (includeInfra) -->
![provisioned with Terraform](https://img.shields.io/badge/provisioned%20with-Terraform-7B42BC)
![deployed via GitHub Actions](https://img.shields.io/badge/deployed%20via-GitHub%20Actions-2088FF)
![hosted on AWS](https://img.shields.io/badge/hosted%20on-AWS-FF9900)
<!--#endif -->
[![license MIT](https://img.shields.io/badge/license-MIT-blue)](LICENSE)

<!--#if (includeUi) -->
A .NET 10 Clean Architecture Web API with a Vue 3 + TypeScript single-page front end,
generated from the **Clean Architecture API + Vue SPA** template.
<!--#endif -->
<!--#if (!includeUi) -->
A .NET 10 Clean Architecture Web API, generated from the **Clean Architecture API + Vue SPA**
template.
<!--#endif -->

```text
StarterApp/
├── src/
<!--#if (includeUi) -->
│   ├── api/          # .NET 10 Web API (Clean Architecture)
│   └── ui/           # Vue 3 + TypeScript + Vite SPA
<!--#endif -->
<!--#if (!includeUi) -->
│   └── api/          # .NET 10 Web API (Clean Architecture)
<!--#endif -->
<!--#if (includeInfra) -->
├── terraform/        # AWS infrastructure, dev and prod
├── .github/          # CI/CD: e2e + accessibility tests, API and UI deploys, releases
<!--#endif -->
<!--#if (includeUi && !includeInfra) -->
├── .github/          # CI: Playwright e2e + accessibility tests
<!--#endif -->
└── StarterApp.slnx
```

<!--#if (includeUi) -->
## UI Documentation (Front-end)

Documentation can be found in the [src/ui](src/ui/README.md) folder.

<!--#endif -->
## API Documentation (Back-end)

Documentation can be found in the [src/api](src/api/README.md) folder.

<!--#if (includeInfra) -->
## Infrastructure and Deployment

Documentation can be found in the [terraform](terraform/README.md) folder.

## Releasing

Two branches, two environments: `main` deploys to **dev**, and `production` cuts a
**release**, the only way production is deployed.

> **Automatic deploys are turned off for now.** The deploy and release workflows run only
> when started from the **Actions** tab (release from the `production` branch). To deploy
> on push again, uncomment the `push:` trigger at the top of `api_build_and_deploy.yml`,
> `ui_build_and_deploy.yml` and `release.yml`.

`.github/workflows/release.yml`, run on the `production` branch:

1. **Computes the next version** — `.github/scripts/Get-NextVersion.ps1`. The latest
   `vMAJOR.MINOR.PATCH` tag is the baseline, bumped by the
   [conventional-commit](https://www.conventionalcommits.org) subjects since it:
   `feat:` is a minor, a `!` before the colon or a `BREAKING CHANGE:` footer is a major,
   anything else is a patch. With no tag yet the first release ships the seed version
   as-is: `version` from `src/ui/package.json` when the SPA is part of the project,
   otherwise `0.1.0`.
2. **Deploys** the API and the UI through `api_build_and_deploy.yml` and
   `ui_build_and_deploy.yml`, passing that version so it is stamped into both — the API
   assembly (`-p:Version`) and its immutable ECR image tag, and `VITE_APP_VERSION` in the
   UI bundle, which the help center prints at the foot of every topic. The UI deploy runs
   the Playwright + WCAG 2.2 AA suite first, so a failing suite blocks the release.
3. **Tags and publishes** — only once both deploys succeed: `vX.Y.Z` is pushed and a
   GitHub Release is created with generated notes and the accessibility report attached.
   Tagging last means a failed deploy leaves no tag behind, and the next attempt computes
   the same version.

Run it from the **Actions** tab to override the bump (`major` for a 2.0), or to redeploy
the current release: with no commits since the last tag nothing is tagged and both sides
are rebuilt at the version they already carry. Dev builds are stamped with the
pre-release version they would become, e.g. `1.4.0-dev.a1b2c3d`.

To see what the next release would be, run the script locally:

```powershell
pwsh .github/scripts/Get-NextVersion.ps1
```

<!--#endif -->
## Getting started

<!--#if (includeUi) -->
Run the two sides separately — the API first, since the dev server proxies to it:

```bash
cd src/api/StarterApp.Api && dotnet run     # https://localhost:7170
cd src/ui && pnpm install --frozen-lockfile && pnpm dev   # http://localhost:3000
```

Prerequisites, configuration, secrets and database setup are in the two READMEs above.
<!--#endif -->
<!--#if (!includeUi) -->
```bash
cd src/api/StarterApp.Api
dotnet run     # https://localhost:7170, Swagger at /swagger
```

Prerequisites, configuration, secrets and database setup are in the
[API README](src/api/README.md).
<!--#endif -->

## Where to start changing things

<!--#if (includeUi) -->
1. **Rename the app in the UI shell** — `app-title` and `app-subtitle` in
   `src/ui/src/App.vue`.
<!--#endif -->
<!--#if (includeSamples) -->
1. **Add your first entity** — follow the sample `Product` slice, which spans
   `Domain/Entities/Product.cs`, `Application/DTOs/ProductDtos.cs`,
   `Application/Interfaces/IProductService.cs`, `Application/Services/ProductService.cs`,
   and `Api/Controllers/ProductsController.cs`. Register it in both
   `DependencyInjection.cs` files and `AppDbContext`.
1. **Delete the sample** once you no longer need it — the files above plus the
   `Products` `DbSet` are the whole slice.
<!--#endif -->
<!--#if (!includeSamples) -->
1. **Add your first entity** — an entity in `Domain`, its DTOs, interface and service in
   `Application`, a controller in `Api`, registered in both `DependencyInjection.cs` files
   and `AppDbContext`.
<!--#endif -->
<!--#if (includeUi) -->
1. **Replace the home page** — `src/ui/src/views/Home.vue`.
1. **Rewrite the help center** — the copy in `src/ui/src/assets/help-content.ts` is
   placeholder text describing the template, and the Terms of Use, Accessibility and
   Privacy pages must be replaced with your organisation's approved wording before you
   go live. The menu lives in `src/ui/src/assets/help-sidebar.ts`; a new topic needs an
   entry in all three of that file, `help-content.ts`, and the help route list in
   `src/ui/src/router/index.ts`.
<!--#endif -->

## License

Released under the MIT License — see [LICENSE](LICENSE). It arrives from the template with
the template author as the copyright holder; change the holder, the year, or the
licence itself to whatever your project actually needs.

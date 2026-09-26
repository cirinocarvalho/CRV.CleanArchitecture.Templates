# CRV Clean Architecture Templates

![packaged as dotnet new template](https://img.shields.io/badge/packaged%20as-dotnet%20new%20template-512BD4)
![built with .NET 10](https://img.shields.io/badge/built%20with-.NET%2010-512BD4)
![front end Vue 3](https://img.shields.io/badge/front%20end-Vue%203-42B883)
![data EF Core + PostgreSQL](https://img.shields.io/badge/data-EF%20Core%20%2B%20PostgreSQL-4169E1)
![auth JWT or Entra ID](https://img.shields.io/badge/auth-JWT%20or%20Entra%20ID-0078D4)
![tests xUnit + Playwright](https://img.shields.io/badge/tests-xUnit%20%2B%20Playwright-5A2D81)
![containerised with Docker](https://img.shields.io/badge/containerised%20with-Docker-2496ED)
![provisioned with Terraform](https://img.shields.io/badge/provisioned%20with-Terraform-7B42BC)
![deployed via GitHub Actions](https://img.shields.io/badge/deployed%20via-GitHub%20Actions-2088FF)
![hosted on AWS](https://img.shields.io/badge/hosted%20on-AWS-FF9900)
[![license MIT](https://img.shields.io/badge/license-MIT-blue)](LICENSE)

A `dotnet new` solution template for a .NET 10 Clean Architecture Web API with an
optional Vue 3 + TypeScript + Vite SPA, container image, and AWS infrastructure: the
architecture and plumbing of a production app, without its business logic. Visual Studio's **New Project** dialog picks it up once
installed, so it works from both the CLI and the IDE.

```text
CRV.CleanArchitecture.Templates/
├── CRV.CleanArchitecture.Templates.csproj     # packs the template into a NuGet package
├── verify.ps1                                 # generate + build + test every flag combination
├── .github/workflows/release.yml              # verify on PR; version, pack and release on push to main
├── docs/                                      # documentation for this repository
└── content/
    └── StarterApp/                            # the template itself
        ├── .template.config/                  # template.json, ide.host.json, dotnetcli.host.json
        ├── .github/workflows/                 # CI/CD, generated with --infra
        ├── terraform/                         # dev + prod root modules, generated with --infra
        └── src/                               # the skeleton that gets generated
```

Generation options — `--auth` (`jwt` / `entra` / `none`), `--ui`, `--tests`, `--samples`,
`--docker`, `--infra` — are documented in full below.

## Template Documentation (using it)

Installing, generating a project, every option flag, and how Visual Studio treats the
template: see [docs/using-the-template.md](docs/using-the-template.md).

## Authoring Documentation (maintaining it)

Verifying changes, writing conditionals per file type, and adding a new optional slice: see
[docs/authoring.md](docs/authoring.md).

## Generated Application Documentation

Every generated project ships its own documentation set, which is authored in
`content/StarterApp` and carries the same conditionals as the code — sections for a slice you
did not generate are not emitted:

| Ships as | Covers |
| --- | --- |
| [README.md](content/StarterApp/README.md) | Index: what was generated, quick start, where to start changing things. |
| [src/api/README.md](content/StarterApp/src/api/README.md) | Back end — stack, configuration and secrets, database, tests, Docker, Entra, security defaults. |
| [src/ui/README.md](content/StarterApp/src/ui/README.md) | Front end — prerequisites, dev server, scripts, CSP and security, conventions. |
| [terraform/README.md](content/StarterApp/terraform/README.md) | Infrastructure — AWS layout, pre-deploy checklist, repository secrets. |

## Known Gaps

Current limitations of the template: see [docs/known-gaps.md](docs/known-gaps.md).

[LICENSE](LICENSE)

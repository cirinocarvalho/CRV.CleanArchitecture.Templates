# StarterApp infrastructure and deployment

Terraform for AWS, plus the GitHub Actions workflows that deploy it. Part of
[StarterApp](../README.md).

---

## What's in here

`dev` and `prod` are separate root modules, each with its own Terraform Cloud workspace. The
API runs on ECS Fargate behind an `api/fargate` module you supply; the SPA is a **private** S3
bucket that only CloudFront can read, through an Origin Access Control.

The workflows live in [`.github/workflows`](../.github/workflows) and deploy on push, with
the branch selecting the environment — `main` → dev / `Test`, `production` → prod /
`Production`. On `main` each of the two deploy workflows triggers only on changes under its
own directory; `production` is deployed by `release.yml` alone, which runs both of them
with the release version and tags the repository afterwards (see
[Releasing](../README.md#releasing)). `skip ci` in a commit message bypasses all of them.

> **Automatic deploys are turned off for now** — the `push:` triggers are commented out,
> so every deploy is started by hand from the **Actions** tab. Uncomment them to restore
> the behaviour described above.

---

## Before the first deploy

| What | Why |
| --- | --- |
| An **ECR repository** named after the project | Terraform does not create it; the API workflow pushes to it and the task definition pulls from it. |
| **Terraform Cloud workspaces** `dev-starterapp` and `prod-starterapp` | Named in the `cloud {}` block, which cannot take variables — rename there if yours differ. |
| A **GitHub OIDC identity provider and deploy role** in the AWS account | The workflows assume a role rather than storing access keys. |
| `terraform.tfvars` from the `.example` next to it | Account IDs, VPC, certificate and zone IDs are deliberately empty in `variables.tf`. |

---

## Repository secrets and variables

| Name | Kind | Purpose |
| --- | --- | --- |
| `AWS_DEPLOY_ROLE_ARN` | secret | Role the workflows assume via OIDC. |
| `CF_DISTRIBUTION_ID_DEV` / `_PROD` | variable | CloudFront distributions to invalidate — the `ui_distribution_id` Terraform output. |
| `AWS_REGION` | variable | Optional; defaults to `us-east-1`. |

---

> **The Fargate module is a placeholder.** `app.terraform.io/your-org/api/fargate` does
> not resolve as shipped, so `terraform init` fails until you point it at your own module
> (and set `organization` in the `cloud` block) or replace it with raw `aws_ecs_*`
> resources. Everything in `ui.tf` is plain AWS provider resources and works anywhere.

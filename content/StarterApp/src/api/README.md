# StarterApp API (back end)

The .NET 10 Web API. Part of [StarterApp](../../README.md).

---

## What's in here

| Project | Holds |
| --- | --- |
| `StarterApp.Api` | HTTP layer: controllers, middleware, DI, configuration |
| `StarterApp.Application` | Use cases, application services, DTOs |
| `StarterApp.Domain` | Entities and domain logic (no dependencies) |
| `StarterApp.Infrastructure` | EF Core, identity, persistence, integrations |

Dependencies point inwards: Api → Application → Domain, with Infrastructure implementing the
interfaces Application declares.
<!--#if (includeTests) -->
`StarterApp.UnitTests` and `StarterApp.IntegrationTests` sit alongside them.
<!--#endif -->

- **API versioning** via URL segment (`/api/v1/...`), configured with `Asp.Versioning`.
- **Swagger / OpenAPI** UI, enabled in Development, one document per API version.
- **Entity Framework Core** against PostgreSQL (Npgsql), with generic repository and unit-of-work
  abstractions (`IRepositoryBase<T>`, `IReadRepositoryBase<T>`, `IUnitOfWork`).
- **Serilog** structured logging (console + file, compact JSON).
- Cross-cutting middleware: global exception handler, security headers, security-event
  logging, HTTPS redirection, CORS.
- **Health endpoints** — `/health` (liveness, runs no checks) and `/health/ready`
  (readiness, probes each `DbContext`). Both anonymous, both deliberately terse in what
  they disclose. See `StarterApp.Api/Extensions/HealthCheckExtensions.cs`.
<!--#if (useLocalIdentity) -->
- **JWT** bearer authentication on ASP.NET Identity, with roles, user and company
  administration, account activation, and password-reset emails.
- **Rate limiting** on the anonymous auth endpoints — 20 requests/minute per IP for login,
  register and reset-password; 5/minute for forgot-password, which sends email. Tune via the
  optional `RateLimitSettings` section; see `StarterApp.Api/Extensions/RateLimitingExtensions.cs`
  for the NAT and reverse-proxy caveats.
<!--#endif -->
<!--#if (useEntra) -->
- **Microsoft Entra ID** bearer authentication, with roles and profile kept in the local
  store — see [Microsoft Entra ID](#microsoft-entra-id) below.
<!--#endif -->

---

## Prerequisites

- **.NET 10 SDK**
- **PostgreSQL** 14+ — for development, one container is enough:

  ```bash
  docker run -d --name starterapp-db -e POSTGRES_DB=starterapp \
    -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:17
  ```

---

## Running it

```bash
cd StarterApp.Api
dotnet restore
dotnet run
```

The API listens on **`https://localhost:7170`**, with Swagger at
`https://localhost:7170/swagger` in Development.

Configuration lives in `appsettings.json`. Connection strings default to the local
container above (`localhost:5432`, database `starterapp`), so it runs as generated. **Provide real secrets through
[user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) or environment
variables — never by editing a committed file.** See `appsettings.Example.json` for the
full shape.

<!--#if (useLocalIdentity) -->
> **The JWT signing key in `appsettings.Development.json` is a placeholder that ships with
> the template — it is identical for everyone who generates a project and is not a secret.**
> It exists so the app runs locally with no setup. Before this app leaves your machine, set
> a real one:
>
> ```bash
> dotnet user-secrets set "JwtSettings:Secret" "<32+ character key>"
> ```
>
> Startup fails with an explicit message if the key is missing or shorter than 32 bytes.

<!--#endif -->
### Database

Both contexts share one PostgreSQL database: Identity tables in the `identity` schema,
application tables in `public`.

<!--#if (useAuth) -->
Create the Identity schema with EF Core, then the company tables with the SQL scripts
(the "None" company they seed is where new registrations are assigned):

```bash
cd src/api
dotnet tool restore
dotnet ef database update --context AppIdentityDbContext \
  --project StarterApp.Infrastructure --startup-project StarterApp.Api
psql "host=localhost dbname=starterapp user=postgres" \
  -f StarterApp.Infrastructure/Data/Scripts/App_CreateCompanyObjects.sql \
  -f StarterApp.Infrastructure/Data/Scripts/App_AddUserActivation.sql
```

Where schema changes are applied by hand, `Identity_CreateObjects.sql` creates the same
Identity schema as the migration and records it as applied. All three scripts are
idempotent.

<!--#endif -->
PostgreSQL compares text case-sensitively. Natural keys that people type — company
names, product SKUs — use the case-insensitive ICU collation `AppDbContext` defines
(`case_insensitive`); apply it with `UseCollation` to any column of your own that needs it.

<!--#if (includeTests) -->
---

## Testing

```bash
cd src/api
dotnet test
```

Integration tests boot the real API through `WebApplicationFactory` against EF Core's
in-memory provider, so they need no database.

<!--#endif -->
<!--#if (useDocker) -->
---

## Docker

From `src/api`:

```bash
docker compose build
docker compose up
docker compose port starterapp.api 8080   # the host port Docker assigned
```

The image runs as the non-root `app` user on port 8080 — since .NET 8 the ASP.NET images
no longer run as root, and a non-root user cannot bind port 80.
`docker-compose.override.yml` mounts your user secrets read-only, so the connection string
and signing key stay out of every image layer.

`docker-compose.override.yml` also starts a `postgres:17` container named `db` and points
the API at it (`Host=db`), so `docker compose up` needs no database of your own. It is
for local development only: the deploy workflow builds from `docker-compose.yml`, which
has no database in it.

`docker-compose.dcproj` is registered in `StarterApp.sln` (giving Visual Studio its
**Docker Compose** launch profile) but not in `StarterApp.slnx`, because its SDK ships with
Visual Studio and would break a CLI `dotnet build`.

<!--#endif -->
<!--#if (useEntra) -->
---

## Microsoft Entra ID

Entra proves **who** the caller is; the database still decides **what they may do**. Roles,
profile and company assignment stay in the local store, so the admin screens, the roles API
and every `[Authorize(Roles = "Admin")]` check work exactly as they would under JWT auth.

What Entra replaces is only the password surface: there is no registration, login,
forgot/reset/change-password endpoint, no API-issued JWT, and no rate limiter in front of
those anonymous endpoints.

### Wiring it up

1. Register the API in Entra, expose a scope (`access_as_user` by default), and note the
   tenant id, client id and application ID URI.
2. Fill in `AzureAd` in `appsettings.json` (`Instance`, `TenantId`, `ClientId`, `Audience`,
   `Scopes`). These are identifiers rather than secrets — the API validates tokens and never
   holds a client secret.
3. For the Swagger **Authorize** button, register a public client with
   `https://localhost:<port>/swagger/oauth2-redirect.html` as a redirect URI and set
   `AzureAd:SwaggerClientId`. It uses authorization code + PKCE.

### First sign-in and the first administrator

There is no registration step, so accounts are provisioned on first request by
[`EntraClaimsTransformation`](StarterApp.Infrastructure/Identity/EntraClaimsTransformation.cs):
it matches the Entra object id against `AspNetUserLogins`, adopts an existing account with
the same email if one is waiting, and otherwise creates the user — then republishes the
local id, email, name and roles as claims.

That leaves a bootstrap problem worth knowing about: **a newly provisioned user has no
roles, and only an Admin can grant them.** For the first administrator, sign in once to
create the row, then insert the `Admin` role assignment directly in the identity database.
After that the Users screen handles everyone else.

> Deactivating a user (`IsActive = false`) stops the projection: they keep authenticating
> against Entra but arrive with no local id and no roles, so authorised endpoints refuse
> them. Removing their access in Entra itself is still the stronger control.

<!--#endif -->
---

## Security defaults worth knowing

Already wired up. They cost nothing to keep and are awkward to retrofit, so prefer adjusting
them over removing them.

- **`StarterApp.Api/web.config` carries real policy, not boilerplate.** It strips the
  `Server` and `X-Powered-By` headers that IIS adds *after* the managed pipeline runs
  (middleware cannot remove them), blocks `TRACE`, hides `App_Data`, raises the IIS request
  limit to match the app's upload limit, and passes through the app's `ProblemDetails` bodies
  instead of IIS error pages.
- **Upload paths are validated twice** — every caller-supplied component is reduced to a safe
  file name, and the resolved path is then checked to be inside the storage root before the
  write opens. `StarterApp.IntegrationTests/Security/FileUploadPathTraversalTests.cs` pins
  this shut.
- **Secrets belong in user-secrets or environment variables**, never in a committed
  `appsettings.*.json`.

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Language

Respond in Brazilian Portuguese (pt-BR). Commit messages and code comments must also be in pt-BR (semantic prefixes: `feat`, `fix`, `docs`, `refactor`, `chore`, `test`).

## Stack

ASP.NET Core 10 (C# 14) REST API, EF Core 10 targeting **SQL Server** (`Microsoft.EntityFrameworkCore.SqlServer` + `UseSqlServer`). JWT auth via HTTP-only cookie (`access_token`), BCrypt.Net-Next for password hashing, FluentValidation for input validation, Scalar for OpenAPI docs.

> **Note:** `README.md` and `.github/copilot-instructions.md` describe an earlier PostgreSQL/Npgsql design (including a `snake_case` naming convention) that is no longer accurate — the project has since moved to SQL Server with default (Pascal/camel-case-ish) EF Core naming. Trust the code (`ServiceCollectionExtensions.cs`, `.env.example`, `compose.yaml`) over the prose docs for connection/database details.

## Commands

```bash
dotnet restore                          # restore dependencies
dotnet run                              # run locally (https://localhost:7209, http://localhost:5148)
dotnet build                            # build
dotnet test                             # run the test project (tests/CoeurApi.Tests)
```

Migrations (applied automatically on startup via `MigrateAsync()` in `Program.cs` — no manual step needed in prod):

```bash
dotnet ef migrations add <Name>         # create a migration after changing/adding an entity
dotnet ef migrations list
dotnet ef migrations remove             # revert the last migration
```

Docker/production, via `Taskfile.yaml` (requires `task` CLI):

```bash
task build      # docker compose build
task rebuild    # build --no-cache
task start      # up -d
task deploy      # build + start
task logs       # follow API logs
task shell      # shell into the api container
task migrate -- <Name>   # dotnet ef migrations add <Name> locally
```

Tests live in `tests/CoeurApi.Tests` (xUnit + Moq), a separate project referencing the main one via `ProjectReference` — kept out of the main `coeur-api.csproj` build (`Compile Remove="tests\**"`) so test dependencies never ship in the production artifact.

## Local setup

Local dev uses .NET User Secrets (not `.env`) for credentials:

```bash
dotnet user-secrets set "ConnectionStrings:Default" "Server=...;Database=...;UID=...;Password=...;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:Secret" "<32+ char secret>"
```

Production reads config from env vars (see `compose.yaml`): `ConnectionStrings__Default`, `Jwt__Secret`, `Jwt__Issuer`, `Jwt__Audience`, `Jwt__ExpirationHours`, `AllowedHosts`.

## Architecture: Modular Monolith

Single deployable process, organized into self-contained modules that communicate only through interfaces registered in DI — never through direct references to another module's internals.

```
App/
├── Core/          # Cross-cutting infra: DbContext, JWT auth, middleware, DI wiring, settings
│                   # Knows about ASP.NET Core / the web pipeline.
├── Shared/         # Interfaces (e.g. ICurrentUser, IUsersRepository) and exceptions any module
│                   # can depend on. Plain C#, no ASP.NET Core references — this is the seam
│                   # modules use to talk to each other without coupling to implementations.
└── Modules/        # Domains: Authentication, Users, Shopping
    └── <Module>/
        ├── Controllers/     # HTTP in, delegates to Service, returns response
        ├── DTOs/            # Input records + output records with a static FromEntity(...) mapper
        ├── Models/           # Entities: private setters, mutated only via entity methods,
        │                     # created only via a static Create(...) factory (never `new`);
        │                     # EF mapping via IEntityTypeConfiguration<T> in the same folder
        ├── Repositories/     # DB access through AppDbContext; implements an interface from Shared/
        ├── Validators/       # FluentValidation validators for DTOs
        ├── Services/         # One class per use case (e.g. CreateUserService, GetUserByIdService) —
        │                     # no grouped multi-method services. Each exposes a single ExecuteAsync(...),
        │                     # throws HttpException, and orchestrates repo + other deps (including other
        │                     # Services, e.g. GetOwnedShoppingListService injected wherever an ownership
        │                     # check needs to be reused). Registered individually in <Module>Module.cs.
        └── <Module>Module.cs # `Add<Module>Module()` extension registering the module's DI bindings
```

Request flow: `Controller → Service → Repository`, response bubbles back up through `Controller`. A controller injects one service per action (constructor gets `CreateUserService createUser, GetUserByIdService getUserById, ...`), never a single grouped service. Adding a module means repeating this anatomy and wiring `Add<Module>Module()` into `Program.cs`.

Module registration and cross-cutting setup live in `App/Core/Extensions/`:
- `ServiceCollectionExtensions.AddCore()` — DbContext, JWT bearer auth (reads the cookie via `OnMessageReceived`), CORS (`Frontend` policy), per-IP rate limiting on login, FluentValidation registrations, global `AuthorizeFilter` + `FluentValidationFilter` on all controllers.
- `WebApplicationExtensions.UseCore()` — middleware pipeline order: `UseExceptionHandler()` → CORS → Authentication → Authorization → RateLimiter.

## Error handling

Business errors are thrown as `HttpException` (`App/Shared/Exceptions/HttpException.cs`) via semantic factory methods (`HttpException.NotFound(...)`, `.Conflict(...)`, `.Forbidden(...)`, etc.) — only the status codes actually used exist as factories (`BadRequest`, `Unauthorized`, `Forbidden`, `NotFound`, `Conflict`, `TooManyRequests`, `NoContent`); add a new one only when a real use case needs it, don't pre-build the full HTTP status registry.

Error responses follow the ASP.NET Core standard, **Problem Details** (RFC 9457): `HttpExceptionHandler` (`App/Core/Middleware/HttpExceptionHandler.cs`), an `IExceptionHandler` registered via `AddExceptionHandler<HttpExceptionHandler>()` + `AddProblemDetails()`, converts every `HttpException` into a `ProblemDetails`/`ValidationProblemDetails` written through `IProblemDetailsService`. Any exception that isn't an `HttpException` falls through (returns `false`) to the framework's own default handler, which logs it and emits a generic 500 Problem Details — no custom catch-all logging needed. Shape:

```json
{ "type": "...", "title": "...", "status": 404, "detail": "Recurso não encontrado.", "toast": { "type": "warning", "message": "Recurso não encontrado." } }
```

`title`/`type` are filled in automatically by the framework from the status code (RFC reason phrase + section link) — don't set them manually. `errors` (per-field validation dictionary) appears as a top-level member (not an extension) whenever `HttpException.Errors` is set, matching the same `ValidationProblemDetails` shape ASP.NET Core's own `[ApiController]` model-binding validation already produces — so a malformed request body and a FluentValidation failure now return identically-shaped 400s.

`toast` is a custom `extensions` entry — the one deliberate deviation from the bare RFC 9457 shape, kept for the Angular frontend's HTTP interceptor. It's added by a single global `options.CustomizeProblemDetails` callback in `AddProblemDetails()` (`ServiceCollectionExtensions.AddCore()`), derived from `ProblemDetails.Status`/`.Detail` (5xx → `error`, 4xx → `warning`, else `info`; falls back to a fixed pt-BR message when `Detail` is empty, e.g. for the framework's generic 500). Because this hook runs for *every* Problem Details response — `HttpExceptionHandler`'s, the framework's own unhandled-exception 500, `[ApiController]`'s model-binding 400s, and the 429 rate-limit rejection (`RateLimiterOptions.OnRejected`) — none of those call sites need to set `toast` themselves; this is the single source of truth for it.

## Auth

JWT stored in an HTTP-only cookie (`access_token`), read out in `AddJwtBearer().Events.OnMessageReceived` rather than the `Authorization` header — so there is no bearer-token handling on the client. Every controller requires auth by default (global `AuthorizeFilter`); use `[AllowAnonymous]` to opt out (e.g. login, user creation). `ICurrentUser`/`CurrentUserService` (`App/Core/Authentication/`) exposes the authenticated user's id/email/name/role for ownership checks inside services (see the `id != currentUser.Id && !currentUser.IsAdmin` pattern in `GetUserByIdService`/`UpdateUserService`/`DeleteUserService`). No refresh tokens — expired token means re-login.

## API routes

All controller routes are prefixed `api/v1/...` (e.g. `[Route("api/v1/users")]`). Keep any new controller and any hardcoded `Location` header (`Created($"api/v1/...")`) consistent with this prefix.


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

Migrations (applied automatically on startup via `MigrateAsync()` in `Program.cs` — no manual step needed in prod). The `DbContext` lives in `src/Infrastructure` but the composition root (and `Microsoft.EntityFrameworkCore.Design`) is `src/Api`, so EF Core tooling always needs both flags:

```bash
dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/Api
dotnet ef migrations list --project src/Infrastructure --startup-project src/Api
dotnet ef migrations remove --project src/Infrastructure --startup-project src/Api
```

Docker/production, via `Taskfile.yaml` (requires `task` CLI):

```bash
task build      # docker compose build
task rebuild    # build --no-cache
task start      # up -d
task deploy      # build + start
task logs       # follow API logs
task shell      # shell into the api container
task migrate -- <Name>   # dotnet ef migrations add <Name> locally (src/Infrastructure, startup src/Api)
```

Tests live in `tests/CoeurApi.Tests` (xUnit + Moq), a separate project referencing the module projects it exercises (`Users.csproj`, `Authentication.csproj`, `Shopping.csproj`) via `ProjectReference` — it lives outside `src/`, so it never gets pulled into any module's or the host's build, and test dependencies never ship in the production artifact.

## Local setup

Local dev uses .NET User Secrets (not `.env`) for credentials:

```bash
dotnet user-secrets set "ConnectionStrings:Default" "Server=...;Database=...;UID=...;Password=...;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:Secret" "<32+ char secret>"
```

Production reads config from env vars (see `compose.yaml`): `ConnectionStrings__Default`, `Jwt__Secret`, `Jwt__Issuer`, `Jwt__Audience`, `Jwt__ExpirationHours`, `AllowedHosts`.

## Architecture: Clean Architecture + Modular Monolith (multi-project)

Single deployable process (one host, one process at runtime), but module boundaries are enforced by **project references**, not just folders — each module is its own `.csproj` and can only reach another module by referencing its project directly (never through the host or through Infrastructure).

```
src/
├── SharedKernel/        # Zero project dependencies. HttpException, PagedResult/Pagination,
│                        # IUnitOfWork, UserRole (a cross-cutting authz concept, not Users-owned
│                        # business data — this is the one deliberately shared domain concept).
├── Application/         # → SharedKernel only. Cross-module contracts: ICurrentUser.
├── Infrastructure/      # → SharedKernel, Application, modules/Users, modules/Shopping (NOT
│                        # Authentication — its JWT bits are self-contained, see below).
│                        # AppDbContext (composes DbSets from every module's Domain), EF Core
│                        # migrations, CurrentUserService (ICurrentUser impl), AddInfrastructure().
├── Api/                 # → everything. Host: Program.cs, appsettings*.json, ServiceCollection/
│                        # WebApplication extensions (AddApiServices/UseApiServices), the
│                        # HttpExceptionHandler middleware, FluentValidationFilter, StatusPage.
└── modules/
    └── <Module>/<Module>.csproj   # One project per module (Users, Authentication, Shopping).
        ├── Domain/              # Entities: private setters, mutated only via entity methods,
        │                        # created only via a static Create(...) factory (never `new`);
        │                        # EF mapping via IEntityTypeConfiguration<T> co-located here.
        ├── Application/
        │   ├── Abstractions/    # The module's own repository interfaces (e.g. IUsersRepository,
        │   │                    # IProductRepository) — every module owns its ports; nothing
        │   │                    # module-specific lives in the shared SharedKernel/Application.
        │   ├── Services/        # One class per use case (e.g. CreateUserService, GetUserByIdService) —
        │   │                    # no grouped multi-method services. Each exposes a single ExecuteAsync(...),
        │   │                    # throws HttpException, and orchestrates repo + other deps (including other
        │   │                    # Services, e.g. GetOwnedShoppingListService injected wherever an ownership
        │   │                    # check needs to be reused).
        │   ├── DTOs/            # Input records + output records with a static FromEntity(...) mapper
        │   └── Validators/      # FluentValidation validators for DTOs
        ├── Infrastructure/      # Repository implementations (depend on the generic EF Core `DbContext`
        │                        # base type + `Set<T>()`, never the concrete AppDbContext — that's what
        │                        # keeps a module from having to reference the Infrastructure project and
        │                        # forming a dependency cycle); JWT/token internals for Authentication.
        ├── Presentation/        # Controllers: HTTP in, delegates to Service, returns response
        └── <Module>Module.cs    # `Add<Module>Module()` extension: registers the module's DI bindings
                                  # AND its own `AddValidatorsFromAssemblyContaining<T>()` scan — a
                                  # module's validators are never registered from outside the module.
```

Modules form a DAG, not a flat set: `Users` is a leaf (depends on nothing but SharedKernel/Application); `Authentication` and `Shopping` both reference `Users.csproj` directly because they need its `User` entity (`ShoppingList.Owner`, `LoginService` manipulating `User` via `IUsersRepository`) — this is an intentional, one-directional exception to "modules never see each other's internals," scoped to the one entity that's genuinely shared. `SharedKernel` and `Application` never depend on any module.

Request flow: `Controller → Service → Repository`, response bubbles back up through `Controller`. A controller injects one service per action (constructor gets `CreateUserService createUser, GetUserByIdService getUserById, ...`), never a single grouped service. Adding a module means creating a new `src/modules/<Module>/<Module>.csproj` with this same anatomy, wiring `Add<Module>Module()` into `Program.cs`, and adding the project to `coeur-api.slnx`.

Host wiring lives in `src/Api/Extensions/`:
- `ServiceCollectionExtensions.AddApiServices(configuration)` — ProblemDetails/toast customization, ForwardedHeaders, CORS (`Frontend` policy), per-IP rate limiting on login, controllers + global `AuthorizeFilter`/`FluentValidationFilter`, authorization. Deliberately thin — it knows nothing about any module's DTOs; each module registers its own validators.
- `WebApplicationExtensions.UseApiServices()` — middleware pipeline order: `UseExceptionHandler()` → CORS → Authentication → Authorization → RateLimiter.
- `src/Infrastructure/DependencyInjection.AddInfrastructure(configuration)` — `AppDbContext` + `IUnitOfWork` + a bare `DbContext` registration (so module repositories can depend on the abstract type), `ICurrentUser`.
- `src/modules/Authentication/AuthModule.AddAuthModule(configuration)` — also configures the JWT bearer scheme (reads the cookie via `OnMessageReceived`) and binds `JwtSettings`, since those are Authentication-module-owned, not host-owned.

## Error handling

Business errors are thrown as `HttpException` (`src/SharedKernel/Exceptions/HttpException.cs`) via semantic factory methods (`HttpException.NotFound(...)`, `.Conflict(...)`, `.Forbidden(...)`, etc.) — only the status codes actually used exist as factories (`BadRequest`, `Unauthorized`, `Forbidden`, `NotFound`, `Conflict`, `TooManyRequests`, `NoContent`); add a new one only when a real use case needs it, don't pre-build the full HTTP status registry.

Error responses follow the ASP.NET Core standard, **Problem Details** (RFC 9457): `HttpExceptionHandler` (`src/Api/Middleware/HttpExceptionHandler.cs`), an `IExceptionHandler` registered via `AddExceptionHandler<HttpExceptionHandler>()` + `AddProblemDetails()`, converts every `HttpException` into a `ProblemDetails`/`ValidationProblemDetails` written through `IProblemDetailsService`. Any exception that isn't an `HttpException` falls through (returns `false`) to the framework's own default handler, which logs it and emits a generic 500 Problem Details — no custom catch-all logging needed. Shape:

```json
{ "type": "...", "title": "...", "status": 404, "detail": "Recurso não encontrado.", "toast": { "type": "warning", "message": "Recurso não encontrado." } }
```

`title`/`type` are filled in automatically by the framework from the status code (RFC reason phrase + section link) — don't set them manually. `errors` (per-field validation dictionary) appears as a top-level member (not an extension) whenever `HttpException.Errors` is set, matching the same `ValidationProblemDetails` shape ASP.NET Core's own `[ApiController]` model-binding validation already produces — so a malformed request body and a FluentValidation failure now return identically-shaped 400s.

`toast` is a custom `extensions` entry — the one deliberate deviation from the bare RFC 9457 shape, kept for the Angular frontend's HTTP interceptor. It's added by a single global `options.CustomizeProblemDetails` callback in `AddProblemDetails()` (`ServiceCollectionExtensions.AddApiServices()`, `src/Api/Extensions/`), derived from `ProblemDetails.Status`/`.Detail` (5xx → `error`, 4xx → `warning`, else `info`; falls back to a fixed pt-BR message when `Detail` is empty, e.g. for the framework's generic 500). Because this hook runs for *every* Problem Details response — `HttpExceptionHandler`'s, the framework's own unhandled-exception 500, `[ApiController]`'s model-binding 400s, and the 429 rate-limit rejection (`RateLimiterOptions.OnRejected`) — none of those call sites need to set `toast` themselves; this is the single source of truth for it.

## API documentation

No XML doc comments (`<GenerateDocumentationFile>` is off) and no third-party Swagger attributes — `builder.Services.AddOpenApi()` (built-in `Microsoft.AspNetCore.OpenApi`, served at `/openapi/v1.json` and rendered by Scalar) infers routes/parameters/schemas from the code, and every action documents its own possible responses with `[ProducesResponseType<T>(StatusCodes.Status...)]`, one attribute per status the action can actually return — not a generic catch-all set. Derive the list from what the action's service(s) can throw (`HttpException.NotFound` → 404, `.Conflict` → 409, `.Forbidden` → 403, etc.), whether the action requires auth (no `[AllowAnonymous]` → 401; `[Authorize(Roles = ...)]` → also 403), and whether it binds a `[FromBody]` DTO (→ 400 `ValidationProblemDetails`, since `[ApiController]` can reject malformed bodies even without a registered FluentValidation validator). Use `ProblemDetails`/`ValidationProblemDetails` as the error response types, matching what `HttpExceptionHandler` actually writes. When adding a new action, add its `[ProducesResponseType]` set the same way — don't skip it.

## Auth

JWT stored in an HTTP-only cookie (`access_token`), read out in `AddJwtBearer().Events.OnMessageReceived` rather than the `Authorization` header — so there is no bearer-token handling on the client. This bearer scheme, plus `JwtSettings`/`TokenService`, are wired inside `src/modules/Authentication/AuthModule.cs` (`src/modules/Authentication/Infrastructure/`) — Authentication is the only module that touches JWT internals. Every controller requires auth by default (global `AuthorizeFilter`); use `[AllowAnonymous]` to opt out (e.g. login, user creation). `ICurrentUser` (`src/Application/Abstractions/`) / `CurrentUserService` (`src/Infrastructure/Authentication/`) exposes the authenticated user's id/email/name/role for ownership checks inside services (see the `id != currentUser.Id && !currentUser.IsAdmin` pattern in `GetUserByIdService`/`UpdateUserService`/`DeleteUserService`). No refresh tokens — expired token means re-login.

## API routes

All controller routes are prefixed `api/v1/...` (e.g. `[Route("api/v1/users")]`). Keep any new controller and any hardcoded `Location` header (`Created($"api/v1/...")`) consistent with this prefix.


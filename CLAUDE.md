# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Language

Respond in Brazilian Portuguese (pt-BR). Commit messages and code comments must also be in pt-BR (semantic prefixes: `feat`, `fix`, `docs`, `refactor`, `chore`, `test`).

## Stack

ASP.NET Core 10 (C# 14) REST API, EF Core 10 targeting **PostgreSQL 17** (`Npgsql.EntityFrameworkCore.PostgreSQL` + `UseNpgsql`). JWT auth via HTTP-only cookie (`access_token`), BCrypt.Net-Next for password hashing, FluentValidation for input validation, Scalar for OpenAPI docs.

> **Note:** `.github/copilot-instructions.md` predates the multi-project module split (it still describes an `App/Core`/`App/Shared`/`App/Modules` folder layout instead of `src/Modules/<Module>/<Module>.<Layer>`) and claims a `snake_case` naming convention (`UseSnakeCaseNamingConvention`) that isn't actually configured — columns use EF Core's default naming; only table/schema names are set explicitly per module. Its mention of PostgreSQL/Npgsql as the database is accurate again now that the project has moved back from SQL Server. `README.md` was updated alongside this file and reflects the current architecture; trust the code (`DependencyInjection.cs`, `.env.example`, `docker-compose.yml`) over `copilot-instructions.md` for connection/database and naming details.

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

Docker/production, via the Visual Studio-generated Compose project (`docker-compose.dcproj`, `docker-compose.yml` + `docker-compose.override.yml`) — run through Visual Studio's "Docker Compose" launch profile, or directly with the CLI:

```bash
docker compose build              # build the image
docker compose build --no-cache   # rebuild without cache
docker compose up -d              # start the stack in the background
docker compose logs -f api        # follow API logs
docker compose exec api /bin/sh   # shell into the api container
docker compose down               # tear down the stack
```

Tests live in `tests/CoeurApi.Tests` (xUnit + Moq), a separate project referencing the specific layer projects it exercises (`Users.Domain`/`Users.Application`, `Authentication.Application`/`Authentication.Infrastructure`, `Shopping.Domain`/`Shopping.Application`) via `ProjectReference` — it lives outside `src/`, so it never gets pulled into any module's or the host's build, and test dependencies never ship in the production artifact.

## Local setup

Local dev uses .NET User Secrets (not `.env`) for credentials:

```bash
dotnet user-secrets set "ConnectionStrings:Default" "Host=...;Port=5432;Database=...;Username=...;Password=..."
dotnet user-secrets set "Jwt:Secret" "<32+ char secret>"
```

Production reads config from env vars (see `docker-compose.yml`): `ConnectionStrings__Default`, `Jwt__Secret`, `Jwt__Issuer`, `Jwt__Audience`, `Jwt__ExpirationHours`, `AllowedHosts`.

## Architecture: Clean Architecture + Modular Monolith (multi-project)

Single deployable process (one host, one process at runtime), but boundaries are enforced by **project references**, not just folders — at *two* levels: a module can only reach another module by referencing its project directly, and **within** a module, each Clean Architecture layer (`Domain`, `Application`, `Infrastructure`, `Presentation`) is itself its own `.csproj`, not a folder. `Users.Presentation` physically cannot `using` a type from `Users.Infrastructure` unless someone adds a `<ProjectReference>` for it — and adding that reference is itself the wrong move, a build-time signal that the layering rule was about to be broken.

**Why both patterns together, not just one:**
- **Clean Architecture** (inside each module) enforces the dependency rule at the layer level: `<Module>.Domain` has zero outward dependencies (beyond `SharedKernel` when it needs a shared concept like `UserRole`), `<Module>.Application` depends only on its own `Domain` + its own `Abstractions` (never on a concrete EF Core repository or the concrete `AppDbContext`), and `<Module>.Infrastructure`/`<Module>.Presentation` are the only projects allowed to know about EF Core/ASP.NET Core concretes. This is what makes a module's business logic testable without a database and swappable at the persistence layer without touching `Application`/`Domain`.
- **The compiler enforces it, not code review** — at both levels. A folder-only "modular monolith" (modules as namespaces, layers as folders, all in one project) relies entirely on discipline; here, `Users.Domain` trying to reach into `Users.Infrastructure`, or `Users` trying to reach into `Shopping.Domain`, is a build error, not a lint warning. This is the direct payoff of splitting layers into their own `.csproj`: coupling that used to be "a convention someone could quietly violate inside the one `Users.csproj`" is now something MSBuild refuses to compile.
- Combining them gives one deployable artifact (simple ops — one process, one DB connection, no network hop between "services") while keeping the option to peel a module (or even just its `Infrastructure`) out into its own service later: since each layer already only talks to the rest of the system through interfaces and its own `.csproj` boundary, extraction is a deployment change, not a rewrite.

```
src/
├── SharedKernel/        # Zero project dependencies. HttpException, PagedResult/Pagination,
│                        # IUnitOfWork, UserRole (a cross-cutting authz concept, not Users-owned
│                        # business data — this is the one deliberately shared domain concept).
├── Application/         # → SharedKernel only. Cross-module contracts: ICurrentUser.
├── Infrastructure/      # → SharedKernel, Application, and both the Domain and Infrastructure
│                        # projects of Users and Shopping (Domain for the entity types in
│                        # AppDbContext's DbSet<T>s, Infrastructure for the IEntityTypeConfiguration<T>
│                        # classes under each module's Persistence/Configurations/ folder). Never a module's
│                        # Application or Presentation. NOT Authentication — its JWT bits are
│                        # self-contained, see below.
│                        # AppDbContext (composes DbSets from every module's Domain), EF Core
│                        # migrations, CurrentUserService (ICurrentUser impl), AddInfrastructure().
├── Api/                 # → everything. Host: Program.cs, appsettings*.json, ServiceCollection/
│                        # WebApplication extensions (AddApiServices/UseApiServices), the
│                        # HttpExceptionHandler middleware, FluentValidationFilter, StatusPage.
└── Modules/
    └── <Module>/                        # e.g. Users, Authentication, Shopping, Finances
        ├── <Module>.Domain/<Module>.Domain.csproj              # → SharedKernel (if needed)
        │   # Entities: private setters, mutated only via entity methods, created only via a
        │   # static Create(...) factory (never `new`). Zero EF Core dependency — no package
        │   # reference at all; EF mapping (IEntityTypeConfiguration<T>) lives in the module's
        │   # Infrastructure project instead (see Persistence/Configurations/ below), keeping Domain framework-
        │   # agnostic. Authentication has no Domain project — it has no entity of its own, only Users'.
        ├── <Module>.Application/<Module>.Application.csproj    # → <Module>.Domain, SharedKernel
        │   ├── Abstractions/    # The module's own ports: repository interfaces (IUsersRepository,
        │   │                    # IProductRepository) AND anything else Infrastructure implements
        │   │                    # concretely (e.g. Authentication's ITokenService, implemented by
        │   │                    # the JWT-based TokenService in Authentication.Infrastructure) —
        │   │                    # Application never references a concrete Infrastructure type.
        │   ├── UseCases/         # One class per use case (never a shared/reusable "service").
        │   │   └── <Group>/      # Only present when a module has more than one entity (Shopping's
        │   │       │             # Products/ShoppingLists); a single-entity module (Users) skips this
        │   │       │             # level and nests <Verb>/ directly under UseCases/. Files shared by
        │   │       │             # multiple use cases in the group — the Response record(s), and any
        │   │       │             # cross-cutting interface/extension (e.g. Shopping's IProductFields,
        │   │       │             # ProductFieldsRules) — sit here, at the group root, not duplicated
        │   │       │             # into every sibling folder.
        │   │       └── <Verb>/   # e.g. Create, Update, Delete, GetById, GetAll, AddItem. Holds
        │   │                     # <Verb><Entity>UseCase.cs, <Verb><Entity>Request.cs (FluentValidation
        │   │                     # input, only when the use case takes one), and <Verb><Entity>Validator.cs
        │   │                     # (only when a Request exists) — colocated per use case instead of
        │   │                     # split across Services/DTOs/Validators folders. The namespace mirrors
        │   │                     # the physical path exactly (`...UseCases.Products.Create`, etc.) —
        │   │                     # each verb folder is its own namespace, and callers add one `using`
        │   │                     # per use case they inject alongside a `using` for the group's shared
        │   │                     # namespace (for the Response type). Response records keep a static
        │   │                     # FromEntity(...) mapper.
        │   └── Settings/        # Config POCOs a Presentation controller also needs to read directly
        │                        # (e.g. Authentication's JwtSettings, read by AuthController for the
        │                        # cookie expiration) — lives here, not in Infrastructure, specifically
        │                        # so Presentation doesn't need a reference to Infrastructure just to
        │                        # see a plain settings class.
        ├── <Module>.Infrastructure/<Module>.Infrastructure.csproj   # → <Module>.Application only
        │   │                    # (Domain arrives transitively). Repository implementations depend
        │   │                    # on the generic EF Core `DbContext` base type + `Set<T>()`, never the
        │   │                    # concrete AppDbContext. Also hosts `Add<Module>Module()` — the module's
        │   │                    # composition root (mirrors root `AddInfrastructure()` in src/Infrastructure):
        │   │                    # registers repositories, services, and its own
        │   │                    # `AddValidatorsFromAssemblyContaining<T>()` scan (never done from outside
        │   │                    # the module). Authentication's JWT bearer scheme setup lives here too.
        │   ├── Persistence/
        │   │   └── Configurations/   # IEntityTypeConfiguration<T> classes (e.g. UserConfiguration), one
        │   │                    # per entity declared in this module's Domain — not in Domain itself, so
        │   │                    # Domain stays free of any EF Core package reference. Root Infrastructure
        │   │                    # references this project (in addition to Domain) specifically so
        │   │                    # AppDbContext can `ApplyConfigurationsFromAssembly` against it.
        └── <Module>.Presentation/<Module>.Presentation.csproj       # → <Module>.Application only,
                                   # + the root Application.csproj directly whenever a controller
                                   # injects ICurrentUser itself (MeController, ShoppingListsController).
                                   # Controllers: HTTP in, delegates to Service, returns response.
                                   # NEVER references <Module>.Infrastructure — that dependency would
                                   # defeat the entire point of splitting layers into projects.
```

Each project sets its own `<RootNamespace>` (`CoeurApi.Modules.<Module>.<Layer>`) — the namespaces inside `.cs` files were already explicit before this split and didn't change; only the physical `.csproj` boundary around them did.

Modules form a DAG, not a flat set: `Users` is a leaf (its `Domain`/`Application` depend on nothing but `SharedKernel`/root `Application`); `Shopping.Domain` and `Finances.Domain` reference `Users.Domain` directly because they need the `User` entity (`ShoppingList.Owner`); `Authentication.Application` references `Users.Application` because `LoginService` needs `IUsersRepository` — this is an intentional, one-directional exception to "modules never see each other's internals," scoped to the layer that's genuinely shared, not the whole module. `SharedKernel` and `Application` never depend on any module.

`Finances` is in `coeur-api.slnx` with the full four-project anatomy already scaffolded (mirroring `Users`/`Shopping`), but `Finances.Infrastructure/FinancesModule.cs` still has no body, and none of the four `Finances.*.csproj` are referenced by `Infrastructure.csproj`/`Api.csproj` or wired into `Program.cs`. It's the next module to build out.

### `.csproj` configuration per project

- **SDK**: only `src/Api` uses `Microsoft.NET.Sdk.Web` (it's the actual host — needs static web assets, launch profiles, hosting startup). Every other project (`SharedKernel`, `Application`, `Infrastructure`, every module layer) uses the plain `Microsoft.NET.Sdk`, since none of them self-host; picking `Sdk.Web` there would misrepresent what the project is.
- **ASP.NET Core types without `Sdk.Web`**: any non-host project that still needs ASP.NET Core types (`Infrastructure` for `DbContext`/HTTP context, a module's `Infrastructure`/`Presentation` project for `IServiceCollection`/`ICurrentUser`/JWT bearer/etc.) adds `<FrameworkReference Include="Microsoft.AspNetCore.App" />` explicitly rather than switching SDKs. A module's `Domain`/`Application` project never needs this — no ASP.NET Core type belongs there.
- **Global usings**: each project only declares `<Using Include="..." />` for namespaces it actually uses pervasively (e.g. `Microsoft.AspNetCore.Http`, `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Configuration`) — not a blanket list copy-pasted across every `.csproj`.
- **`<RootNamespace>`**: explicitly set on every project — `CoeurApi.<ProjectName>` for `src/*` (`CoeurApi.SharedKernel`, `CoeurApi.Infrastructure`, `CoeurApi.Api`, ...) and `CoeurApi.Modules.<ModuleName>.<Layer>` for `src/Modules/*/*` (`CoeurApi.Modules.Users.Domain`, `CoeurApi.Modules.Users.Application`, ...). Set explicitly rather than left to the SDK's folder-derived default, so a module or layer folder can be renamed without silently renaming every namespace inside it.
- **`<ProjectReference>` direction** always follows the dependency rule from the diagram above, now enforced at the layer level too: `Domain → SharedKernel` (at most); `Application → Domain` + root `Application` (only if a service needs `ICurrentUser`); `Infrastructure → Application` only (`Domain` arrives transitively); `Presentation → Application` (+ root `Application` directly, only if a controller injects `ICurrentUser` itself). `Infrastructure` and `Presentation` never reference each other. Cross-module references attach at the specific layer that's actually shared (`Shopping.Domain → Users.Domain`, `Authentication.Application → Users.Application`), never module-to-module as a whole. Root `Infrastructure`/`Api` never get referenced by a module (that would create a cycle) — root `Infrastructure` references each module's `Domain` project (for the entity types used in `DbSet<T>`) **and** that module's `Infrastructure` project (for the `IEntityTypeConfiguration<T>` classes under its `Persistence/Configurations/` folder, so `AppDbContext.OnModelCreating` can `ApplyConfigurationsFromAssembly` against it); this doesn't cycle back since a module's `Infrastructure` never references root `Infrastructure`/`Api`. `Api` references each module's `Infrastructure` (for `Add<Module>Module()`) and `Presentation` (for controller discovery). If a reference in the wrong direction doesn't compile, that's the multi-project split doing its job, not a bug to work around.
- **Package placement**: a `PackageReference` lives in the project that actually uses its types, not centralized in the host — `BCrypt.Net-Next` sits in `Users.Application`/`Authentication.Application` (password hashing/verification). `Microsoft.EntityFrameworkCore`+`.Relational` sit in every module's `Infrastructure` project (for the `IEntityTypeConfiguration<T>` classes under `Persistence/Configurations/`, which need `.Relational` for `ToTable(...)`), on top of the plain `Microsoft.EntityFrameworkCore` already there for `DbContext`/`Set<T>()` in the repository — never in `Api`, which never talks to EF Core directly, and never in `Domain`, which has no EF Core package reference at all. The `Npgsql.EntityFrameworkCore.PostgreSQL` provider plus `Microsoft.EntityFrameworkCore.Design` tooling live only in the root `Infrastructure`. `Api` additionally carries its own `Microsoft.EntityFrameworkCore.Design` reference because `dotnet ef` resolves design-time tooling from the **startup** project, not just the project holding the `DbContext` — hence `--project src/Infrastructure --startup-project src/Api` on every `dotnet ef` invocation.
- **Validator registration**: every module's `Infrastructure` project carries the `FluentValidation.DependencyInjectionExtensions` package reference and calls `AddValidatorsFromAssemblyContaining<T>()` inside its own `Add<Module>Module()` — the validators themselves live in `Application` (which only needs the plain `FluentValidation` package), never a single blanket scan from `Api`.

Request flow: `Controller → Service → Repository`, response bubbles back up through `Controller`. A controller injects one service per action (constructor gets `CreateUserService createUser, GetUserByIdService getUserById, ...`), never a single grouped service. Adding a module means creating a new `src/Modules/<Module>/` with a `<Module>.Domain`/`<Module>.Application`/`<Module>.Infrastructure`/`<Module>.Presentation` project (skip `Domain` if the module has no entity of its own, like `Authentication`), wiring `Add<Module>Module()` (in `<Module>.Infrastructure`) into `Program.cs`, and adding all four projects to `coeur-api.slnx`.

### Um único `AppDbContext` compartilhado, não um por módulo

`AppDbContext` (`src/Infrastructure/Persistence/AppDbContext.cs`) declara os `DbSet<T>` de **todos** os módulos (`Users`, `ShoppingList`, `Product`, `ListItem`, ...) e roda `ApplyConfigurationsFromAssembly` sobre o assembly de cada módulo que tem entidades. Isso é deliberado, não uma violação acidental do isolamento de módulos:

- **Uma transação por request.** `SaveChangesAsync()` é chamado uma vez, no fim da unit of work (`IUnitOfWork`, implementado pelo próprio `AppDbContext`). Se um caso de uso tocar entidades de dois módulos (ex.: `ShoppingList.Owner` referenciando `User`), a persistência dos dois lado a lado continua atômica. Um `DbContext` por módulo exigiria coordenar múltiplas transações (ou um padrão saga) só para operações que, num monólito, deveriam ser triviais.
- **Um banco físico, um lugar para migrations.** Todas as migrations vivem em `src/Infrastructure/Persistence/Migrations`, geradas a partir de um único model. Múltiplos `DbContext`s apontando pro mesmo banco forçariam decidir qual projeto é dono de qual tabela e como evitar migrations conflitantes — complexidade real de multi-banco, sem o benefício de múltiplos bancos.
- **O isolamento entre módulos continua garantido em compile-time, só que em outro ponto.** Um módulo nunca referencia `AppDbContext` diretamente — os repositórios (`UsersRepository`, `ProductRepository`, ...), que vivem no `.csproj` de `Infrastructure` de cada módulo, dependem do tipo genérico `DbContext` + `Set<T>()` (ver `AddInfrastructure()`, que registra `services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>())`). Isso é o que evita um módulo precisar referenciar o projeto `Infrastructure` raiz (que formaria um ciclo, já que `Infrastructure` referencia os módulos). O módulo sabe que existe *um* `DbContext`; ele não sabe, nem pode saber, que esse `DbContext` também carrega `DbSet`s de outros módulos. E o `Infrastructure` raiz, por sua vez, referencia tanto o `.csproj` de `Domain` quanto o de `Infrastructure` de cada módulo (`Users.Domain`+`Users.Infrastructure`, `Shopping.Domain`+`Shopping.Infrastructure`) — nunca `Application` nem `Presentation` do módulo — porque `AppDbContext` precisa das entidades (`Domain`) e dos `IEntityTypeConfiguration<T>` (`Infrastructure/Persistence/Configurations/`, ver abaixo); essa segunda referência não fecha ciclo, já que a `Infrastructure` de um módulo nunca referencia a `Infrastructure`/`Api` raiz de volta.

### `IEntityTypeConfiguration<T>` mora em `Infrastructure/Persistence`, não em `Domain`

As classes `IEntityTypeConfiguration<T>` de cada módulo (`UserConfiguration`, `ProductConfiguration`, `ShoppingListConfiguration`, `ListItemConfiguration`) vivem em `<Módulo>.Infrastructure/Persistence/Configurations/`, não em `<Módulo>.Domain`. Isso é deliberado:

- **`Domain` fica livre de qualquer pacote do EF Core.** Antes, `<Módulo>.Domain.csproj` carregava `Microsoft.EntityFrameworkCore`+`.Relational` só por causa da configuration co-localizada ali — uma dependência de framework num projeto que a Clean Architecture diz que deveria ter zero dependências externas (além de `SharedKernel`). Movendo a configuration pra `Infrastructure`, `Domain` não referencia mais nenhum pacote do EF Core.
- **`AppDbContext.OnModelCreating` aponta pro assembly de `Infrastructure` de cada módulo**, não mais pro de `Domain`: `modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly)` (era `typeof(User).Assembly`). Isso exige que o `Infrastructure.csproj` raiz referencie também `Users.Infrastructure.csproj`/`Shopping.Infrastructure.csproj`, além do `Domain.csproj` de cada um (que continua necessário pros tipos usados nos `DbSet<T>`).
- **`Migrations/InitialCreate` (e qualquer migration futura) não muda** — o mapeamento (`ToTable`, `HasMaxLength`, `HasIndex`, ...) é idêntico, só o arquivo `.cs` mudou de projeto. `dotnet ef migrations list --project src/Infrastructure --startup-project src/Api` continua funcionando sem gerar diff de modelo.
- **Trade-off aceito:** um módulo pode, em teoria, chamar `context.Set<TOutroModulo>()` já que o tipo genérico `DbContext` expõe qualquer `DbSet` registrado nele. Isso não é bloqueado pelo compilador — é convenção: um módulo só deve usar `Set<T>()` para as entidades que ele mesmo declara no seu próprio `<Módulo>.Domain`. Se esse limite virar um problema real (não é hoje), a saída é dividir o banco antes de dividir o `DbContext`.

Host wiring lives in `src/Api/Extensions/`:
- `ServiceCollectionExtensions.AddApiServices(configuration)` — ProblemDetails/toast customization, ForwardedHeaders, CORS (`Frontend` policy), per-IP rate limiting on login, controllers + global `AuthorizeFilter`/`FluentValidationFilter`, authorization. Deliberately thin — it knows nothing about any module's DTOs; each module registers its own validators.
- `WebApplicationExtensions.UseApiServices()` — middleware pipeline order: `UseExceptionHandler()` → CORS → Authentication → Authorization → RateLimiter.
- `src/Infrastructure/DependencyInjection.AddInfrastructure(configuration)` — `AppDbContext` + `IUnitOfWork` + a bare `DbContext` registration (so module repositories can depend on the abstract type), `ICurrentUser`.
- `src/Modules/Authentication/Authentication.Infrastructure/AuthModule.AddAuthModule(configuration)` — also configures the JWT bearer scheme (reads the cookie via `OnMessageReceived`) and binds `JwtSettings`, since those are Authentication-module-owned, not host-owned.

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

JWT stored in an HTTP-only cookie (`access_token`), read out in `AddJwtBearer().Events.OnMessageReceived` rather than the `Authorization` header — so there is no bearer-token handling on the client. This bearer scheme is wired inside `src/Modules/Authentication/Authentication.Infrastructure/AuthModule.cs`, which also configures the concrete `TokenService` (`Authentication.Infrastructure/Security/`) behind the `ITokenService` port that `LoginService` (`Authentication.Application`) depends on — Authentication is the only module that touches JWT internals. `JwtSettings` (`Authentication.Application/Settings/`) is a plain config POCO, not an Infrastructure concern, specifically so `AuthController` (`Authentication.Presentation`) can read `ExpirationHours` for the cookie without needing a reference to `Authentication.Infrastructure`. Every controller requires auth by default (global `AuthorizeFilter`); use `[AllowAnonymous]` to opt out (e.g. login, user creation). `ICurrentUser` (`src/Application/Abstractions/`) / `CurrentUserService` (`src/Infrastructure/Authentication/`) exposes the authenticated user's id/email/name/role for ownership checks inside services (see the `id != currentUser.Id && !currentUser.IsAdmin` pattern in `GetUserByIdService`/`UpdateUserService`/`DeleteUserService`). No refresh tokens — expired token means re-login.

## API routes

All controller routes are prefixed `api/v1/...` (e.g. `[Route("api/v1/users")]`). Keep any new controller and any hardcoded `Location` header (`Created($"api/v1/...")`) consistent with this prefix.


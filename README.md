# Coeur API

Coeur API é uma API REST de propósito geral desenvolvida para servir de base para projetos pessoais. O objetivo é ter uma fundação sólida, bem estruturada e reutilizável — com autenticação, gerenciamento de usuários e uma arquitetura que escala de forma organizada conforme o projeto cresce, sem a complexidade desnecessária de microserviços.

O projeto foi construído com foco em clareza de código, convenções consistentes e facilidade de extensão. Cada decisão arquitetural foi tomada priorizando a compreensão do que está acontecendo, sem magia excessiva ou abstrações desnecessárias.

---

## Tecnologias

| Tecnologia | Versão | Papel |
|---|---|---|
| .NET | 10 | Runtime |
| ASP.NET Core | 10 | Framework web |
| PostgreSQL | 17 | Banco de dados |
| Entity Framework Core | 10 | ORM (`Npgsql.EntityFrameworkCore.PostgreSQL`) |
| JWT (HTTP-only cookie) | — | Autenticação |
| BCrypt.Net-Next | — | Hash de senhas |
| FluentValidation | — | Validação de entrada |
| Scalar | — | Documentação interativa (OpenAPI) |
| xUnit + Moq | — | Testes |

---

## Arquitetura

O projeto combina dois padrões arquiteturais, cada um resolvendo um problema diferente — e ambos são impostos por **fronteiras de `.csproj`** (não por pastas), em dois níveis:

- **Clean Architecture** *(dentro de cada módulo)* impõe a regra de dependência na camada, e cada camada é seu próprio `.csproj`: `<Módulo>.Domain` não depende de nada (além de `SharedKernel`, quando precisa), `<Módulo>.Application` depende só do seu próprio `Domain` + `Abstractions` (nunca de um repository EF Core concreto ou do `AppDbContext`), e `<Módulo>.Infrastructure`/`<Módulo>.Presentation` são os únicos projetos que conhecem ASP.NET Core/EF Core diretamente. É isso que torna a regra de negócio de um módulo testável sem banco e trocável na camada de persistência sem tocar em `Application`/`Domain`.
- **Monólito Modular** *(entre módulos)* impõe essa mesma regra de dependência no nível do módulo. Quem garante os dois níveis é o compilador, não a revisão de código: `Users.Presentation` não consegue enxergar `Users.Infrastructure`, e `Users` não consegue enxergar `Shopping.Domain`, a menos que o `.csproj` correspondente tenha uma `<ProjectReference>` explícita pra isso. Um "monólito modular" baseado só em pastas (módulos como namespaces, camadas como subpastas, tudo num único projeto) depende inteiramente de disciplina; aqui, qualquer uma dessas violações é erro de build, não warning de lint.

Juntos, os dois padrões dão um único artefato de deploy (operação simples — um processo, uma conexão com banco, sem latência de rede entre "serviços") mantendo a opção de extrair um módulo (ou só a `Infrastructure` dele) para um serviço próprio no futuro: como cada camada já só se comunica com o resto do sistema por interfaces e pela fronteira do seu próprio `.csproj`, extrair é uma mudança de deploy, não uma reescrita.

### Por que não microserviços (ainda)?

- **Simplicidade operacional** — um único artefato para fazer deploy, uma única conexão com o banco, sem latência de rede entre serviços
- **Organização por domínio, garantida em tempo de compilação** — cada módulo é dono do seu código; outro módulo só o alcança via `ProjectReference` explícita
- **Escalabilidade de código** — conforme o projeto cresce, novos módulos são adicionados sem impactar os existentes
- **Caminho para microserviços** — se um domínio precisar escalar independentemente no futuro, a separação já está feita: módulos falam entre si por interface, não por acoplamento direto

### Estrutura de pastas

```
coeur-api/
├── src/
│   ├── SharedKernel/                  # Zero dependências de projeto. HttpException, PagedResult/
│   │   ├── Abstractions/              # Pagination, IUnitOfWork, UserRole — o único conceito de
│   │   ├── Common/                    # domínio deliberadamente compartilhado entre módulos.
│   │   ├── Enums/
│   │   └── Exceptions/
│   │
│   ├── Application/                   # → SharedKernel apenas. Contratos cross-module: ICurrentUser.
│   │   └── Abstractions/
│   │
│   ├── Infrastructure/                # → SharedKernel, Application, Modules/Users/Users.Domain,
│   │   ├── Authentication/            # Modules/Shopping/Shopping.Domain (só o Domain de cada — NÃO
│   │   └── Persistence/               # Authentication, ver módulo abaixo). AppDbContext (compõe
│   │       └── Migrations/            # DbSets de todos os módulos), migrations, CurrentUserService,
│   │                                  # AddInfrastructure().
│   │
│   ├── Api/                           # → tudo. Host: Program.cs, appsettings*.json,
│   │   ├── Extensions/                # ServiceCollection/WebApplication extensions
│   │   │                              # (AddApiServices/UseApiServices), HttpExceptionHandler,
│   │   ├── Filters/                   # FluentValidationFilter, StatusPage.
│   │   ├── Middleware/
│   │   └── Pages/
│   │
│   └── Modules/
│       ├── Users/                                 # Módulo folha — não depende de nenhum outro módulo
│       │   ├── Users.Domain/Users.Domain.csproj           # Entidade User: private set, factory
│       │   │                                              # Create(...), mapeamento EF via
│       │   │                                              # IEntityTypeConfiguration<T>
│       │   ├── Users.Application/Users.Application.csproj # Abstractions/ (IUsersRepository),
│       │   │                                              # Services/, DTOs/, Validators/
│       │   ├── Users.Infrastructure/Users.Infrastructure.csproj # UsersRepository (DbContext
│       │   │                                              # genérico, não AppDbContext), UsersModule.cs
│       │   └── Users.Presentation/Users.Presentation.csproj    # UsersController
│       │
│       ├── Authentication/                        # Sem Domain — não tem entidade própria
│       │   ├── Authentication.Application/...             # LoginService, ITokenService (porta),
│       │   │                                              # JwtSettings — → Users.Application
│       │   ├── Authentication.Infrastructure/...           # TokenService (implementa ITokenService),
│       │   │                                              # AuthModule.cs — únicos internals de JWT
│       │   └── Authentication.Presentation/...             # AuthController, MeController
│       │
│       ├── Shopping/                              # → Users.Domain (ShoppingList.Owner)
│       │   ├── Shopping.Domain/...                         # ShoppingList, Product, ListItem
│       │   ├── Shopping.Application/...
│       │   ├── Shopping.Infrastructure/...
│       │   └── Shopping.Presentation/...                    # ShoppingListsController, ProductsController
│       │
│       └── Finances/                              # → Users.Domain — em construção: as 4 camadas já
│                                                    # existem como scaffold, mas Finances.Infrastructure/
│                                                    # FinancesModule.cs ainda não tem corpo, nenhuma delas
│                                                    # é referenciada por Infrastructure/Api e nada é
│                                                    # registrado em Program.cs
│
├── tests/
│   └── CoeurApi.Tests/                # xUnit + Moq. Fora de src/, referencia as camadas que exercita
│                                       # via ProjectReference (Users.Domain/.Application,
│                                       # Authentication.Application/.Infrastructure, Shopping.Domain/.Application)
│
├── coeur-api.slnx
├── docker-compose.dcproj
├── docker-compose.yml
├── docker-compose.override.yml
└── Taskfile.yaml
```

### Anatomia de um módulo

```
<Módulo>/
├── <Módulo>.Domain/<Módulo>.Domain.csproj              # → SharedKernel (se precisar de algo
│   # Entidades com private set, mutadas só por métodos próprios, criadas só via factory estático
│   # Create(...) (nunca `new`). Mapeamento EF Core via IEntityTypeConfiguration<T> fica junto da
│   # entidade (por isso referencia Microsoft.EntityFrameworkCore/.Relational).
│   # Authentication não tem Domain — não tem entidade própria, só usa a de Users.
├── <Módulo>.Application/<Módulo>.Application.csproj    # → <Módulo>.Domain, SharedKernel
│   ├── Abstractions/    # Os ports do próprio módulo: interfaces de repository (IUsersRepository,
│   │                    # IProductRepository) E qualquer outra coisa que a Infrastructure implementa
│   │                    # de forma concreta (ex.: ITokenService da Authentication, implementado pelo
│   │                    # TokenService baseado em JWT) — Application nunca referencia um tipo
│   │                    # concreto de Infrastructure.
│   ├── Services/        # Uma classe por caso de uso (CreateUserService, GetUserByIdService, ...),
│   │                    # nunca um service com vários métodos agrupados. Cada uma expõe um único
│   │                    # ExecuteAsync(...), lança HttpException, orquestra repository e outras
│   │                    # dependências (inclusive outros Services, reaproveitados quando necessário).
│   ├── DTOs/            # Records de entrada + de saída com FromEntity(...) estático
│   ├── Validators/      # Validators FluentValidation dos DTOs
│   └── Settings/        # POCOs de configuração que um controller de Presentation também precisa
│                        # ler direto (ex.: JwtSettings, lido pelo AuthController) — fica aqui, não
│                        # em Infrastructure, justamente pra Presentation não precisar de uma
│                        # referência a Infrastructure só pra ver uma classe de configuração.
├── <Módulo>.Infrastructure/<Módulo>.Infrastructure.csproj  # → <Módulo>.Application apenas (Domain
│   │                    # chega transitivamente). Implementação dos repositories — dependem do
│   │                    # DbContext genérico do EF Core + Set<T>(), nunca do AppDbContext concreto.
│   │                    # Também hospeda Add<Módulo>Module(): registra repositories, services E o
│   │                    # próprio AddValidatorsFromAssemblyContaining<T>() (nunca de fora do módulo).
│   │                    # O esquema JWT bearer da Authentication também é configurado aqui.
└── <Módulo>.Presentation/<Módulo>.Presentation.csproj      # → <Módulo>.Application apenas, + o
                           # Application raiz direto quando um controller injeta ICurrentUser
                           # (MeController, ShoppingListsController). Controllers: recebe HTTP,
                           # delega pro Service, retorna a response. NUNCA referencia
                           # <Módulo>.Infrastructure — isso anularia o motivo de separar as camadas.
```

### Configuração de cada `.csproj`

- **SDK**: só `src/Api` usa `Microsoft.NET.Sdk.Web` (é o host de fato — precisa de static web assets, launch profiles, hosting startup). Todos os outros projetos (`SharedKernel`, `Application`, `Infrastructure`, todo projeto de camada de módulo) usam `Microsoft.NET.Sdk` puro, já que nenhum deles se auto-hospeda.
- **Tipos do ASP.NET Core sem `Sdk.Web`**: qualquer projeto não-host que precise de tipos do ASP.NET Core (`Infrastructure` para `DbContext`/`HttpContext`, o `Infrastructure`/`Presentation` de um módulo para `IServiceCollection`/`ICurrentUser`/JWT bearer/etc.) adiciona `<FrameworkReference Include="Microsoft.AspNetCore.App" />` explicitamente, em vez de trocar de SDK. `Domain`/`Application` de um módulo nunca precisam disso — nenhum tipo do ASP.NET Core pertence ali.
- **`<Using>` globais**: cada projeto só declara os globais que realmente usa de forma pervasiva (`Microsoft.AspNetCore.Http`, `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Configuration`), não uma lista padrão copiada em todo `.csproj`.
- **`<RootNamespace>`**: definido explicitamente em todo projeto — `CoeurApi.<Projeto>` em `src/*` e `CoeurApi.Modules.<Módulo>.<Camada>` em `src/Modules/*/*` (`CoeurApi.Modules.Users.Domain`, `CoeurApi.Modules.Users.Application`, ...) — em vez de depender do default do SDK derivado do nome da pasta, para uma pasta poder ser renomeada sem renomear silenciosamente todo o namespace.
- **Direção do `<ProjectReference>`** segue sempre a regra de dependência do diagrama acima, agora também no nível da camada: `Domain → SharedKernel` (no máximo); `Application → Domain` + `Application` raiz (só se algum service precisar de `ICurrentUser`); `Infrastructure → Application` apenas (`Domain` chega transitivamente); `Presentation → Application` (+ `Application` raiz direto, só se um controller injetar `ICurrentUser`). `Infrastructure` e `Presentation` nunca se referenciam. Referências entre módulos apontam pra camada específica que é compartilhada (`Shopping.Domain → Users.Domain`, `Authentication.Application → Users.Application`), nunca pro módulo inteiro. `Infrastructure`/`Api` na raiz nunca são referenciados por um módulo (formaria ciclo) — o `Infrastructure` raiz referencia só o `Domain` de cada módulo; `Api` referencia o `Infrastructure` (por `Add<Módulo>Module()`) e o `Presentation` (pros controllers) de cada módulo. Se uma referência na direção errada não compila, é a separação em múltiplos projetos fazendo seu trabalho.
- **Onde colocar cada `PackageReference`**: no projeto que realmente usa o pacote, não centralizado no host — `BCrypt.Net-Next` fica em `Users.Application`/`Authentication.Application`, `Microsoft.EntityFrameworkCore`+`.Relational` no `Domain` de cada módulo (por causa de `IEntityTypeConfiguration<T>`) e `Microsoft.EntityFrameworkCore` puro de novo na `Infrastructure` do módulo (por causa de `DbContext`/`Set<T>()` no repository) — nunca em `Api`, que não fala com EF Core diretamente. O provider `Npgsql.EntityFrameworkCore.PostgreSQL` + a tooling `Microsoft.EntityFrameworkCore.Design` ficam só no `Infrastructure` raiz. `Api` também carrega sua própria referência a `Microsoft.EntityFrameworkCore.Design`, porque o `dotnet ef` resolve a tooling de design-time a partir do projeto de **startup**, não só do projeto dono do `DbContext` — daí `--project src/Infrastructure --startup-project src/Api` em todo comando `dotnet ef`.
- **Registro de validators**: a `Infrastructure` de cada módulo carrega a referência a `FluentValidation.DependencyInjectionExtensions` e chama `AddValidatorsFromAssemblyContaining<T>()` dentro do seu próprio `Add<Módulo>Module()` — os validators em si ficam em `Application` (que só precisa do pacote `FluentValidation` puro), nunca um scan único a partir de `Api`.

### Um único `AppDbContext` compartilhado, não um por módulo

`AppDbContext` (`src/Infrastructure/Persistence/AppDbContext.cs`) declara os `DbSet<T>` de **todos** os módulos (`Users`, `ShoppingList`, `Product`, `ListItem`, ...) e roda `ApplyConfigurationsFromAssembly` sobre o assembly de cada módulo que tem entidades. Isso é deliberado, não uma violação acidental do isolamento de módulos:

- **Uma transação por request.** `SaveChangesAsync()` é chamado uma vez, no fim da unit of work (`IUnitOfWork`, implementado pelo próprio `AppDbContext`). Se um caso de uso tocar entidades de dois módulos (ex.: `ShoppingList.Owner` referenciando `User`), a persistência dos dois lado a lado continua atômica. Um `DbContext` por módulo exigiria coordenar múltiplas transações (ou um padrão saga) só para operações que, num monólito, deveriam ser triviais.
- **Um banco físico, um lugar para migrations.** Todas as migrations vivem em `src/Infrastructure/Persistence/Migrations`, geradas a partir de um único model. Múltiplos `DbContext`s apontando pro mesmo banco forçariam decidir qual projeto é dono de qual tabela e como evitar migrations conflitantes — complexidade real de multi-banco, sem o benefício de múltiplos bancos.
- **O isolamento entre módulos continua garantido em compile-time, só que em outro ponto.** Um módulo nunca referencia `AppDbContext` diretamente — os repositórios (`UsersRepository`, `ProductRepository`, ...), que vivem no `.csproj` de `Infrastructure` de cada módulo, dependem do tipo genérico `DbContext` + `Set<T>()` (`AddInfrastructure()` registra `services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>())`). Isso evita um módulo precisar referenciar o projeto `Infrastructure` raiz (que formaria um ciclo, já que `Infrastructure` referencia os módulos). O módulo sabe que existe *um* `DbContext`; ele não sabe, nem pode saber, que esse `DbContext` também carrega `DbSet`s de outros módulos. E o `Infrastructure` raiz, por sua vez, só referencia o `.csproj` de `Domain` de cada módulo (`Users.Domain`, `Shopping.Domain`) — nem `Application`, nem `Infrastructure`, nem `Presentation` do módulo — porque `AppDbContext` só precisa das entidades e dos `IEntityTypeConfiguration<T>` que moram ali.
- **Trade-off aceito:** um módulo pode, em teoria, chamar `context.Set<TOutroModulo>()`, já que o tipo genérico `DbContext` expõe qualquer `DbSet` registrado nele. Isso não é bloqueado pelo compilador — é convenção: um módulo só deve usar `Set<T>()` para as entidades que ele mesmo declara no seu próprio `<Módulo>.Domain`. Se esse limite virar um problema real (não é hoje), a saída é dividir o banco antes de dividir o `DbContext`.

### Fluxo de uma request

```
HTTP Request
    └── Controller (Presentation)  # valida entrada, chama service
        └── Service (Application)  # aplica regras de negócio, lança HttpException se necessário
            └── Repository (Infrastructure)  # consulta ou persiste no banco via EF Core (DbContext genérico)
        └── Controller               # retorna response com status HTTP adequado
HTTP Response

Em caso de erro:
    HttpException → HttpExceptionHandler (IExceptionHandler) → Problem Details (RFC 9457) com extension toast
```

Um controller injeta um service por ação (construtor recebe `CreateUserService createUser, GetUserByIdService getUserById, ...`), nunca um único service agrupado.

### Tratamento de erros

Erros de negócio são lançados como `HttpException` (`src/SharedKernel/Exceptions/HttpException.cs`) via factory methods semânticos — só existem os status codes realmente usados hoje (`BadRequest`, `Unauthorized`, `Forbidden`, `NotFound`, `Conflict`, `TooManyRequests`, `NoContent`):

```csharp
throw HttpException.NotFound("Usuário não encontrado.");
throw HttpException.Conflict("Email já está em uso.");
throw HttpException.Forbidden("Acesso negado.");
```

`HttpExceptionHandler` converte toda `HttpException` no formato padrão **Problem Details** ([RFC 9457](https://www.rfc-editor.org/rfc/rfc9457)):

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "Email já está em uso.",
  "toast": {
    "type": "warning",
    "message": "Email já está em uso."
  }
}
```

`title`/`type` são preenchidos automaticamente pelo framework a partir do status HTTP — não são setados manualmente. `errors` (dicionário de validação por campo) aparece como membro de topo sempre que `HttpException.Errors` é definido, no mesmo formato que o `[ApiController]` já usa nativamente para erros de model binding — um JSON malformado e uma falha de validação do FluentValidation respondem com o mesmo contrato.

`toast` é o único campo fora do RFC — consumido pelo interceptor HTTP do frontend Angular. É adicionado por um único hook global (`options.CustomizeProblemDetails` em `AddProblemDetails()`, `src/Api/Extensions/ServiceCollectionExtensions.cs`) que roda para **qualquer** Problem Details gerado pela aplicação: os de `HttpExceptionHandler`, o 500 genérico do próprio framework para exceptions não tratadas, os 400 automáticos de model binding do `[ApiController]`, e o 429 do rate limiter — nenhum desses call sites precisa setar `toast` manualmente.

### Autenticação

JWT armazenado em **cookie HTTP-only** (`access_token`), lido em `AddJwtBearer().Events.OnMessageReceived` em vez do header `Authorization` — não há lógica de bearer token no client. O esquema JWT é configurado em `src/Modules/Authentication/Authentication.Infrastructure/AuthModule.cs`, que também registra o `TokenService` concreto (`Authentication.Infrastructure/Security/`) atrás da porta `ITokenService` da qual o `LoginService` (`Authentication.Application`) depende — é o único módulo que conhece os internals de JWT. `JwtSettings` (`Authentication.Application/Settings/`) é um POCO de configuração puro, não uma preocupação de Infrastructure, justamente pra o `AuthController` (`Authentication.Presentation`) poder ler `ExpirationHours` pro cookie sem precisar de referência a `Authentication.Infrastructure`.

Todo controller exige autenticação por padrão (`AuthorizeFilter` global); use `[AllowAnonymous]` para abrir exceção (login, criação de usuário). `ICurrentUser`/`CurrentUserService` expõe id/email/nome/role do usuário autenticado para checagens de ownership dentro dos services (padrão `id != currentUser.Id && !currentUser.IsAdmin` em `GetUserByIdService`/`UpdateUserService`/`DeleteUserService`).

O token expira em 24 horas. Não há refresh token — ao expirar, o usuário precisa fazer login novamente.

### Rotas da API

Todas as rotas são prefixadas com `api/v1/...` (ex.: `[Route("api/v1/users")]`). Qualquer controller novo e qualquer header `Location` (`Created($"api/v1/...")`) deve manter esse prefixo.

---

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 17](https://www.postgresql.org/) (local ou em container)
- [Task](https://taskfile.dev/) *(opcional, para comandos facilitados)*
- [Docker](https://www.docker.com/) *(opcional, para produção)*

---

## Configuração do ambiente

### 1. Clonar o repositório

```bash
git clone https://github.com/seu-usuario/coeur-api.git
cd coeur-api
```

### 2. Configurar User Secrets (desenvolvimento local)

O desenvolvimento local usa o mecanismo de **User Secrets** do .NET, não um arquivo `.env` — as credenciais ficam fora do repositório:

```bash
dotnet user-secrets set "ConnectionStrings:Default" "Host=...;Port=5432;Database=...;Username=...;Password=..."
dotnet user-secrets set "Jwt:Secret" "<secret com pelo menos 32 caracteres>"
```

No Visual Studio, o gerenciador de User Secrets está disponível clicando com o botão direito no projeto `src/Api` → **Manage User Secrets**.

### 3. Variáveis de ambiente (produção)

Em produção, a configuração vem de variáveis de ambiente (ver `docker-compose.yml`). Copie o exemplo e preencha:

```bash
cp .env.example .env
```

```env
POSTGRES_CONNECTION=Host=;Port=5432;Database=;Username=;Password=;
JWT__SECRET=
JWT__ISSUER=coeur-api
JWT__AUDIENCE=coeur-api
JWT__EXPIRATIONHOURS=24
ALLOWED_HOSTS=seudominio.com
CORS_ALLOWED_ORIGINS=https://coeur.app.br
DATABASE_AUTO_MIGRATE=true
```

### 4. Restaurar dependências

```bash
dotnet restore
```

---

## Banco de dados

As migrations são aplicadas automaticamente ao iniciar a aplicação via `MigrateAsync()` em `Program.cs` (desligável com `Database:AutoMigrate=false`/`DATABASE_AUTO_MIGRATE=false` para ambientes com múltiplas réplicas, onde a migration deve rodar como step separado do deploy). Não é necessário nenhum comando manual em produção.

O `DbContext` vive em `src/Infrastructure`, mas o composition root (e `Microsoft.EntityFrameworkCore.Design`) é `src/Api` — por isso a tooling do EF Core sempre precisa das duas flags:

```bash
dotnet ef migrations add NomeDaMigration --project src/Infrastructure --startup-project src/Api
dotnet ef migrations list --project src/Infrastructure --startup-project src/Api
dotnet ef migrations remove --project src/Infrastructure --startup-project src/Api
```

> O EF Core tools precisa estar instalado: `dotnet tool install --global dotnet-ef`

Via Task: `task migrate -- NomeDaMigration`.

---

## Executando

```bash
dotnet run --project src/Api
```

A API estará disponível em:

- `https://localhost:7209`
- `http://localhost:5148`

A documentação interativa (Scalar) estará disponível em:

- `https://localhost:7209/scalar/v1`

---

## Testes

Os testes ficam em `tests/CoeurApi.Tests`, um projeto xUnit + Moq separado dos módulos — vive fora de `src/`, então nunca é puxado para o build de nenhum módulo ou do host, e as dependências de teste nunca vão pro artefato de produção. Referencia as camadas específicas que exercita via `ProjectReference` (`Users.Domain`/`Users.Application`, `Authentication.Application`/`Authentication.Infrastructure`, `Shopping.Domain`/`Shopping.Application`).

```bash
dotnet test
```

---

## Produção (Docker)

O Dockerfile utiliza multi-stage build — a imagem de produção parte da imagem `aspnet` sem SDK, resultando em uma imagem final enxuta.

Em produção, a API é exposta via **Nginx Proxy Manager** na rede Docker interna, sem expor portas diretamente no host.

### Build e deploy

```bash
task build     # builda a imagem
task rebuild   # builda a imagem sem cache
task start     # sobe a stack em background
task deploy    # build + start em um comando
```

### Gerenciamento

```bash
task logs      # exibe logs em tempo real
task restart   # reinicia o container
task shell     # abre shell no container
task down      # derruba os containers
```

---

## Padrões de resposta

### Sucesso — recurso retornado

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "João Silva",
  "email": "joao@email.com",
  "isActive": true,
  "isEmailVerified": false,
  "createdAt": "2026-01-01T00:00:00Z",
  "updatedAt": null,
  "lastLoginAt": null
}
```

### Erro de negócio (Problem Details, RFC 9457)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "Email já está em uso.",
  "toast": {
    "type": "warning",
    "message": "Email já está em uso."
  }
}
```

### Erro de validação (`ValidationProblemDetails`)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "errors": {
    "email": ["O campo Email é obrigatório."]
  },
  "toast": {
    "type": "warning",
    "message": "Requisição inválida."
  }
}
```

---

## Convenções de código

- Namespaces espelham o projeto/módulo/camada: `CoeurApi.<Projeto>` em `src/*`, `CoeurApi.Modules.<Módulo>.<Camada>` em `src/Modules/*/*`
- Entidades usam `private set` em todas as propriedades — estado só é alterado por métodos da própria entidade
- Entidades são criadas via factory method estático `Create`, nunca por construtor público
- DTOs de saída são `record` imutáveis com método estático `FromEntity` para conversão
- Erros de negócio usam `HttpException` com factory methods semânticos (`NotFound`, `Conflict`, etc.) — só existem os status realmente usados
- Todas as datas são armazenadas e retornadas em **UTC**
- Nomes de tabelas e colunas seguem a convenção **default do EF Core** (não há `snake_case` configurado — ver nota na seção Stack)
- Cada módulo registra seus próprios serviços e validators via extension method `Add<Módulo>Module()`
- Um repository depende do `DbContext` genérico + `Set<T>()`, nunca do `AppDbContext` concreto
- Um controller injeta um service por ação, nunca um service agrupado com vários métodos

---

## Adicionando um novo módulo

1. Crie `src/Modules/NomeModulo/NomeModulo.Domain/NomeModulo.Domain.csproj`, `NomeModulo.Application/...`, `NomeModulo.Infrastructure/...` e `NomeModulo.Presentation/...`, seguindo a configuração e a direção de `<ProjectReference>` descritas em [Configuração de cada `.csproj`](#configuração-de-cada-csproj) (pule o `Domain` se o módulo não tiver entidade própria, como a Authentication)
2. Declare a entidade em `NomeModulo.Domain/` com `private set` e factory method `Create`, com o mapeamento EF Core via `IEntityTypeConfiguration<T>` co-localizado
3. Declare a interface do repository em `NomeModulo.Application/Abstractions/`
4. Implemente o repository em `NomeModulo.Infrastructure/`, dependendo do `DbContext` genérico + `Set<T>()`
5. Implemente os services (um por caso de uso) em `NomeModulo.Application/Services/`, os DTOs em `NomeModulo.Application/DTOs/` e os validators em `NomeModulo.Application/Validators/`
6. Crie o controller em `NomeModulo.Presentation/`
7. Registre tudo em `NomeModulo.Infrastructure/NomeModuloModule.cs` (`AddNomeModuloModule()`, incluindo `AddValidatorsFromAssemblyContaining<T>()`)
8. Adicione o `DbSet<Entidade>` e `ApplyConfigurationsFromAssembly` correspondentes em `AppDbContext` (`src/Infrastructure/Persistence/AppDbContext.cs`)
9. Adicione `<ProjectReference>` pra `NomeModulo.Domain` em `Infrastructure.csproj` (se o módulo tiver entidades) e pra `NomeModulo.Infrastructure`/`NomeModulo.Presentation` em `Api.csproj`
10. Chame `builder.Services.AddNomeModuloModule(...)` em `Program.cs`
11. Adicione os 4 `.csproj` em `coeur-api.slnx`
12. Rode `dotnet ef migrations add NomeDaMigration --project src/Infrastructure --startup-project src/Api`

---

## Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE.txt) para mais detalhes.

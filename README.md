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
| Entity Framework Core | 10 | ORM |
| Npgsql | 10 | Driver PostgreSQL para EF Core |
| BCrypt.Net-Next | — | Hash de senhas |
| Scalar | — | Documentação interativa (OpenAPI) |

---

## Arquitetura

O projeto adota o padrão **Monólito Modular** — uma abordagem que combina a simplicidade operacional de um monólito com a organização e separação de responsabilidades de uma arquitetura modular.

Ao contrário dos microserviços, onde cada domínio é um serviço independente com sua própria infraestrutura, o monólito modular mantém tudo em um único processo mas organiza o código em módulos bem delimitados que se comunicam por interfaces, não por acoplamento direto.

### Por que monólito modular?

- **Simplicidade operacional** — um único artefato para fazer deploy, uma única conexão com o banco, sem latência de rede entre serviços
- **Organização por domínio** — cada módulo é dono do seu código, sem que outros módulos conheçam seus detalhes internos
- **Escalabilidade de código** — conforme o projeto cresce, novos módulos são adicionados sem impactar os existentes
- **Caminho para microserviços** — se um domínio precisar escalar independentemente no futuro, a separação já está feita conceitualmente

### Estrutura de pastas

```
coeur-api/
├── App/
│   ├── Core/                          # Infraestrutura transversal da aplicação
│   │   ├── Authentication/            # TokenService, CurrentUserService
│   │   ├── Database/                  # AppDbContext, Migrations
│   │   │   └── Migrations/
│   │   ├── Extensions/                # ServiceCollectionExtensions, WebApplicationExtensions
│   │   ├── Middleware/                # HttpExceptionHandler
│   │   └── Settings/                  # JwtSettings
│   │
│   ├── Shared/                        # Contratos e utilitários compartilhados entre módulos
│   │   ├── Exceptions/                # HttpException com factory methods por status HTTP
│   │   ├── Interfaces/                # ICurrentUser, IUsersRepository
│   │   ├── Models/                    # Result<T>, PagedList<T>
│   │   └── Validators/                # Base de validação
│   │
│   └── Modules/                       # Domínios da aplicação
│       ├── Authentication/            # Login, logout, contexto do usuário autenticado
│       │   ├── Controllers/
│       │   ├── DTOs/
│       │   ├── Services/
│       │   └── AuthModule.cs
│       └── Users/                     # Gerenciamento de usuários
│           ├── Controllers/
│           ├── DTOs/
│           ├── Models/
│           ├── Repositories/
│           ├── Services/
│           └── UsersModule.cs
│
├── tests/
│   └── CoeurApi.Tests/                # Projeto de testes (xUnit + Moq), separado do artefato publicável
│       └── Modules/                   # Espelha os módulos de App/Modules
│
├── Program.cs
├── appsettings.json
├── Dockerfile
├── compose.yaml
└── Taskfile.yaml
```

### Camadas

**Core** contém tudo que é infraestrutura técnica — classes que conversam com o .NET, com o banco, com o pipeline HTTP. Não há regra de negócio aqui. O `AppDbContext` vive aqui, assim como o middleware de exceção e a configuração de autenticação JWT.

**Shared** contém contratos e utilitários que qualquer módulo pode precisar, mas que não pertencem a nenhum módulo específico. A distinção com o `Core` é que o `Shared` não sabe que existe ASP.NET — é C# puro. As interfaces que um módulo expõe para os outros ficam aqui.

**Modules** contém os domínios da aplicação. Cada módulo é fechado internamente e expõe apenas o que precisa através de interfaces declaradas em `Shared`. A comunicação entre módulos acontece sempre por interfaces registradas no container de DI, nunca por acoplamento direto entre implementações.

### Anatomia de um módulo

```
ModuleName/
├── Controllers/        # Recebe a request HTTP, delega pro service, retorna a response
├── DTOs/               # Objetos de entrada (CreateDto, UpdateDto) e saída (NameResponse)
│                       # DTOs de saída têm método estático FromEntity para mapear da entidade
├── Models/             # Entidade de domínio com propriedades private set e factory method Create
│                       # Configuração do EF Core via IEntityTypeConfiguration<T>
├── Repositories/       # Acesso ao banco via AppDbContext, implementa interface de Shared
├── Services/           # Uma classe por caso de uso (ex: CreateUserService, GetUserByIdService),
│                       # nunca um service com vários métodos agrupados. Cada uma expõe um único
│                       # ExecuteAsync(...), orquestra repository e outras dependências (inclusive
│                       # outros services, quando alguma lógica precisa ser reaproveitada)
└── ModuleNameModule.cs # Extension method que registra tudo do módulo no container de DI
```

### Fluxo de uma request

```
HTTP Request
    └── Controller          # valida entrada, chama service
        └── Service         # aplica regras de negócio, lança HttpException se necessário
            └── Repository  # consulta ou persiste no banco via EF Core
        └── Controller      # retorna response com status HTTP adequado
HTTP Response

Em caso de erro:
    HttpException → HttpExceptionHandler → Problem Details (RFC 9457) com extension toast
```

### Tratamento de erros

Erros de negócio são comunicados via `HttpException`, uma exception com factory methods semânticos que interrompem o fluxo:

```csharp
// No service — interrompe o fluxo imediatamente
throw HttpException.NotFound("Usuário não encontrado.");
throw HttpException.Conflict("Email já está em uso.");
throw HttpException.Forbidden("Acesso negado.");
```

`HttpExceptionHandler` (`IExceptionHandler`) intercepta e converte pra **Problem Details** ([RFC 9457](https://www.rfc-editor.org/rfc/rfc9457)) — o formato padrão de resposta de erro do ASP.NET Core, reconhecido por Scalar/Swagger e por qualquer client HTTP gerado a partir do OpenAPI:

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

`type`/`title` são preenchidos automaticamente pelo framework a partir do status HTTP. O único campo fora do RFC é `toast`, consumido pelo interceptor do frontend Angular — adicionado por um único hook global (`options.CustomizeProblemDetails` em `AddProblemDetails()`) que roda pra **qualquer** Problem Details que a aplicação gerar, não só as vindas de `HttpException`. Isso inclui o Problem Details genérico de 500 que o próprio ASP.NET Core cria pra bugs não tratados (que caem direto no handler default do framework, já loga sozinho) e os 400 automáticos de model binding do `[ApiController]` — todos ganham `toast` sem precisar de tratamento manual em cada lugar. Quando o erro carrega validação por campo (`HttpException.BadRequest(msg, errors)`/`.Conflict(msg, errors)`), a resposta vira `ValidationProblemDetails` com um `errors` no formato `{ campo: [mensagens] }` — o mesmo shape que o `[ApiController]` já usa nativamente pra erros de model binding, então os dois casos (JSON malformado vs. falha de validação do FluentValidation) respondem com o mesmo contrato.

### Autenticação

A autenticação utiliza **JWT armazenado em HTTP-only cookie**, o que significa que o token nunca fica acessível via JavaScript, protegendo contra ataques XSS. O browser envia o cookie automaticamente em cada request, sem necessidade de lógica explícita no frontend além de um interceptor de CORS.

O token expira em 24 horas. Não há refresh token — ao expirar, o usuário precisa fazer login novamente.

---

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 17](https://www.postgresql.org/)
- [Task](https://taskfile.dev/) *(opcional, para comandos facilitados)*
- [Docker](https://www.docker.com/) *(opcional, para produção)*

---

## Configuração do ambiente

### 1. Clonar o repositório

```bash
git clone https://github.com/seu-usuario/coeur-api.git
cd coeur-api
```

### 2. Configurar variáveis de ambiente

Copie o arquivo de exemplo e preencha com suas credenciais:

```bash
cp .env.example .env
```

```env
POSTGRES_CONNECTION=Host=;Port=5432;Database=;Username=;Password=
JWT__SECRET=
JWT__ISSUER=coeur-api
JWT__AUDIENCE=coeur-api
JWT__EXPIRATIONHOURS=24
```

### 3. Configurar User Secrets (desenvolvimento local)

O desenvolvimento local utiliza o mecanismo de User Secrets do .NET, que armazena credenciais fora do repositório em `%APPDATA%\Microsoft\UserSecrets\`:

```bash
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=senha"
dotnet user-secrets set "Jwt:Secret" "seu-secret-com-pelo-menos-32-caracteres"
```

No Visual Studio, o gerenciador de User Secrets está disponível clicando com o botão direito no projeto → **Manage User Secrets**.

### 4. Restaurar dependências

```bash
dotnet restore
```

---

## Banco de dados

As migrations são aplicadas automaticamente ao iniciar a aplicação via `MigrateAsync()` no `Program.cs`. Não é necessário nenhum comando manual em produção.

Para criar uma nova migration após alterar ou adicionar uma entidade:

```bash
dotnet ef migrations add NomeDaMigration
```

Para listar o status das migrations:

```bash
dotnet ef migrations list
```

Para reverter a última migration:

```bash
dotnet ef migrations remove
```

> O EF Core tools precisa estar instalado: `dotnet tool install --global dotnet-ef`

---

## Executando

```bash
dotnet run
```

A API estará disponível em:

- `https://localhost:7209`
- `http://localhost:5148`

A documentação interativa (Scalar) estará disponível em:

- `https://localhost:7209/scalar/v1`

---

## Testes

Os testes ficam em `tests/CoeurApi.Tests`, um projeto xUnit + Moq separado do artefato publicável (excluído do build do `coeur-api.csproj` e referenciando a API principal via `ProjectReference`).

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

### Erro de negócio (4xx)

```json
{
  "message": "Email já está em uso.",
  "toast": {
    "type": "warning",
    "message": "Email já está em uso."
  }
}
```

### Erro interno (5xx)

```json
{
  "message": "Erro interno do servidor.",
  "toast": {
    "type": "error",
    "message": "Erro interno do servidor."
  }
}
```

---

## Convenções de código

- Namespaces espelham a estrutura de pastas: `CoeurApi.App.{Camada}.{Módulo}`
- Entidades usam `private set` em todas as propriedades — estado só é alterado por métodos da própria entidade
- Entidades são criadas via factory method estático `Create`, nunca por construtor público
- DTOs de saída são `record` imutáveis com método estático `FromEntity` para conversão
- Erros de negócio usam `HttpException` com factory methods semânticos (`NotFound`, `Conflict`, etc.)
- Todas as datas são armazenadas e retornadas em **UTC**
- Nomes de tabelas e colunas seguem **snake_case** no banco via `UseSnakeCaseNamingConvention`
- Cada módulo registra seus próprios serviços via extension method `AddNomeModule`

---

## Adicionando um novo módulo

1. Crie a pasta `App/Modules/NomeModulo` com a estrutura padrão
2. Declare a entidade em `Models/` com `private set` e factory method `Create`
3. Configure o mapeamento EF Core via `IEntityTypeConfiguration<T>`
4. Adicione o `DbSet<Entidade>` no `AppDbContext`
5. Declare a interface do repository em `Shared/Interfaces/`
6. Implemente o repository em `Modules/NomeModulo/Repositories/`
7. Implemente o service em `Modules/NomeModulo/Services/`
8. Crie o controller em `Modules/NomeModulo/Controllers/`
9. Registre tudo em `NomeModuloModule.cs` e chame `builder.Services.AddNomeModulo()` no `Program.cs`
10. Rode `dotnet ef migrations add NomeDaMigration` para criar a migration

---

## Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE.txt) para mais detalhes.
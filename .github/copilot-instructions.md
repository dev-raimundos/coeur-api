# Copilot Instructions — NeonVertexApi

## Idioma
- Sempre responda em português brasileiro
- Mensagens de commit devem ser em português brasileiro
- Comentários de código devem ser em português brasileiro

## Projeto
- API REST em ASP.NET Core 10 com arquitetura monólito modular
- Linguagem: C# 14
- Banco de dados: PostgreSQL 17 via EF Core 10 com Npgsql
- Autenticação: JWT via HTTP-only cookie

## Estrutura
- `App/Core` — infraestrutura transversal (DbContext, middleware, autenticação, extensions)
- `App/Shared` — contratos e utilitários compartilhados entre módulos (interfaces, exceptions, models)
- `App/Modules` — domínios da aplicação (Users, Authentication, etc)
- Cada módulo segue: Controllers / Services / Repositories / Models / DTOs / XModule.cs

## Convenções
- Namespaces seguem a estrutura de pastas: `NeonVertexApi.App.*`
- Modelos usam propriedades com `private set` e factory method estático `Create`
- DTOs são `record` imutáveis com método estático `FromEntity`
- Erros de negócio usam `AppException` com factory methods estáticos
- Respostas de erro incluem objeto `toast` com `type` e `message`
- Snake_case no banco via `UseSnakeCaseNamingConvention`
- Datas sempre em UTC

## Mensagens de commit

- Escreva SEMPRE em português brasileiro, nunca em inglês. Isso vale para o título e para o corpo da mensagem.
- Use commits semânticos: feat, fix, docs, refactor, chore, test (o prefixo continua em inglês, o resto da frase em português).
- Seja descritivo e detalhado na mensagem, explicando o porquê da mudança, não só o quê.

Exemplo:

```
fix: corrige validação de e-mail duplicado no cadastro de usuário

Antes o serviço permitia criar dois usuários com o mesmo e-mail em
maiúsculas/minúsculas diferentes, pois a checagem não normalizava o
valor antes de comparar.
```
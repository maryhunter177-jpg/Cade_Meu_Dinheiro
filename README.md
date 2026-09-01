# Cadê Meu Dinheiro?

Plataforma multiplataforma de finanças pessoais criada com .NET 8, ASP.NET Core, PostgreSQL e .NET MAUI. A fundação atual entrega autenticação por JWT, regras financeiras, transações isoladas por usuário, dashboard e uma identidade visual própria.

## Arquitetura

```text
MAUI App -> ASP.NET Core API -> Application -> Domain
                              -> Infrastructure -> PostgreSQL
```

- `Domain`: entidades e regras monetárias sem dependências externas.
- `Application`: casos de uso, contratos e validações de fluxo.
- `Infrastructure`: EF Core, PostgreSQL, PBKDF2 e JWT.
- `Api`: endpoints, autenticação, Problem Details, rate limit e health checks.
- `App`: cliente MAUI com MVVM e design system claro/escuro.
- `tests`: testes de regras críticas e segurança de senha.

Consulte [architecture.md](docs/architecture.md), [security.md](docs/security.md) e [design-system.md](docs/design-system.md).

## Executar localmente

Pré-requisitos: SDK .NET 8 com workload MAUI, Docker Desktop e PostgreSQL 16 (se não usar Compose).

```powershell
Copy-Item .env.example .env
# Edite .env com valores aleatórios fortes.
docker compose up --build
```

API: `http://localhost:8080`. Health checks: `/health/live` e `/health/ready`. Swagger fica habilitado somente em Development, em `/swagger`.

Sem Docker:

```powershell
$env:ConnectionStrings__Database='Host=localhost;Database=cade_meu_dinheiro;Username=...;Password=...'
$env:Jwt__SigningKey='uma-chave-aleatoria-com-no-minimo-32-bytes'
dotnet ef database update --project src/CadeMeuDinheiro.Infrastructure --startup-project src/CadeMeuDinheiro.Api
dotnet run --project src/CadeMeuDinheiro.Api
```

## Testes

```powershell
dotnet test CadeMeuDinheiro.sln --collect:"XPlat Code Coverage"
```

## Configuração

| Variável | Obrigatória | Finalidade |
|---|---:|---|
| `ConnectionStrings__Database` | sim | Conexão PostgreSQL |
| `Jwt__SigningKey` | sim | Assinatura JWT; mínimo de 32 bytes |
| `Jwt__AccessTokenMinutes` | não | Validade do access token; padrão 15 min |
| `Cors__Origins__0` | produção | Origem confiável do cliente |

Nunca envie `.env`, tokens ou credenciais ao repositório.

## API MVP

- `POST /api/auth/register`, `POST /api/auth/login`
- `GET|POST /api/transactions`, `DELETE /api/transactions/{id}`
- `GET /api/dashboard`
- `GET /health/live`, `GET /health/ready`

## Roadmap

Refresh tokens rotativos e recuperação de senha; CRUD completo de categorias/orçamentos; alertas; relatórios e exportação; integração e testes de API com PostgreSQL efêmero; observabilidade OpenTelemetry; sincronização offline no MAUI.

## Estado de validação

Validado localmente com SDK .NET 8.0.424 e workload MAUI Windows 8.0.3:

- API Release: compilação com zero avisos e zero erros.
- MAUI Windows `win-x64` Release: compilação com zero avisos e zero erros.
- Testes unitários: 11 aprovados, zero falhas.
- XML, XAML e JSON: todos os arquivos parseados com sucesso.

Android e iOS exigem os workloads e SDKs nativos dos respectivos hosts. PostgreSQL/Docker não foram iniciados neste ambiente; execute a migration e os testes de integração antes de publicar.

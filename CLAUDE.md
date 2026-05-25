# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Argon is a personal finance management application (v1.7.0). It handles bank account tracking, transaction management, budget items, bank statement imports with automatic parsing, and transaction reconciliation/duplicate detection.

## Commands

### Backend (.NET 8)

```bash
# Build all projects
dotnet build

# Run the API (dev)
dotnet run --project src/Argon.WebApi/Argon.WebApi.csproj

# Run all tests
dotnet test

# Run a single test class or method
dotnet test --filter "FullyQualifiedName~TestClassName"
```

### Frontend (React/TypeScript)

Working directory: `src/Argon.WebGui/`

```bash
pnpm install        # Install dependencies
pnpm dev            # Vite dev server at localhost:5173
pnpm build          # TypeScript compile + Vite bundle
pnpm lint           # ESLint
pnpm lint:fix       # ESLint with auto-fix
pnpm format         # Prettier with import sorting
```

### Docker Compose

```bash
docker-compose -f compose/docker-compose.dev.yml up    # Dev (PostgreSQL only)
docker-compose -f compose/docker-compose.prod.yml up   # Production
docker-compose -f compose/docker-compose.test.yml up   # Test environment
```

## Architecture

### Backend — Clean Architecture + CQRS

The backend is split into four projects:

**Argon.Domain** — Core domain entities (`Account`, `Transaction`, `TransactionRow`, `BankStatement`, `Counterparty`, `BudgetItem`, etc.). All entities extend `BaseAuditableEntity`. Dates use NodaTime (`LocalDate`/`Instant`).

**Argon.Application** — Business logic organized into feature folders (Accounts, Transactions, BankStatements, Counterparties, BudgetItems). Each feature contains CQRS handlers using **MediatR** with Request/Handler/Response types. FluentValidation validators run via `ValidationBehaviour`. Performance and logging pipeline behaviours are also wired here.

**Argon.Infrastructure** — EF Core 8 with PostgreSQL (Npgsql). `ApplicationDbContext` is the main DbContext. Entity configurations use fluent API. `AuditableEntitySaveChangesInterceptor` sets Created/LastModified on save.

**Argon.WebApi** — ASP.NET Core controllers (one per feature). NSwag generates the OpenAPI spec, which in turn generates the TypeScript client for the frontend. JWT Bearer authentication with CORS. Health endpoint at `/healthz`.

**Argon.Application.Tests** — NUnit + Moq + FluentAssertions + InMemory EF Core.

### Request lifecycle (example)

```
HTTP GET /transactions
  → TransactionsController.GetList(request)
  → MediatR.Send(TransactionsGetListRequest)
  → ValidationBehaviour (FluentValidation)
  → TransactionsGetListHandler (queries IApplicationDbContext)
  → EF Core → PostgreSQL
  → TransactionsGetListResponse (paginated DTO)
```

### Frontend — React + MUI + React Query

Entry point: `src/main.tsx` — mounts nested providers: AuthProvider (OIDC) → ThemeProvider → QueryClientProvider → MainRouter.

**Authentication**: OAuth 2.0 via `react-oidc-context` using `WebStorageStateStore` (localStorage). All routes are wrapped in `ProtectedRoute`.

**API layer**: `BackendClient` in `src/services/backend/BackendClient.ts` is auto-generated from the NSwag spec. React Query v5 wraps these calls for caching and mutations.

**State**: Server state is managed entirely by React Query. There is no global client-side state manager.

**UI**: Material-UI v6. Internationalization via i18next.

### Key files for orientation

| File | Purpose |
|---|---|
| `src/Argon.WebApi/Startup.cs` | Service registration and middleware pipeline |
| `src/Argon.WebApi/nswag.json` | NSwag config — triggers TypeScript client regen |
| `src/Argon.Application/Common/Behaviours/` | MediatR pipeline: validation, logging, performance |
| `src/Argon.Infrastructure/Persistence/ApplicationDbContext.cs` | DbContext with all DbSets |
| `src/Argon.WebGui/src/router/index.tsx` | All frontend routes |
| `src/Argon.WebGui/src/services/backend/BackendClient.ts` | Auto-generated API client (do not edit manually) |

## Database

PostgreSQL 15+. EF Core migrations live in `src/Argon.Infrastructure/Persistence/Migrations/`. `ApplicationDbContextInitializer` (called from Startup) applies pending migrations and seeds initial data on startup.

## Code Generation

When backend API contracts change, rebuild the WebApi project — this triggers NSwag to regenerate the TypeScript client automatically. The generated file is `src/Argon.WebGui/src/services/backend/BackendClient.ts`.

**Never edit generated files manually.** Any manual changes will be overwritten on the next build.

## Identity

Authentication is handled by **Authentik** (self-hosted IdP). The frontend uses OIDC via `react-oidc-context`. The API validates JWT Bearer tokens issued by Authentik. Any new client (CLI, worker service) should register as an OAuth 2.0 application in Authentik — no stored credentials, no `.env` token hacks.

## Dev secrets

`Argon.WebApi` has a `<UserSecretsId>` in its csproj and the default ASP.NET builder loads .NET user-secrets automatically when the environment is `Development`. Real values for `Auth:Authority`, `Auth:ClientId`, and any local connection-string overrides belong in user-secrets, not in `appsettings.json`. The root `README.md` lists the `dotnet user-secrets set …` commands.

## Git commits

This repository uses **Conventional Commits** (`<type>(<scope>): <subject>`). Common types: `feat`, `fix`, `refactor`, `chore`, `docs`, `test`. The scope is the affected area in kebab-case (e.g. `parsers`, `cli`, `webapi`, `webgui`). Examples from the history:

```
feat(parsers): added incoming bank transfers parsing logic
feat(parsers): added Visa credit card parsing logic
feat(parsers): added bank commission parsing logic
```

When Claude is the one creating the commit, the message **must** end with a `Co-Authored-By` trailer for attribution:

```
Co-Authored-By: Claude Opus 4.7 (1M context) <noreply@anthropic.com>
```

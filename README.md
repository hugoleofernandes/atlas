# Atlas

**A production-ready .NET 10 modular monolith starter for multi-tenant SaaS.**

Atlas is an opinionated foundation for teams who want to ship a real product — not wire up boilerplate. Every architectural decision is explicit, documented, and designed to scale from a single-tenant MVP to a full multi-tenant SaaS without a rewrite.

---

## What is Atlas?

Atlas is a **modular monolith** — bounded contexts share a process and a database, but they never call each other directly. Modules communicate exclusively through the **Outbox Pattern**, which gives you:

- **Atomic consistency** — the domain change and the integration event are committed in the same transaction. No lost events, ever.
- **Reliable fan-out** — one event can trigger multiple independent handlers across modules (e.g. "user created" → send welcome email + create staff member).
- **Natural path to microservices** — the module boundary is already drawn. Extract a module when you genuinely need to, not before.

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│  Atlas.API  (ASP.NET Core 10)                                   │
│  ┌──────────┐  ┌────────────┐  ┌──────────────────────────────┐ │
│  │ Identity │  │   Staff    │  │ Security · OIDC · Rate Limit │ │
│  │  Module  │  │  Module    │  │ CORS · Headers · i18n        │ │
│  └────┬─────┘  └─────┬──────┘  └──────────────────────────────┘ │
│       │              │                                           │
│       └──────────────┴──────────── PostgreSQL ──────────────────┤
│                                   (domain + outbox tables)      │
└─────────────────────────────────────────────────────────────────┘
                                        │ outbox rows polled
                                        ▼
┌─────────────────────────────────────────────────────────────────┐
│  Atlas.Outbox.Worker  (BackgroundService)                       │
│  Dispatcher → fan-out → handler pipeline                        │
│  [IdempotencyDecorator → ValidationDecorator → PersistDb]      │
└─────────────────────────────────────────────────────────────────┘
                                        │
                         ┌──────────────┴──────────────┐
                         ▼                             ▼
              SendWelcomeEmail              CreateStaffMember
              (Identity module)             (Staff module)
```

---

## Features

### Architecture & Design Patterns
- **Modular Monolith** following Clean Architecture and DDD
- **CQRS** — commands and queries routed through a custom `HandlerInvoker` (no MediatR dependency)
- **Outbox Pattern** with an **Attempt-Chain** retry model — each attempt is a separate, immutable row; the full processing history is queryable
- **Domain Events** → **Integration Events** pipeline, decoupled via the outbox
- **Fan-out** — a single integration event dispatches to multiple independent handlers
- **Building Blocks** — reusable cross-cutting packages (`SharedKernel`, `BuildingBlocks.Persistence`, `BuildingBlocks.Infrastructure`, `BuildingBlocks.AspNetCore`, `BuildingBlocks.Observability`)

### Custom Handler Pipeline
Every command and integration-event adapter runs through the same explicit decorator chain — no magic, no hidden middleware:

```
IdempotencyDecorator   → skip if (IdempotencyKey, HandlerName) already processed
  ValidationDecorator  → FluentValidation (opt-in, no-op if no validator registered)
    PersistDbDecorator → UnitOfWork.SaveChangesAsync (NullUnitOfWork for side-effect-only handlers)
      handler
        OutputTransformDecorator → Result<T> boundary
          DomainExceptionDecorator
            LoggingDecorator    → structured Serilog log per handler
              TelemetryDecorator → OpenTelemetry span per handler
```

### Multi-Tenancy
- Every aggregate root is scoped to a `TenantId`
- `TenantResolverMiddleware` resolves the tenant from OIDC claims
- `UserBootstrapMiddleware` lazily provisions the `User` record on first login
- `MultiTenantDbContext` applies a global EF Core query filter — tenant isolation is automatic and cannot be accidentally bypassed

### Authentication & Security
- **Microsoft Entra ID** (Azure AD) via OIDC — one issuer per tenant, multi-tenant login
- Prepared for additional OIDC providers (e.g. AWS Cognito) — pluggable `ITenantConfigurator`
- **Dual cookie** authentication (session + anti-CSRF)
- **JWT** token validation
- **Rate limiting** (per endpoint, per tenant)
- **CORS** — per-environment configuration
- **Security headers** — CSP, X-Frame-Options, HSTS, Referrer-Policy

### Observability (Grafana Stack)
- **Traces** → Grafana Tempo via OpenTelemetry (OTLP)
- **Logs** → Grafana Loki via OpenTelemetry + Serilog
- **Metrics** → Prometheus + Grafana dashboards
- `CorrelationId` propagated from the HTTP request through the outbox message to every worker span and log entry — one search finds the full flow
- Fallback to local file logs when Grafana Cloud is not configured

### Data & Persistence
- **PostgreSQL** with **Entity Framework Core 10** (Npgsql)
- **Audit trail** — every entity change stamped with `CreatedBy`, `UpdatedAt`, `TenantId`
- **PII protection** — `ILogSummary` interface lets each entity control what properties are included in structured logs, preventing accidental PII leakage (LGPD / GDPR readiness)
- Outbox rows include `CorrelationId`, `TenantId`, `UserId`, and a full execution history table (`outbox_handler_executions`)

### Developer Experience
- **Idempotency** — built into the handler pipeline. Handlers opt in via `IIdempotentHandler`. No framework ceremony.
- **FluentValidation** — validators are auto-discovered; the pipeline invokes them before the handler runs
- **Problem Details** — all errors return `application/problem+json` (RFC 9457) with a structured `ErrorCode` and localized message
- **Global exception middleware** — no unhandled exception leaks to the client
- **Internationalization** — error messages localized via `.resx` files, keyed by `ErrorCode`, resolved from `Accept-Language`
- **Unit tests** — AAA pattern, xUnit, NSubstitute; test projects mirror the source structure
- **Scalar API UI** — interactive API docs available in development (`/scalar`)
- **.NET 10** — latest SDK and runtime

---

## Solution Structure

```
atlas/
├── infrastructure/
│   └── docker-compose.dev.yml      # PostgreSQL + pgAdmin + Grafana stack
└── source-code/
    └── src/
        ├── Atlas.API                           # Entry point — HTTP, middlewares, controllers
        │
        ├── Atlas.Identity.Domain               # Identity bounded context — domain layer
        ├── Atlas.Identity.Application          # Identity — use cases, commands, validators
        ├── Atlas.Identity.Infrastructure       # Identity — EF Core, repositories, migrations
        │
        ├── Atlas.Staff.Domain                  # Staff bounded context — domain layer
        ├── Atlas.Staff.Application             # Staff — use cases, commands, queries
        ├── Atlas.Staff.Infrastructure          # Staff — EF Core, repositories
        │
        ├── Atlas.Outbox.Worker                 # BackgroundService — polls and dispatches outbox
        ├── Atlas.Outbox.Application            # ProcessOutbox command handler
        ├── Atlas.Outbox.Infrastructure         # Dispatcher, locking, retry logic
        ├── Atlas.Outbox.Publisher.Identity     # Publish side: domain event → outbox message
        ├── Atlas.Outbox.Consumer.Identity      # Consume side: integration event → adapters (fan-out)
        │
        ├── Atlas.Contracts                     # Shared integration event DTOs
        ├── Atlas.SharedKernel                  # Result<T>, DomainException, IUnitOfWork, base types
        │
        ├── Atlas.BuildingBlocks.Infrastructure # HandlerInvoker, decorators, pipeline
        ├── Atlas.BuildingBlocks.Persistence    # MultiTenantDbContext, AuditTrail, OutboxRepo
        ├── Atlas.BuildingBlocks.AspNetCore     # Middlewares, OIDC, security, error filters
        └── Atlas.BuildingBlocks.Observability  # OTel setup, Serilog sinks, Grafana config
```

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### 1 — Start the infrastructure

```bash
docker compose -f infrastructure/docker-compose.dev.yml up -d
```

This starts:
| Service | URL |
|---|---|
| PostgreSQL | `localhost:5432` |
| pgAdmin | `http://localhost:5050` |
| Grafana | `http://localhost:3000` |
| Prometheus | `http://localhost:9090` |

### 2 — Configure secrets

```bash
cd source-code/src/Atlas.API
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Database=atlas;Username=postgres;Password=postgres"
dotnet user-secrets set "Tenants:0:Authority" "https://login.microsoftonline.com/<your-tenant-id>/v2.0"
dotnet user-secrets set "Tenants:0:ClientId" "<your-client-id>"
```

### 3 — Run

```bash
# API (auto-migrates the database on first run in Development)
cd source-code/src/Atlas.API
dotnet run

# Outbox Worker (separate terminal)
cd source-code/src/Atlas.Outbox.Worker
dotnet run
```

API: `https://localhost:7XXX`  
Scalar docs: `https://localhost:7XXX/scalar`

---

## Modules

### Identity
Manages tenants, users, roles, and invitations.

Key flows:
- **Tenant onboarding** — first login via Entra ID resolves or creates the `Tenant` and `User` records
- **Invite user** — generates an invitation link; when followed, creates the `User` and raises `UserCreatedFromInvitationDomainEvent`
- **Send welcome email** — triggered by the integration event (OutboxWorker)

### Staff
Manages `StaffMember` records associated with a tenant.

Key flows:
- **Create staff member** — triggered when a user accepts an invitation (integration event fan-out from Identity)

---

## Integration Event Flow (Outbox Pattern)

```
[API request]
  1. Aggregate raises DomainEvent
  2. IntegrationEventEnqueuer maps it to an OutboxMessage
  3. EF Core saves domain changes + OutboxMessage in one transaction

[OutboxWorker — polling loop]
  4. Picks up pending OutboxMessage rows (SELECT … FOR UPDATE SKIP LOCKED)
  5. Restores CorrelationId / TenantId / UserId context from the message
  6. Dispatcher deserializes payload → resolves all IIntegrationEventHandler<TEvent>
  7. Each adapter maps the event to a Command → calls HandlerInvoker
  8. HandlerInvoker runs the full pipeline per handler (idempotency → validation → persist)
  9. Execution result recorded in outbox_handler_executions (one row per handler per attempt)
 10. On partial failure → CreateRetryAttempt() (same IdempotencyKey, AttemptNumber + 1)
 11. On max retries → MarkAsDeadLettered()
```

The `IdempotencyKey` is stable across all retry attempts. Handlers that already succeeded are skipped automatically on retries.

---

## Roadmap

- [x] Distributed trace propagation — `traceparent` stored in `OutboxMessage`; OutboxWorker restores it as parent context so Grafana Tempo shows API → Worker as a single end-to-end trace
- [ ] AWS Cognito OIDC provider implementation
- [ ] Email infrastructure (`IEmailService`)
- [ ] Tenant self-service onboarding UI
- [ ] Role management endpoints
- [ ] Archival / cleanup job for processed outbox rows

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 |
| API framework | ASP.NET Core 10 |
| ORM | Entity Framework Core 10 + Npgsql |
| Database | PostgreSQL 16 |
| Auth | OIDC (Microsoft Entra ID) · JWT · Cookie |
| Validation | FluentValidation 12 |
| Logging | Serilog → Grafana Loki (OTLP) |
| Tracing | OpenTelemetry → Grafana Tempo (OTLP) |
| Metrics | OpenTelemetry → Prometheus → Grafana |
| Testing | xUnit · NSubstitute · AAA pattern |
| Containerization | Docker Compose |
| API docs | Scalar (OpenAPI 3.1) |

---

## License

MIT — use it, fork it, ship it.

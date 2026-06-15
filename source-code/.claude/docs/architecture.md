# Architecture

## Foundational Principles

Atlas is a **modular monolith** built on **Clean Architecture** with **light CQRS**.

- **Clean Architecture**: dependencies flow inward only. Outer layers know about inner layers; inner layers know nothing about outer layers. Domain is the center — it has no dependencies on infrastructure, frameworks, or other modules.
- **Light CQRS**: reads and writes are separated into two distinct handler types (QueryHandler and CommandHandler) but share the same process and database. No event sourcing, no separate read database.
- **Modular Monolith**: each module is an isolated vertical slice. Modules do not reference each other. They communicate through the Outbox (async) or through the API layer (sync orchestration).

---

## Solution Structure

The `src/` folder has four layers. **Upper layers may reference lower layers; never the reverse.**

```
src/
├── Atlas.API              ← 1. Executables
├── Atlas.API.Tests        ←    (tests for the executable)
├── Identity/              ← 2. Modules
├── Staff/                 ←    (modules are peers — they cannot reference each other)
├── Platform/
├── Outbox/                ←    (special module — see below)
├── BuildingBlocks/        ← 3. BuildingBlocks
└── Shared/                ← 4. Shared
    └── Atlas.SharedKernel
```

### Layer Rules

| Layer | Can reference |
|---|---|
| **Executables** (`Atlas.API`) | Modules, BuildingBlocks, SharedKernel |
| **Modules** (`Identity`, `Staff`, `Platform`) | BuildingBlocks, SharedKernel — **never another Module** |
| **BuildingBlocks** | SharedKernel only — **never another BuildingBlock** |
| **SharedKernel** | Nothing — zero dependencies |

### Outbox Exception

`Outbox` is a special module. It is allowed to reference other modules' contracts (e.g., `Atlas.Identity.Contracts`, `Atlas.Staff.Contracts`) because its job is to dispatch integration events to their target handlers. This is the **only** permitted cross-module reference in the solution.

---

## Module Internal Structure

Each module (`Identity`, `Staff`, `Platform`) follows the same internal layer hierarchy.

```
src/{Module}/
├── Atlas.{Module}.BffApi         ← 1. API (BFF-facing)
├── Atlas.{Module}.InternalApi    ← 1. API (internal, service-to-service)
├── Atlas.{Module}.Infrastructure ← 2. Infrastructure
├── Atlas.{Module}.Application    ← 3. Application (AppServices)
├── Atlas.{Module}.Domain         ← 4. Domain
├── Atlas.{Module}.Contracts      ← 5. Contracts
└── Atlas.{Module}.Resources      ←    (resx files — no code)
```

**Dependency rule within a module:**

- `BffApi` / `InternalApi` may reference all layers below them.
- `Infrastructure` may reference `Application`, `Domain`, and `Contracts` — it implements interfaces defined in Application and Domain.
- `Application` may reference `Domain` and `Contracts` only.
- `Domain` may reference `Contracts` only.
- **`Contracts` may not reference any other project** — it is the public interface of the module, consumed by other modules and the Outbox. Zero dependencies.

### What Each Layer Contains

| Layer | Responsibilities |
|---|---|
| **BffApi / InternalApi** | FastEndpoints endpoints, request/response types, DI wiring for the API surface |
| **Infrastructure** | EF Core DbContext, repositories, Dapper readers, email/external integrations |
| **Application** | Command handlers, query handlers, domain services |
| **Domain** | Aggregate roots, entities, value objects, domain events, repository interfaces |
| **Contracts** | Integration event types, public DTOs shared across modules |

### BffApi vs InternalApi

Every module exposes two distinct API surfaces, both using FastEndpoints:

- **BffApi** (`Atlas.{Module}.BffApi`) — serves the frontend (BFF pattern). Endpoints here are user-facing: they enforce authentication, RBAC permissions, and tenant context. This is the primary API surface.
- **InternalApi** (`Atlas.{Module}.InternalApi`) — serves service-to-service calls. Today its main consumer is the Outbox when configured in HTTP dispatch mode. Endpoints here use a different security model (internal API key / no OIDC token required).

---

## Atlas.API — The Convergence Point

`Atlas.API` is the single executable that hosts the entire application. It does not contain business logic — its job is to assemble and start all modules.

It references every module's `BffApi` and `InternalApi` directly, which registers their FastEndpoints routes into the shared pipeline. The result is one HTTP host exposing all module endpoints under a unified base URL.

```
Atlas.API
├── references Atlas.Identity.BffApi      → /identity/...   (user-facing)
├── references Atlas.Identity.InternalApi → /internal/...   (service-to-service)
├── references Atlas.Staff.BffApi
├── references Atlas.Staff.InternalApi
├── references Atlas.Platform.BffApi
├── references Atlas.Platform.InternalApi
└── owns: Program.cs, global middleware, health checks, seeding, OIDC config
```

Cross-cutting concerns owned by `Atlas.API`: CORS, rate limiting, security headers, OIDC provider config, global exception middleware, health check endpoints, and bootstrap seeding.

---

## Outbox — Integration Event Processing

The Outbox is a **separate Background Service** (`Atlas.Outbox.Service`) that runs alongside `Atlas.API`. Its job is to connect module behaviors in the background by processing integration events that were written to the outbox table by business operations.

### Why the Outbox exists

When a command handler completes a domain operation, it raises a domain event which may produce an integration event (e.g., `UserInvitedIntegrationEvent`). Rather than coupling that handler to downstream side effects (sending emails, notifying other modules), the event is persisted to the outbox table. The Outbox Service picks it up asynchronously and dispatches it to the registered targets.

### Processing Workflow (4 steps)

For each module, a `ModuleOutboxBackgroundService` polls on a configurable interval and runs `OutboxProcessingWorkflow`:

```
Step 1 — Fetch pending messages
         Lock a batch of outbox rows for the current module (optimistic concurrency via LockedUntil)

Step 2 — Resolve targets (fan-out)
         One integration event → N registered targets
         e.g., UserInvitedIntegrationEvent → [SendInvitationEmail, NotifyAdmin]

Step 3 — Dispatch to each target
         Each target maps the event payload to a Command and executes it via IHandlerInvoker
         Two dispatch modes exist (see below)

Step 4 — Update message status
         Success → mark processed
         Failure → increment retry counter; after MaxRetries → move to Dead Letters
```

### Fan-out Pattern

A single integration event can have multiple targets registered in the `TargetCatalog`. Each target is dispatched independently — one failure does not block the others. The final status of the outbox message reflects whether all targets succeeded.

### Target Dispatch Modes

**Direct mode (current)** — `DirectTargetExecutor`  
The target handler (`OutboxTargetHandler<TEvent, TCommand>`) lives in `Atlas.Outbox.Targets.{Module}` and references the module's Application layer directly. It deserializes the event, maps it to a command, and calls the command handler via `IHandlerInvoker`. No HTTP involved — in-process call.

```csharp
// Example: Atlas.Outbox.Targets.Identity
public sealed class SendInvitationEmailTargetHandler(...)
    : OutboxTargetHandler<UserInvitedIntegrationEvent, SendInvitationEmailCommand>(...)
{
    protected override SendInvitationEmailCommand MapToCommand(UserInvitedIntegrationEvent @event) =>
        new(@event.TenantId, @event.Email);
}
```

**HTTP mode (future)** — `HttpTargetExecutor`  
The infrastructure exists: `HttpTargetExecutor` dispatches to a configured URL via `IInternalApiInvoker`. This is how the InternalApi layer will be consumed when modules are split into separate processes. Today it is not in active use.

### Dead Letters

Messages that exceed `MaxRetries` are moved to a Dead Letters table. The Outbox module exposes query and command handlers to list and resubmit dead letters (`ListDeadLettersQueryHandler`, `ResubmitDeadLetterCommandHandler`).

---

## Handler Pipeline (HandlerInvoker)

**All handlers must be invoked through `IHandlerInvoker`** — never instantiated or called directly. The invoker runs every handler through a decorator pipeline that guarantees consistent observability and behavior.

```
IHandlerInvoker.InvokeAsync(handler, input, ct)
  └─ TelemetryDecorator
       └─ LoggingDecorator
            └─ DomainExceptionDecorator
                 └─ OutputTransformDecorator     (TOutput → Result<TOutput>)
                      └─ [IdempotencyDecorator]  ← commands only (IIdempotentHandler opt-in)
                           └─ [ValidationDecorator]   ← commands only (IValidator opt-in)
                                └─ [PersistDbDecorator]    ← commands only (UoW.SaveChangesAsync)
                                     └─ handler
```

- **QueryHandlers** skip idempotency, validation, and UoW — they get observability only.
- **CommandHandlers** go through the full pipeline. Each inner decorator is a no-op when not applicable (e.g., no `IValidator` registered → validation step passes through).
- All handlers return `Result<TOutput>` — uniform contract for endpoints.

---

## QueryHandler Pattern

QueryHandlers are for **read operations only**. They always use Dapper (never EF Core) for maximum query performance.

### Strict Ownership

Every QueryHandler owns three private artifacts — they are created exclusively for that handler and never shared:

```
{Feature}/Handlers/Queries/{QueryName}/
├── I{QueryName}QueryHandler.cs    ← interface
├── {QueryName}QueryHandler.cs     ← implementation
├── {QueryName}Query.cs            ← input (the query object)
├── I{QueryName}Reader.cs          ← reader interface
├── {QueryName}Reader.cs           ← reader implementation (Infrastructure layer)
└── {QueryName}Dto.cs              ← return DTO
```

- The **Reader** is an infrastructure concern — it lives in `Atlas.{Module}.Infrastructure`, co-located with the handler folder.
- The **DTO** is shaped for that exact query's use case. Never reuse a DTO between two query handlers — projections will diverge.
- QueryHandlers **never call repositories** and **never call CommandHandlers**.

### Return Type

QueryHandlers return `IReadOnlyList<TDto>` for lists or `TDto` for single results. Never return domain objects.

---

## CommandHandler Pattern

CommandHandlers are for **write operations** (insert, update, delete). They orchestrate the domain layer and persist the result via repositories.

### What a CommandHandler orchestrates

A CommandHandler is the entry point into the domain for a write operation. Its job is to coordinate:

- **Repositories** — load aggregate roots via EF Core, persist changes after domain logic runs.
- **Aggregate roots** — call domain methods that encapsulate business rules and raise domain events.
- **Domain services** — for logic that spans multiple aggregates or doesn't belong on a single root.
- **Factories** — when construction of a domain object is complex or requires domain rules.
- **Value objects** — created and passed into aggregate methods as part of the command execution.

The handler itself contains no business logic — it delegates everything to the domain layer and uses repositories only for loading and saving.

### Strict Ownership

Every CommandHandler owns these private artifacts — never shared with other handlers:

```
{Feature}/Handlers/Commands/{CommandName}/
├── I{CommandName}CommandHandler.cs
├── {CommandName}CommandHandler.cs
├── {CommandName}Command.cs         ← input
├── {CommandName}Output.cs          ← output (if the command returns data)
└── {CommandName}Validator.cs       ← FluentValidation (opt-in)
```

- CommandHandlers **never call Readers** — they call repositories that return domain objects.
- CommandHandlers **never call QueryHandlers**.

### Hard Boundary

```
QueryHandler  ──✗──►  CommandHandler   (forbidden)
CommandHandler ──✗──►  QueryHandler    (forbidden)
```

If you need to call two handlers in sequence, orchestrate at the **API layer** (inside an endpoint) or create a **Workflow class** in the Application layer. A Workflow is a plain class (not a handler) that coordinates multiple handler calls via `IHandlerInvoker`.

---

## Handler Naming Summary

| Type | Input | Output (exclusive) | Infrastructure (exclusive) |
|---|---|---|---|
| QueryHandler | `{Name}Query` | `{Name}Dto` | `I{Name}Reader` + `{Name}Reader` |
| CommandHandler | `{Name}Command` | `{Name}Output` | — (uses shared repositories) |

Validators (`{Name}Validator`) are optional and opt-in per command handler.

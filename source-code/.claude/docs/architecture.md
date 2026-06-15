# Architecture

## Rules

✅ Dependencies flow inward only — outer layers reference inner layers, never the reverse
✅ Modules are peers — they cannot reference each other
✅ The only permitted cross-module reference is `Outbox` referencing module Contracts
✅ `Contracts` has zero dependencies — it is the public interface consumed by other modules
✅ `Atlas.API` may orchestrate handlers from multiple modules when exposing a unified HTTP contract
❌ Never add a project reference from a module to another module
❌ Never add a project reference from a BuildingBlock to another BuildingBlock
❌ Never call a QueryHandler from a CommandHandler or vice versa — orchestrate via Workflow or endpoint

## Solution Layers

```
src/
├── Atlas.API              ← 1. Executables    — references all modules
├── Identity/              ← 2. Modules        — peers, cannot reference each other
├── Staff/
├── Platform/
├── Outbox/                ←    (exception: may reference module Contracts)
├── BuildingBlocks/        ← 3. BuildingBlocks — reference SharedKernel only
└── Shared/Atlas.SharedKernel  ← 4. Shared     — zero dependencies
```

| Layer | Can reference |
|---|---|
| Executables | Modules, BuildingBlocks, SharedKernel |
| Modules | BuildingBlocks, SharedKernel — **never another Module** |
| BuildingBlocks | SharedKernel only — **never another BuildingBlock** |
| SharedKernel | Nothing |

## Module Internal Layers

```
Atlas.{Module}.BffApi         ← user-facing endpoints (auth, RBAC, tenant context)
Atlas.{Module}.InternalApi    ← service-to-service endpoints (API key, no OIDC)
Atlas.{Module}.Infrastructure ← EF Core, repositories, Dapper readers, integrations
Atlas.{Module}.Application    ← command handlers, query handlers, domain services
Atlas.{Module}.Domain         ← aggregates, value objects, domain events
Atlas.{Module}.Contracts      ← integration events, public DTOs (zero dependencies)
Atlas.{Module}.Resources      ← .resx files only, no code
```

| Layer | Can reference |
|---|---|
| BffApi / InternalApi | All layers below |
| Infrastructure | Application, Domain, Contracts |
| Application | Domain, Contracts |
| Domain | Contracts only |
| Contracts | Nothing |

## Atlas.API — Convergence Point

Single executable. No business logic. Assembles all modules into one HTTP host:
- References every `{Module}.BffApi` → registers user-facing routes
- References every `{Module}.InternalApi` → registers service-to-service routes
- Owns: `Program.cs`, global middleware, health checks, seeding, OIDC config

`Atlas.API` may also expose convenience endpoints that orchestrate handlers from multiple modules.
That orchestration belongs in the API layer only:
- route by `ModuleId`, `EntityTypeId`, or another neutral routing key
- authorize per target module
- invoke the selected module handler via `IHandlerInvoker`
- never move module business rules into `Atlas.API`

## Outbox — Background Service

`Atlas.Outbox.Service` runs alongside `Atlas.API` and processes integration events asynchronously.

**4-step workflow per module:**
```
1. Fetch — lock a batch of pending outbox rows (LockedUntil for concurrency)
2. Fan-out — one integration event → N registered targets (dispatched independently)
3. Dispatch — each target maps the event to a Command, executes via IHandlerInvoker
4. Update — success → mark processed; failure → retry; MaxRetries exceeded → Dead Letters
```

**Target handler (Direct mode — current):**
```csharp
// Atlas.Outbox.Targets.{Module}
public sealed class SendInvitationEmailTargetHandler(...)
    : OutboxTargetHandler<UserInvitedIntegrationEvent, SendInvitationEmailCommand>(...)
{
    protected override SendInvitationEmailCommand MapToCommand(UserInvitedIntegrationEvent @event)
        => new(@event.TenantId, @event.Email);
}
```

## Handler Boundaries

```
QueryHandler  → reads only, Dapper, returns IReadOnlyList<Dto> or Dto
CommandHandler → writes only, EF Core via repositories, returns Output

QueryHandler  ──✕──▶ CommandHandler   forbidden
CommandHandler ──✕──▶ QueryHandler    forbidden
```

To orchestrate multiple handlers: use a **Workflow** (plain class in Application) or the **endpoint**.

When the orchestration spans multiple modules, prefer an endpoint in `Atlas.API`.

## Naming

| Type | Input | Output | Infrastructure |
|---|---|---|---|
| QueryHandler | `{Name}Query` | `{Name}Dto` | `I{Name}Reader` + `{Name}Reader` |
| CommandHandler | `{Name}Command` | `{Name}Output` | — (shared repositories) |

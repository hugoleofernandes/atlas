# Atlas

Modular monolith built with **.NET 10**, **Clean Architecture**, **DDD**, and the **Outbox pattern**.
Multi-tenant via **OIDC (Microsoft Entra ID)**. Inter-module communication is asynchronous via the outbox — no direct calls between modules.

---

## Modules

| Module | Responsibility |
|---|---|
| **Identity** | Tenants, users, invitations, access resolution from OIDC claims |
| **Staff** | Staff members — created reactively when a user is onboarded in Identity |

Cross-cutting:

- `Atlas.SharedKernel` — domain primitives, `Result<T>`, `ErrorDefinition`, outbox interfaces
- `Atlas.BuildingBlocks.Persistence` — multi-tenant DbContext, audit, outbox storage
- `Atlas.BuildingBlocks.Infrastructure` — enqueuer, validation extensions
- `Atlas.OutboxWorker` — background process that dispatches integration events

---

## Quickstart

```bash
# Restore + build
dotnet build

# Apply migrations
dotnet ef database update --project src/Atlas.Identity.Infrastructure --startup-project src/Atlas.API
dotnet ef database update --project src/Atlas.Staff.Infrastructure --startup-project src/Atlas.API

# Run API
dotnet run --project src/Atlas.API

# Run outbox worker (separate terminal)
dotnet run --project src/Atlas.OutboxWorker

# Run tests
dotnet test
```

---

## Documentation

Full docs are generated with **DocFX** under `/docs`.

```bash
cd docs
docfx docfx.json --serve
# open http://localhost:8080
```

Quick links:

- [Overview](docs/overview.md) — system architecture
- [Identity module](docs/modules/identity/index.md) — domain model + invariants
- [Staff module](docs/modules/staff/index.md) — domain model + cross-module integration
- [Flows](docs/flows/) — end-to-end sequence diagrams
- [Guidelines](docs/guidelines/) — DDD, testing, design rules

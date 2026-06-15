# Domain Events & Integration Events

## Rules

✅ Domain events live in `Atlas.{Module}.Domain/{Aggregate}/Events/`
✅ Integration events live in `Atlas.{Module}.Contracts/IntegrationEvents/{Aggregate}/`
✅ Integration events are `record` types — only fields consumers need
✅ Mappers live in `Atlas.{Module}.OutboxPublisher/{Aggregate}/{EventName}/`
✅ Every mapper must be registered in `{Module}OutboxPublisherDependencyInjection`
❌ Never raise an integration event directly — they are produced by mappers only
❌ Never put full aggregate state in a domain event — only what the mapper needs
❌ Never skip the mapper — CommandHandlers raise domain events, not integration events

## Two Event Types

| | Domain Event | Integration Event |
|---|---|---|
| Lives in | `Domain` | `Contracts` |
| Raised by | Aggregate root method | Never directly — mapper produces it |
| Consumed by | Mapper (OutboxPublisher) | Outbox Target Handlers |
| Persistence | In-memory until SaveChanges | Written to outbox table as JSON |

## Flow

```
Aggregate.Create() raises DomainEvent (in-memory)
  └─ PersistDbDecorator calls SaveChangesAsync()
       └─ Mapper converts DomainEvent → IntegrationEvent → OutboxMessage (same transaction)
            └─ Outbox Service picks it up → dispatches to target handlers
```

## How to Add a New Integration Event (5 steps)

**1 — Domain event** in `Atlas.{Module}.Domain/{Aggregate}/Events/`
```csharp
public sealed class UserInvitedDomainEvent(Guid tenantId, string email) : IDomainEvent
{
    public Guid TenantId { get; } = tenantId;
    public string Email  { get; } = email;
}
```

**2 — Integration event** in `Atlas.{Module}.Contracts/IntegrationEvents/{Aggregate}/`
```csharp
public sealed record UserInvitedIntegrationEvent(Guid TenantId, string Email);
```

**3 — Mapper** in `Atlas.{Module}.OutboxPublisher/{Aggregate}/{EventName}/`
```csharp
internal sealed class UserInvitedMapper(IOutboxMessageFactory factory) : IIntegrationEventMapper
{
    public Type DomainEventType => typeof(UserInvitedDomainEvent);

    public OutboxMessage Map(IDomainEvent domainEvent)
    {
        var e = (UserInvitedDomainEvent)domainEvent;
        return factory.Create(new UserInvitedIntegrationEvent(e.TenantId, e.Email));
    }
}
```

**4 — Register mapper** in `{Module}OutboxPublisherDependencyInjection`
```csharp
services.AddScoped<IIntegrationEventMapper, UserInvitedMapper>();
```

**5 — Target handler** in `Atlas.Outbox.Targets.{Module}` — see `architecture.md` Outbox section

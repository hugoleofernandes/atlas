# DDD — Aggregate Root vs Child Entity

## Rules

✅ Need `GetByIdAsync` without loading the parent → Aggregate Root
✅ Need a repository for this entity → Aggregate Root (always)
✅ Other aggregates reference it by ID only → Aggregate Root
✅ Every Aggregate Root must inherit from `AggregateRoot`
✅ When creating a new aggregate root, verify the type inherits from `AggregateRoot` before writing its EF configuration
✅ Pure business rules belong in the domain, even when the result is exposed only by a read model
✅ When a business rule can be evaluated from primitive values already loaded by a query, prefer a pure domain method instead of re-implementing the rule in the reader or endpoint
✅ Aggregate roots do not carry infrastructure-only audit-trail opt-in markers — audit registration belongs in EF mapping
✅ Audited aggregate roots are registered for audit in Infrastructure via `AuditedAggregateConfiguration<TEntity>`, not via domain interfaces
❌ Never make an entity a child just because it has a uniqueness invariant with the parent — use pre-check + unique index instead
❌ Never embed an Aggregate Root as a child entity — reference by ID
❌ Never materialize an aggregate only to compute a response flag when a pure domain rule can evaluate the same decision from primitive values
❌ Never model an Aggregate Root as a plain class or child entity base when `AggregateRoot` is the correct abstraction

## Decision Checklist

**YES → Aggregate Root:**

| Signal | Example |
|---|---|
| Independent lifecycle — created/destroyed without going through the parent | `CreateRole` exists outside `Tenant` |
| Queried directly by ID without loading the parent | `GetRoleById` |
| Referenced by ID from other aggregates | `User.RoleId` |
| Has its own write operations (Update/Delete) that don't affect the parent | `UpdateRole`, `RemoveRole` |

**YES → Child Entity:**

| Signal | Example |
|---|---|
| Meaningless without the parent | `Permission` without `Role` |
| Changes must be atomic with the parent | Role + its Permissions saved together |
| Collection is naturally bounded and small | ~10–20 permissions per role |

## Uniqueness Invariants Don't Justify Child

`Role.Name` must be unique per `Tenant` — that does **not** make `Role` a child of `Tenant`. Solve it with:
- Pre-check in CommandHandler: `ExistsWithNameAsync(tenantId, name)`
- Unique index in the database as race condition guard

## TPH Aggregate Hierarchy (Same-Table Subtypes)

A stable identity core can have mutually-exclusive subtypes that share the same table via EF Core's table-per-hierarchy mapping. Example: `Party` (abstract aggregate root) with `Person` and `Organization` as sealed subtypes.

```csharp
public abstract class Party : AggregateRoot, IMultiTenantEntity { ... }
public sealed class Person : Party { ... }
public sealed class Organization : Party { ... }
```

✅ Use when the subtypes are genuinely the same aggregate with a discriminator-driven shape difference — not two unrelated aggregates that happen to share some fields
✅ Repositories and queries are written against the concrete subtype (`IPersonRepository`, `IOrganizationRepository`) — never against the abstract base
✅ The base type carries only the fields and invariants common to every subtype
❌ Never query or repository the abstract base type directly — callers always know which subtype they need

### Multi-Tenant Query Filter — Root Only

EF Core only allows `HasQueryFilter` on the **root** entity type of a TPH hierarchy — never on a derived subtype. The base's filter automatically applies to queries against every subtype, since they share the same table.

`DbContextBase.ApplyMultiTenantFilters` (`Atlas.BuildingBlocks.Persistence`) already skips entity types with a non-null `BaseType` for this reason. If you introduce a new TPH hierarchy, you don't need to do anything extra — just don't apply a second, manual `HasQueryFilter` on the subtype's own `IEntityTypeConfiguration`, or EF Core throws at model-build time (`'GetEntityTypes' ... A filter may only be applied to the root entity type`).

## Domain Rules Used by Read Models

Read models often need derived flags such as `CanResubmit`, `IsExpired`, or `CanBeRevoked`.
If that flag represents a business decision, the rule must still live in the domain.

Preferred pattern:

```csharp
public static bool CanBeResubmitted(bool isDeadLettered, bool hasReplayChild)
    => isDeadLettered && !hasReplayChild;
```

Then the reader or response mapper may call that pure domain rule using values it already has.

Avoid duplicating the same boolean logic directly inside:
- Reader
- Endpoint
- Response mapper

The goal is:
- domain owns the rule
- read side projects the data
- HTTP response only exposes the result

## Whole-Collection Replace for Client-Managed Child Entities

Some child-entity collections are best edited entirely client-side — the UI lets the user add/remove items freely (e.g. a Party's addresses) without a round-trip per item, then submits the final list once together with the rest of the aggregate.

✅ Expose a single aggregate method that replaces the entire collection: `Party.ReplaceAddresses(IReadOnlyList<AddressInput> addresses)`
✅ Validate cross-item invariants (e.g. "only one primary per type") inside that method, before mutating state
✅ Call the same replace method from both the Register and Update command handlers — one codepath, not two
✅ Owned-entity collections (`OwnsMany`) detect a full clear+repopulate automatically via EF Core's change tracker — no manual diffing needed
❌ Never add per-item endpoints (`AddAddress`, `RemoveAddress`) for a collection designed to be managed client-side — that reintroduces the round-trips the design is avoiding
❌ Never replicate the cross-item invariant check in the command handler or endpoint — it belongs in the aggregate method

```csharp
public void ReplaceAddresses(IReadOnlyList<AddressInput> addresses)
{
    foreach (var group in addresses.GroupBy(a => a.Type))
        if (group.Count(a => a.IsPrimary) > 1)
            throw new MultiplePrimaryAddressesException(group.Key);

    _addresses.Clear();
    foreach (var a in addresses)
        _addresses.Add(new Address(Id, a.Type, a.PostalAddress, a.IsPrimary));
}
```

This is still atomic with the parent: the handler calls `ReplaceAddresses` before `SaveChangesAsync`, so the entire Party (including its addresses) is persisted in a single transaction.

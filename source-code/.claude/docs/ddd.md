# DDD — Aggregate Root vs Child Entity

## Rules

✅ Need `GetByIdAsync` without loading the parent → Aggregate Root
✅ Need a repository for this entity → Aggregate Root (always)
✅ Other aggregates reference it by ID only → Aggregate Root
❌ Never make an entity a child just because it has a uniqueness invariant with the parent — use pre-check + unique index instead
❌ Never embed an Aggregate Root as a child entity — reference by ID

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

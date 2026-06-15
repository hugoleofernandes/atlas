# Documentation

## Rules

✅ All documentation and code comments must be written in **English**
✅ Comments explain **intent, invariants, and boundaries** — not implementation
✅ Use the **ubiquitous language** — terms must match the domain model exactly
✅ Keep documentation close to the code — domain rules in XML comments, not in markdown
❌ Never write comments that restate what the code already says
❌ Never document implementation details — document the WHY, not the HOW
❌ Never introduce new terminology without it existing in the domain model
❌ Never duplicate domain rules already documented elsewhere

## Where Each Type of Documentation Lives

| What | Where | Format |
|---|---|---|
| Business intent, invariants, aggregate boundaries | Domain classes | XML `<summary>` |
| Handler responsibilities, what the use case does NOT do | Application handlers | XML `<summary>` |
| Module overview, aggregates, use cases, domain events | `docs/` markdown | Markdown |
| Architecture decisions, boundaries, event flows | `docs/` markdown | Markdown + Mermaid |
| Claude-facing rules and patterns | `.claude/docs/` | Markdown |

## Pattern — XML Comment on Domain Class

```csharp
// ✅ documents intent and invariant
/// <summary>
/// Represents a pending invitation for a user to join a tenant.
/// An invitation is single-use and expires after its TTL.
/// Once used it cannot be revoked or reused.
/// </summary>
public sealed class Invitation : AggregateRoot { ... }

// ❌ restates the code — adds nothing
/// <summary>
/// Invitation class with Id, Email, RoleId and ExpiresAt fields.
/// </summary>
public sealed class Invitation : AggregateRoot { ... }
```

## Pattern — XML Comment on Handler

```csharp
// ✅ documents responsibility and explicit non-scope
/// <summary>
/// Creates a pending invitation for the given email.
/// Does not send the invitation email — that is handled by the Outbox (UserInvitedIntegrationEvent).
/// </summary>
public sealed class InviteUserCommandHandler : IInviteUserCommandHandler { ... }
```

## Anti-patterns

```csharp
// ❌ implementation detail — documents HOW, not WHY
/// <summary>
/// Calls roleRepository.GetByIdWithPermissionsAsync, then checks TenantId,
/// then calls Invitation.Create and adds to the repository.
/// </summary>

// ❌ generic comment that applies to any class
/// <summary>
/// Handles the invite user use case.
/// </summary>

// ❌ wrong language
/// <summary>
/// Cria um convite para o usuário.
/// </summary>
```

## When to Add Comments in Code

Add an XML `<summary>` to:
- Every aggregate root, entity, and value object in the Domain layer
- Every command handler and query handler in the Application layer
- Every public interface that is not self-explanatory from its name

Do **not** add comments to:
- Private methods whose name and signature already describe intent
- DTOs and records with self-explanatory property names
- Endpoint classes — the HTTP verb + route already documents the intent

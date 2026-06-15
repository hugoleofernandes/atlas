# Idempotency

## Rules

✅ Apply `IIdempotentHandler` to Outbox target handlers whose side effects are not naturally idempotent
✅ The deduplication key is composite: `(IdempotencyKey, HandlerName)` — tracked independently per handler
✅ `IIdempotencyContextSetter.Set(...)` is called by the Outbox dispatcher before each target invocation
❌ Never add `IIdempotentHandler` to regular API command handlers — this mechanism is for Outbox retries only
❌ Never add `IIdempotentHandler` to handlers whose operation is naturally idempotent (e.g. setting a field to the same value)

## When to Use

| Side effect | Idempotent? | Add `IIdempotentHandler`? |
|---|---|---|
| Send email | No | ✅ yes |
| Create a record | No | ✅ yes |
| Set a status field to the same value | Yes | ❌ no |
| Regular API command handler | N/A | ❌ never |

## Pattern

```csharp
public sealed class SendInvitationEmailCommandHandler
    : ISendInvitationEmailCommandHandler, IIdempotentHandler  // ← opt-in
{
    public IUnitOfWork UnitOfWork => _uow;

    public async Task<Unit> ExecuteAsync(SendInvitationEmailCommand cmd, CancellationToken ct)
    {
        // runs only once per (IdempotencyKey, HandlerName) pair
        await _emailService.SendAsync(cmd.Email, ct);
        return Unit.Value;
    }
}
```

## How It Works

The `IdempotencyDecorator` runs before validation, persistence, and the handler:

```
[IdempotencyDecorator]   ← atomic INSERT ON CONFLICT DO NOTHING
  [ValidationDecorator]
    [PersistDbDecorator]
      handler
```

- **1 row inserted** → first time this (key, handler) pair is seen → handler runs
- **0 rows inserted** → conflict → already processed → handler skipped, returns `Unit.Value`

One outbox message dispatched to two handlers produces two independent idempotency records.
A retry skips only the handlers that already succeeded — failed ones still run.

# HandlerInvoker

## Rules

✅ Always invoke handlers through `IHandlerInvoker.InvokeAsync` — never call `handler.ExecuteAsync` directly
✅ Use `InvokeOrThrowAsync` in Outbox target handlers — failure must propagate to trigger retry logic
✅ Endpoints, workflows, and the Outbox worker all go through `IHandlerInvoker`
❌ Never call a handler directly — bypasses telemetry, logging, validation, persistence, and domain exception handling
❌ Never call `InvokeAsync` in Outbox target handlers — failures are silently swallowed as success

## Pattern

```csharp
// ✅ correct — always via invoker
var result = await _invoker.InvokeAsync(_handler, command, ct);

// ❌ forbidden — bypasses the entire pipeline
var result = await _handler.ExecuteAsync(command, ct);
```

## Two Pipelines

**Query** (observability only):
```
TelemetryDecorator → LoggingDecorator → DomainExceptionDecorator → OutputTransformDecorator → handler
```

**Command** (full pipeline):
```
TelemetryDecorator → LoggingDecorator → DomainExceptionDecorator → OutputTransformDecorator
  → [IdempotencyDecorator]   ← opt-in: IIdempotentHandler
    → [ValidationDecorator]  ← opt-in: IValidator<TInput> registered in DI
      → PersistDbDecorator   ← calls UnitOfWork.SaveChangesAsync after handler returns
        → handler
```

Optional decorators are safe no-ops when not applicable.

## InvokeOrThrowAsync — Outbox Only

```csharp
// ✅ in OutboxTargetHandler — failure must propagate to trigger retry
await _invoker.InvokeOrThrowAsync(_handler, command, ct);

// ❌ in OutboxTargetHandler — failure swallowed, message marked as success
var result = await _invoker.InvokeAsync(_handler, command, ct);
```

## Workflow Usage

```csharp
public sealed class MyWorkflow(IMyQueryHandler query, IMyCommandHandler command, IHandlerInvoker invoker)
{
    public async Task RunAsync(CancellationToken ct)
    {
        var queryResult   = await invoker.InvokeAsync(query,   new MyQuery(...),   ct);
        var commandResult = await invoker.InvokeAsync(command, new MyCommand(...), ct);
    }
}
```

## Atlas.API Orchestration

When `Atlas.API` routes a single request to one of several module handlers, it must still invoke the chosen handler through `IHandlerInvoker`.

Preferred pattern:

```csharp
var result = await target.ExecuteAsync(command, ct);
```

Where `target.ExecuteAsync(...)` internally calls:

```csharp
invoker.InvokeAsync(moduleHandler, command, ct)
```

This keeps cross-module orchestration compatible with the same telemetry, logging, validation, and persistence pipeline used everywhere else.

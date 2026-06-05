using Atlas.SharedKernel.Application.Idempotency;

namespace Atlas.Outbox.Infrastructure;

/// <summary>
/// Scoped context that holds the idempotency key for the handler currently executing.
/// OutboxMessageDispatcher sets it before invoking each handler.
/// Handlers resolve IIdempotencyContext and IIdempotencyService reads it internally.
/// </summary>
internal sealed class IdempotencyContext : IIdempotencyContext, IIdempotencyContextSetter
{
    private Guid   _idempotencyKey;
    private string _handlerName = string.Empty;

    public Guid   IdempotencyKey => _idempotencyKey;
    public string HandlerName    => _handlerName;

    public void Set(Guid idempotencyKey, string handlerName)
    {
        _idempotencyKey = idempotencyKey;
        _handlerName    = handlerName;
    }
}

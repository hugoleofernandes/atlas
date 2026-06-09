using Atlas.SharedKernel.Application.Idempotency;

namespace Atlas.BuildingBlocks.Application.Idempotency;

/// <summary>
/// Scoped idempotency context populated by delivery adapters before invoking idempotent handlers.
/// </summary>
public sealed class MutableIdempotencyContext : IIdempotencyContext, IIdempotencyContextSetter
{
    private Guid _idempotencyKey;
    private string _handlerName = string.Empty;

    public Guid IdempotencyKey => _idempotencyKey;
    public string HandlerName => _handlerName;

    public void Set(Guid idempotencyKey, string handlerName)
    {
        _idempotencyKey = idempotencyKey;
        _handlerName = handlerName;
    }
}

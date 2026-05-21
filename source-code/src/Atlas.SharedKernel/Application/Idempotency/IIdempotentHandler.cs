namespace Atlas.SharedKernel.Application.Idempotency;

/// <summary>
/// Marker interface for integration event handlers that require automatic
/// idempotency protection.
///
/// When IntegrationIdempotencyDecorator detects this interface on a handler,
/// it checks the idempotency store (IdempotencyKey + HandlerName) before executing
/// HandleAsync. If the pair is already recorded the handler is skipped — no error,
/// no retry consumed.
///
/// Handlers that do NOT implement this interface are invoked unconditionally.
/// Use this marker on any handler whose side effects must not be repeated on retry.
/// </summary>
public interface IIdempotentHandler { }

namespace Atlas.BuildingBlocks.Application.HandlerInvokers;

/// <summary>
/// The outcome of a single integration event handler invocation.
///
/// Built by OutboxMessageDispatcher from the Result&lt;Unit&gt; returned by
/// IHandlerInvoker, then aggregated into a list — one entry per
/// registered handler.
///
/// ProcessOutboxCommandHandler inspects the list to decide whether to mark
/// the OutboxMessage as processed or failed, and to build a structured error
/// payload for the future execution history.
///
/// Note: skipped (idempotency hit) is mapped to Success — both mean the handler
/// completed correctly; the idempotency entry in the database is the record of the skip.
/// </summary>
public sealed record HandlerInvocationResult
{
    /// <summary>Integration event handler type name.</summary>
    public string HandlerName { get; init; } = default!;

    /// <summary>True if the handler ran to completion or was intentionally skipped (idempotency hit).</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Error message when IsSuccess is false.</summary>
    public string? ErrorMessage { get; init; }

    // ── Factory methods ──────────────────────────────────────────────────────

    public static HandlerInvocationResult Success(string handlerName) =>
        new() { HandlerName = handlerName, IsSuccess = true };

    public static HandlerInvocationResult Failure(string handlerName, Exception ex) =>
        new() { HandlerName = handlerName, IsSuccess = false, ErrorMessage = ex.Message };

    public static HandlerInvocationResult Failure(string handlerName, string errorMessage) =>
        new() { HandlerName = handlerName, IsSuccess = false, ErrorMessage = errorMessage };
}

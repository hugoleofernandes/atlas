namespace Atlas.SharedKernel.Application.OutboxMessages;

/// <summary>
/// The outcome of a single integration event handler invocation.
///
/// Built by OutboxMessageDispatcher from the Result&lt;Unit&gt; returned by
/// IHandlerInvoker, then aggregated into a list — one entry per
/// registered handler.
///
/// ProcessOutboxCommandHandler inspects the list to decide whether to mark
/// the OutboxMessage as processed or failed, and to build a structured error
/// payload for the execution history.
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
        new() { HandlerName = handlerName, IsSuccess = false, ErrorMessage = BuildErrorMessage(ex) };

    public static HandlerInvocationResult Failure(string handlerName, string errorMessage) =>
        new() { HandlerName = handlerName, IsSuccess = false, ErrorMessage = errorMessage };

    // ── Private ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Walks the full inner-exception chain and builds a readable message like:
    ///   DbUpdateException: An error occurred while saving the entity changes.
    ///     → PostgresException: 23505: duplicate key value violates unique constraint "IX_staff_members_TenantId_UserId"
    ///
    /// Generic wrapper messages ("See the inner exception for details.") are collapsed
    /// so they don't add noise when a more specific inner message is available.
    ///
    /// AggregateException is unwrapped to its first inner exception — the typical case
    /// for fire-and-forget task wrappers.
    /// </summary>
    private static string BuildErrorMessage(Exception ex)
    {
        var parts   = new List<string>();
        Exception?  current = ex;

        while (current is not null)
        {
            // Unwrap AggregateException to its first real cause
            if (current is AggregateException agg && agg.InnerExceptions.Count > 0)
            {
                current = agg.InnerExceptions[0];
                continue;
            }

            var message = current.Message;

            // Skip vacuous wrapper messages that only say "see inner exception"
            bool isVacuous = current.InnerException is not null
                && message.Contains("See the inner exception", StringComparison.OrdinalIgnoreCase);

            if (!isVacuous)
                parts.Add($"{current.GetType().Name}: {message}");

            current = current.InnerException;
        }

        return parts.Count > 0
            ? string.Join(" → ", parts)
            : ex.Message;
    }
}

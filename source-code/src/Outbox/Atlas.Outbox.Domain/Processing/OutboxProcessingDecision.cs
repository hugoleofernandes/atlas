using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.Outbox.Domain.Processing;

public sealed record OutboxProcessingDecision(
    OutboxMessageFinalStatus Status,
    int FailureCount,
    string? ErrorSummary = null)
{
    public static OutboxProcessingDecision Decide(
        OutboxMessage message,
        IReadOnlyList<HandlerInvocationResult> results,
        int maxRetries)
    {
        var failureCount = results.Count(result => !result.IsSuccess);

        if (failureCount == 0)
            return new OutboxProcessingDecision(OutboxMessageFinalStatus.Processed, failureCount);

        if (message.IsMaxAttemptReached(maxRetries))
            return new OutboxProcessingDecision(OutboxMessageFinalStatus.DeadLettered, failureCount);

        var errorSummary =
            $"{failureCount} of {results.Count} handler(s) failed on attempt {message.AttemptNumber}.";

        return new OutboxProcessingDecision(
            OutboxMessageFinalStatus.RetryScheduled,
            failureCount,
            errorSummary);
    }
}

using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.Outbox.Application.Commands.UpdateOutboxMessageStatus;

public sealed class UpdateOutboxMessageStatusCommandHandler(
    IOutboxWorkerRepository repository,
    IUnitOfWork unitOfWork)
    : IUpdateOutboxMessageStatusCommandHandler
{
    public IUnitOfWork UnitOfWork => unitOfWork;

    public async Task<UpdateOutboxMessageStatusOutput> ExecuteAsync(
        UpdateOutboxMessageStatusCommand command,
        CancellationToken ct)
    {
        var message = await repository.GetByIdAsync(command.Message.Id, ct)
            ?? throw new InvalidOperationException(
                $"Outbox message '{command.Message.Id}' was not found.");

        var executions = BuildExecutions(message.Id, command.Results);
        await repository.AddExecutionsAsync(executions, ct);

        var failureCount = command.Results.Count(result => !result.IsSuccess);

        if (failureCount == 0)
        {
            message.MarkAsProcessed();
            return new UpdateOutboxMessageStatusOutput(
                OutboxMessageFinalStatus.Processed,
                executions.Count,
                failureCount);
        }

        if (message.IsMaxAttemptReached(command.MaxRetries))
        {
            message.MarkAsDeadLettered();
            return new UpdateOutboxMessageStatusOutput(
                OutboxMessageFinalStatus.DeadLettered,
                executions.Count,
                failureCount);
        }

        var errorSummary =
            $"{failureCount} of {command.Results.Count} handler(s) failed on attempt {message.AttemptNumber}.";

        var retry = message.CreateRetryAttempt(errorSummary);
        await repository.AddRetryAsync(retry, ct);

        return new UpdateOutboxMessageStatusOutput(
            OutboxMessageFinalStatus.RetryScheduled,
            executions.Count,
            failureCount);
    }

    private static IReadOnlyList<OutboxHandlerExecution> BuildExecutions(
        Guid outboxMessageId,
        IReadOnlyList<HandlerInvocationResult> results)
    {
        return results
            .Select(result => new OutboxHandlerExecution(
                outboxMessageId,
                result.HandlerName,
                result.IsSuccess,
                result.ErrorMessage))
            .ToList();
    }
}

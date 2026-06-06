using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.OutboxMessages;
using Atlas.Outbox.Domain.Processing;

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

        var decision = OutboxProcessingDecision.Decide(message, command.Results, command.MaxRetries);

        if (decision.Status == OutboxMessageFinalStatus.Processed)
        {
            message.MarkAsProcessed();
            return new UpdateOutboxMessageStatusOutput(
                OutboxMessageFinalStatus.Processed,
                executions.Count,
                decision.FailureCount);
        }

        if (decision.Status == OutboxMessageFinalStatus.DeadLettered)
        {
            message.MarkAsDeadLettered();
            return new UpdateOutboxMessageStatusOutput(
                OutboxMessageFinalStatus.DeadLettered,
                executions.Count,
                decision.FailureCount);
        }

        var retry = message.CreateRetryAttempt(decision.ErrorSummary);
        await repository.AddRetryAsync(retry, ct);

        return new UpdateOutboxMessageStatusOutput(
            OutboxMessageFinalStatus.RetryScheduled,
            executions.Count,
            decision.FailureCount);
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

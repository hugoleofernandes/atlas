using Atlas.Outbox.Application.OutboxMessages;
using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.Outbox.Application.ProcessOutbox;

/// <summary>
/// Reads a batch of pending outbox messages, dispatches each one, and persists the outcome.
///
/// This handler is registered twice in the worker — once per module (Identity, Staff) —
/// each time with a different IOutboxWorkerRepository and UoW pointing to that module's DbContext.
/// The handler itself has no knowledge of which module it is processing.
///
/// Save strategy: one SaveChangesAsync at the end of the batch.
/// A failure mid-batch leaves earlier messages marked but unsaved until the end —
/// acceptable given saves are simple status updates with very low failure probability.
/// </summary>
public sealed class ProcessOutboxCommandHandler : IIdentityOutboxCommandHandler, IStaffOutboxCommandHandler
{
    private readonly IOutboxWorkerRepository _repository;
    private readonly IOutboxMessageDispatcher _dispatcher;

    public ProcessOutboxCommandHandler(
        IOutboxWorkerRepository repository,
        IOutboxMessageDispatcher dispatcher)
    {
        _repository = repository;
        _dispatcher = dispatcher;
    }

    public async Task<ProcessOutboxOutput> ExecuteAsync(ProcessOutboxCommand command, CancellationToken ct)
    {
        var messages = await _repository.GetPendingBatchAsync(
            command.BatchSize, command.LockDuration, ct);

        if (messages.Count == 0)
            return new ProcessOutboxOutput(0, 0, 0);

        var processed    = 0;
        var failed       = 0;
        var deadLettered = 0;

        foreach (var message in messages)
        {
            if (message.HasExceededRetries(command.MaxRetries))
            {
                message.MarkAsDeadLettered();
                deadLettered++;
                continue;
            }

            try
            {
                await _dispatcher.DispatchAsync(message, ct);
                message.MarkAsProcessed();
                processed++;
            }
            catch (Exception ex)
            {
                message.MarkAsFailed(ex.Message);
                failed++;

                if (message.HasExceededRetries(command.MaxRetries))
                {
                    message.MarkAsDeadLettered();
                    deadLettered++;
                }
            }
        }

        await _repository.SaveChangesAsync(ct);

        return new ProcessOutboxOutput(processed, failed, deadLettered);
    }
}

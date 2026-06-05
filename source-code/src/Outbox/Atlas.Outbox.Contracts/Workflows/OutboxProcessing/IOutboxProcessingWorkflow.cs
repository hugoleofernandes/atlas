using Atlas.Outbox.Contracts.Commands.ProcessOutbox;

namespace Atlas.Outbox.Contracts.Workflows.OutboxProcessing;

public interface IOutboxProcessingWorkflow
{
    Task RunAsync(ProcessOutboxCommand command, CancellationToken ct);
}

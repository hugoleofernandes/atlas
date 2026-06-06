using Atlas.Outbox.Application.Commands.ProcessOutbox;

namespace Atlas.Outbox.Application.Workflows.OutboxProcessing;

public interface IOutboxProcessingWorkflow
{
    Task RunAsync(ProcessOutboxCommand command, CancellationToken ct);
}

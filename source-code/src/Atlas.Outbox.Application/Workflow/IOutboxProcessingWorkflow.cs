using Atlas.Outbox.Application.ProcessOutbox;

namespace Atlas.Outbox.Application.Workflow;

public interface IOutboxProcessingWorkflow
{
    Task RunAsync(ProcessOutboxCommand command, CancellationToken ct);
}

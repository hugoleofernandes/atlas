using Atlas.BuildingBlocks.Outbox.ListPendingMessages;
using Atlas.Outbox.Application.Targets;
using Atlas.Outbox.Domain.Targets;
using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.Outbox.Application.Commands.ProcessOutboxTargets;

public sealed class DirectTargetExecutor(IEnumerable<ITargetHandler> handlers) : IOutboxTargetExecutor
{
    public TargetMode Mode => TargetMode.Direct;

    public async Task<HandlerInvocationResult> ExecuteAsync(
        TargetMapping target,
        ListPendingMessagesDto message,
        CancellationToken ct
    )
    {
        var handler = handlers.FirstOrDefault(x => string.Equals(x.Name, target.Name, StringComparison.Ordinal));

        if (handler is null)
        {
            return HandlerInvocationResult.Failure(
                target.Name,
                $"No direct target handler registered for '{target.Name}'."
            );
        }

        return await handler.ExecuteAsync(message, ct);
    }
}

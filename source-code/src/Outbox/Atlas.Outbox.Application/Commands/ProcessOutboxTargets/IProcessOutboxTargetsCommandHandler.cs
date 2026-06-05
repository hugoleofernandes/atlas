using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.Outbox.Application.Commands.ProcessOutboxTargets;

public interface IProcessOutboxTargetsCommandHandler
    : ICommandHandler<ProcessOutboxTargetsCommand, IReadOnlyList<HandlerInvocationResult>>
{
}

using Atlas.Outbox.Application.Queries.ResolveOutboxTargets;
using Atlas.Outbox.Contracts;

namespace Atlas.Outbox.Application.Commands.ProcessOutboxTargets;

public sealed record ProcessOutboxTargetsCommand(
    OutboxMessageDto Message,
    IReadOnlyList<OutboxDispatchTargetDto> Targets
);

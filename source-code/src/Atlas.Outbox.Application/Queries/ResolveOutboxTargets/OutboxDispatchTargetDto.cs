namespace Atlas.Outbox.Application.Queries.ResolveOutboxTargets;

public sealed record OutboxDispatchTargetDto(
    string Name,
    OutboxTargetMode Mode);

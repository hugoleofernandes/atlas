using Atlas.Outbox.Contracts;

namespace Atlas.Outbox.Application.Queries.ResolveOutboxTargets;

public sealed record ResolveOutboxTargetsQuery(OutboxMessageDto Message);

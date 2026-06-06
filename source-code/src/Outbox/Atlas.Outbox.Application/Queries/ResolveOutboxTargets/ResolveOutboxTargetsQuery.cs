using Atlas.BuildingBlocks.Outbox.ListPendingMessages;

namespace Atlas.Outbox.Application.Queries.ResolveOutboxTargets;

public sealed record ResolveOutboxTargetsQuery(ListPendingMessagesDto Message);

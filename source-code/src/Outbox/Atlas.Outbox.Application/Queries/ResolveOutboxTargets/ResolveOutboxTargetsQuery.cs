using Atlas.Outbox.Contracts.Queries.ListPendingMessages;

namespace Atlas.Outbox.Application.Queries.ResolveOutboxTargets;

public sealed record ResolveOutboxTargetsQuery(ListPendingMessagesDto Message);

namespace Atlas.Outbox.Contracts.Queries.ListPendingMessages;

public sealed record ListPendingMessagesQuery(int BatchSize, TimeSpan LockDuration);

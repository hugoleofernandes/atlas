namespace Atlas.Outbox.Application.Queries.ListPendingMessages;

public sealed record ListPendingMessagesQuery(int BatchSize, TimeSpan LockDuration);

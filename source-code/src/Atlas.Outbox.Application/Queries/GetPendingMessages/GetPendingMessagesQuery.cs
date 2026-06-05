namespace Atlas.Outbox.Application.Queries.GetPendingMessages;

public sealed record GetPendingMessagesQuery(int BatchSize, TimeSpan LockDuration);

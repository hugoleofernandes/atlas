namespace Atlas.BuildingBlocks.Outbox.ListPendingMessages;

public sealed record ListPendingMessagesQuery(int BatchSize, TimeSpan LockDuration);

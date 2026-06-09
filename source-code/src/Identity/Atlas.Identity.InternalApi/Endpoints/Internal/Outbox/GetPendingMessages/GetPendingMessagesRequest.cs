namespace Atlas.Identity.InternalApi.Endpoints.Internal.Outbox.GetPendingMessages;

public sealed class GetPendingMessagesRequest
{
    public int BatchSize { get; init; } = 50;
    public int LockDurationSeconds { get; init; } = 30;
}

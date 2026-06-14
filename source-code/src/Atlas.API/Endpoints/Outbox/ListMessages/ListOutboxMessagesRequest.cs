namespace Atlas.API.Endpoints.Outbox.ListMessages;

public sealed class ListOutboxMessagesRequest
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

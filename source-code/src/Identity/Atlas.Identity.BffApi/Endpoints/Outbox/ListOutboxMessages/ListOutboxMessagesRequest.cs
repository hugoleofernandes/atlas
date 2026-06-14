namespace Atlas.Identity.BffApi.Endpoints.Outbox.ListOutboxMessages;

/// <summary>
/// Optional UTC period bounds — defaults to the last 24 hours when omitted.
/// Maximum window: 7 days.
/// </summary>
public sealed record ListOutboxMessagesRequest(DateTime? From, DateTime? To);

namespace Atlas.Outbox.Application.Queries.ListOutboxMessages;

/// <summary>
/// Period filter over occurred_on. Both bounds optional —
/// defaults to the last 24 hours when omitted.
/// </summary>
public sealed record ListOutboxMessagesQuery(DateTime? From, DateTime? To);

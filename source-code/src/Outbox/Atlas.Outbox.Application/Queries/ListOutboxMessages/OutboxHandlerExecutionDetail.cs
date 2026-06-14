namespace Atlas.Outbox.Application.Queries.ListOutboxMessages;

public sealed record OutboxHandlerExecutionDetail(
    string HandlerName,
    string Status,
    string? ErrorMessage,
    DateTime AttemptedAt
);

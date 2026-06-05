using Atlas.Outbox.Contracts.Queries.ListPendingMessages;
using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.Outbox.Application.Commands.UpdateOutboxMessageStatus;

public sealed record UpdateOutboxMessageStatusCommand(
    ListPendingMessagesDto Message,
    IReadOnlyList<HandlerInvocationResult> Results,
    int MaxRetries
);

using Atlas.Outbox.Contracts;
using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.Outbox.Application.Commands.UpdateOutboxMessageStatus;

public sealed record UpdateOutboxMessageStatusCommand(
    OutboxMessageDto Message,
    IReadOnlyList<HandlerInvocationResult> Results,
    int MaxRetries
);

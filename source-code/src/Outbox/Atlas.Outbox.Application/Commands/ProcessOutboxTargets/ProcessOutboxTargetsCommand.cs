using Atlas.Outbox.Contracts.Queries.ListPendingMessages;
using Atlas.Outbox.Contracts.Targets;

namespace Atlas.Outbox.Application.Commands.ProcessOutboxTargets;

public sealed record ProcessOutboxTargetsCommand(ListPendingMessagesDto Message, IReadOnlyList<TargetMapping> Targets);

using Atlas.BuildingBlocks.Outbox.ListPendingMessages;
using Atlas.Outbox.Domain.Targets;

namespace Atlas.Outbox.Application.Commands.ProcessOutboxTargets;

public sealed record ProcessOutboxTargetsCommand(ListPendingMessagesDto Message, IReadOnlyList<TargetMapping> Targets);

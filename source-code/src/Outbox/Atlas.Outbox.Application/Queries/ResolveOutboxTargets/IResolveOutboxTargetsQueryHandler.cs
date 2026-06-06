using Atlas.Outbox.Domain.Targets;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Outbox.Application.Queries.ResolveOutboxTargets;

public interface IResolveOutboxTargetsQueryHandler
    : IQueryHandler<ResolveOutboxTargetsQuery, IReadOnlyList<TargetMapping>>;

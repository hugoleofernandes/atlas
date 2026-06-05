using Atlas.Outbox.Application.DirectTargets;
using Atlas.Outbox.Application.Queries.ResolveOutboxTargets;
using Atlas.Outbox.Contracts;

namespace Atlas.Outbox.Infrastructure;

internal sealed class DirectOutboxTargetResolver(
    IIntegrationEventTypeResolver typeResolver,
    IDirectOutboxTargetCatalog catalog)
    : IOutboxTargetResolver
{
    public Task<IReadOnlyList<OutboxDispatchTargetDto>> ResolveAsync(
        OutboxMessageDto message,
        CancellationToken ct)
    {
        var eventType = typeResolver.Resolve(message.Type)
            ?? throw new InvalidOperationException(
                $"Integration event type '{message.Type}' not found.");

        var targets = catalog.GetFor(eventType)
            .OrderBy(target => target.Order)
            .ThenBy(target => target.Name, StringComparer.Ordinal)
            .Select(target => new OutboxDispatchTargetDto(
                target.Name,
                OutboxTargetMode.Direct))
            .ToList();

        if (targets.Count == 0)
            throw new InvalidOperationException(
                $"No direct target registered for '{eventType.Name}'.");

        return Task.FromResult<IReadOnlyList<OutboxDispatchTargetDto>>(targets);
    }
}

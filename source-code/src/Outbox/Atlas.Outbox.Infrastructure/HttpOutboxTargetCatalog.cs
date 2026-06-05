using Atlas.Outbox.Contracts.Targets;
using Atlas.Outbox.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Atlas.Outbox.Infrastructure;

internal sealed class HttpOutboxTargetCatalog(IOptions<OutboxWorkerOptions> options) : ITargetCatalog
{
    public IReadOnlyList<TargetMapping> GetFor(Type eventType)
    {
        var subscriptions = ResolveSubscriptions(eventType);

        if (subscriptions.Count == 0)
            return [];

        return subscriptions
            .Where(subscription => subscription.Enabled)
            .OrderBy(subscription => subscription.Order)
            .ThenBy(subscription => subscription.Name, StringComparer.Ordinal)
            .Select(subscription => new TargetMapping(
                subscription.Name,
                TargetMode.Http,
                subscription.Order,
                subscription.Url,
                subscription.Method
            ))
            .ToList();
    }

    private IReadOnlyList<OutboxSubscriptionOptions> ResolveSubscriptions(Type eventType)
    {
        var allSubscriptions = options.Value.Subscriptions;

        if (allSubscriptions.TryGetValue(eventType.Name, out var byName))
            return byName;

        if (eventType.FullName is not null && allSubscriptions.TryGetValue(eventType.FullName, out var byFullName))
            return byFullName;

        return [];
    }
}

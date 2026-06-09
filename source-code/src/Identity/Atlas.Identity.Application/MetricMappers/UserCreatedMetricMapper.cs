using Atlas.BuildingBlocks.Infrastructure.Observability;
using Atlas.Identity.Domain.Users.Events;
using Atlas.SharedKernel.Application.Metrics;
using Atlas.SharedKernel.Domain.Events;

namespace Atlas.Identity.Application.MetricMappers;

public sealed class UserCreatedMetricMapper : IMetricMapper
{
    public Type DomainEventType => typeof(UserCreatedFromInvitationDomainEvent);

    public void Execute(IDomainEvent domainEvent)
    {
        var evt = (UserCreatedFromInvitationDomainEvent)domainEvent;

        AtlasMetrics.UsersCreated.Add(1,
            new KeyValuePair<string, object?>("tenant.id", evt.TenantId.ToString()),
            new KeyValuePair<string, object?>("role", evt.Role));
    }
}

using Atlas.Identity.Domain.Entities.Tenants.Events;
using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.Identity.Application.OutboxMessages.Mappings;

public class TenantOutboxMappings : ITenantOutboxMappings
{
    public IReadOnlyList<OutboxEventDefinition> All { get; } =
    [
        new(
            Type: typeof(UserCreatedFromInvitationDomainEvent),
            Name: "tenant.user-created-from-invitation",
            Module: "identity"),
    ];
}
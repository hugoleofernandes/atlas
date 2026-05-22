using Atlas.Identity.Domain.Entities.Tenants.Events;
using Atlas.Integration.Contracts.Tenants;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.SharedKernel.Application.OutboxMessages;
using Atlas.SharedKernel.Domain.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.Publisher.Identity.Tenants.UserCreatedFromInvitation;

/// <summary>
/// Maps <see cref="UserCreatedFromInvitationDomainEvent"/> (Identity domain) to
/// <see cref="UserCreatedFromInvitationIntegrationEvent"/> (shared contract) and
/// wraps it in an outbox message for reliable delivery.
/// </summary>
internal sealed class UserCreatedFromInvitationMapper : IIntegrationEventMapper
{
    private readonly IOutboxMessageFactory _factory;

    public UserCreatedFromInvitationMapper(IOutboxMessageFactory factory)
    {
        _factory = factory;
    }

    public Type DomainEventType => typeof(UserCreatedFromInvitationDomainEvent);

    public OutboxMessage Map(IDomainEvent domainEvent)
    {
        var e = (UserCreatedFromInvitationDomainEvent)domainEvent;

        var integrationEvent = new UserCreatedFromInvitationIntegrationEvent(
            e.TenantId,
            e.UserId,
            e.Email,
            e.Role);

        return _factory.Create(integrationEvent);
    }
}

internal static class UserCreatedFromInvitationPublishDependencyInjection
{
    internal static IServiceCollection AddUserCreatedFromInvitationMapper(
        this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventMapper, UserCreatedFromInvitationMapper>();

        return services;
    }
}

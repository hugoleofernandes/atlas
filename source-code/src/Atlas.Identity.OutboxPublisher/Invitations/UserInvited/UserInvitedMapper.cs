using Atlas.Identity.Contracts.IntegrationEvents.Users;
using Atlas.Identity.Domain.Invitations.Events;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.SharedKernel.Application.OutboxMessages;
using Atlas.SharedKernel.Domain.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.OutboxPublisher.Invitations.UserInvited;

/// <summary>
/// Maps <see cref="UserInvitedDomainEvent"/> to <see cref="UserInvitedIntegrationEvent"/>
/// and wraps it in an outbox message for reliable delivery.
/// This event is raised in a normal authenticated request, so the ambient request context
/// already contains the actor identity used by <see cref="IOutboxMessageFactory.Create{T}(T)"/>.
/// </summary>
internal sealed class UserInvitedMapper : IIntegrationEventMapper
{
    private readonly IOutboxMessageFactory _factory;

    public UserInvitedMapper(IOutboxMessageFactory factory)
    {
        _factory = factory;
    }

    public Type DomainEventType => typeof(UserInvitedDomainEvent);

    public OutboxMessage Map(IDomainEvent domainEvent)
    {
        var e = (UserInvitedDomainEvent)domainEvent;
        var integrationEvent = new UserInvitedIntegrationEvent(e.TenantId, e.Email);
        return _factory.Create(integrationEvent);
    }
}

internal static class UserInvitedPublishDependencyInjection
{
    internal static IServiceCollection AddUserInvitedMapper(this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventMapper, UserInvitedMapper>();
        return services;
    }
}

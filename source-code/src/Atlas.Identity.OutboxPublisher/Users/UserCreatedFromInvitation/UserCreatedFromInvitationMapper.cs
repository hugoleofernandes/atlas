using Atlas.Identity.Domain.Users.Events;
using Atlas.Integration.Contracts.Tenants;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.SharedKernel.Application.OutboxMessages;
using Atlas.SharedKernel.Domain.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.OutboxPublisher.Users.UserCreatedFromInvitation;

/// <summary>
/// Maps <see cref="UserCreatedFromInvitationDomainEvent"/> (Identity domain) to
/// <see cref="UserCreatedFromInvitationIntegrationEvent"/> (shared contract) and
/// wraps it in an outbox message for reliable delivery.
///
/// <para>
/// This mapper deliberately uses the explicit-identity overload of
/// <see cref="IOutboxMessageFactory.Create{T}(T,Guid,Guid,string?)"/>.
/// The event is raised inside <c>ResolveTenantAccessCommandHandler</c>, which
/// is invoked by <c>UserBootstrapMiddleware</c> <em>before</em> the tenant/user
/// claims are written to the cookie — meaning <c>IRequestContext.TenantId</c> and
/// <c>IRequestContext.UserId</c> are still null at that point.
/// The domain event itself already carries the correct identity values, so we
/// pass them directly instead of relying on the ambient request context.
/// </para>
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

        var integrationEvent = new UserCreatedFromInvitationIntegrationEvent(e.TenantId, e.UserId, e.Email, e.Role);

        // Pass identity values explicitly — IRequestContext is not yet populated
        // when this mapper runs during the bootstrap flow.
        return _factory.Create(integrationEvent, e.TenantId, e.UserId, e.Email);
    }
}

internal static class UserCreatedFromInvitationPublishDependencyInjection
{
    internal static IServiceCollection AddUserCreatedFromInvitationMapper(this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventMapper, UserCreatedFromInvitationMapper>();

        return services;
    }
}

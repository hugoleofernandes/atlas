using Atlas.Integration.Contracts.Tenants;
using Atlas.SharedKernel.Application.Idempotency;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.Staff.Application.StaffMembers.Commands.CreateFromInvitation;

namespace Atlas.Outbox.Consumer.Identity.Tenants.UserCreatedFromInvitation.Staff;

/// <summary>
/// Adapter — Staff module.
/// Translates the integration contract into CreateStaffMemberFromInvitationCommand and delegates.
/// Implements IIdempotentHandler so the IntegrationIdempotencyDecorator deduplicates retries.
/// </summary>
internal sealed class CreateStaffMemberOnUserCreatedHandler
    : IIntegrationEventHandler<UserCreatedFromInvitationIntegrationEvent>,
      IIdempotentHandler
{
    private readonly ICreateStaffMemberFromInvitationCommandHandler _handler;

    public CreateStaffMemberOnUserCreatedHandler(
        ICreateStaffMemberFromInvitationCommandHandler handler)
    {
        _handler = handler;
    }

    public Task HandleAsync(
        UserCreatedFromInvitationIntegrationEvent @event,
        CancellationToken                         ct)
        => _handler.ExecuteAsync(
            new CreateStaffMemberFromInvitationCommand(
                @event.TenantId,
                @event.UserId,
                @event.Email,
                @event.Role),
            ct);
}

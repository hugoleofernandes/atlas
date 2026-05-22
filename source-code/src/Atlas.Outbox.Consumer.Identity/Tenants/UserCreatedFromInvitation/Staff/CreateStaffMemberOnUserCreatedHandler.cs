using Atlas.BuildingBlocks.Application.HandlerInvokers.Interfaces;
using Atlas.Integration.Contracts.Tenants;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.Staff.Application.StaffMembers.Commands.CreateFromInvitation;

namespace Atlas.Outbox.Consumer.Identity.Tenants.UserCreatedFromInvitation.Staff;

/// <summary>
/// Adapter — Staff module.
/// Translates UserCreatedFromInvitationIntegrationEvent → CreateStaffMemberFromInvitationCommand
/// and delegates through IHandlerInvoker so the command runs the full pipeline
/// (IdempotencyDecorator → ValidationDecorator → PersistDbDecorator → telemetry).
///
/// Idempotency is enforced at the command handler level
/// (CreateStaffMemberFromInvitationCommandHandler implements IIdempotentHandler).
/// No IIdempotentHandler here — the adapter is intentionally thin.
/// </summary>
internal sealed class CreateStaffMemberOnUserCreatedHandler
    : IIntegrationEventHandler<UserCreatedFromInvitationIntegrationEvent>
{
    private readonly ICreateStaffMemberFromInvitationCommandHandler _handler;
    private readonly IHandlerInvoker                                _invoker;

    public CreateStaffMemberOnUserCreatedHandler(
        ICreateStaffMemberFromInvitationCommandHandler handler,
        IHandlerInvoker                                invoker)
    {
        _handler = handler;
        _invoker  = invoker;
    }

    public async Task HandleAsync(
        UserCreatedFromInvitationIntegrationEvent @event,
        CancellationToken                         ct)
    {
        var result = await _invoker.InvokeAsync(
            _handler,
            new CreateStaffMemberFromInvitationCommand(
                @event.TenantId,
                @event.UserId,
                @event.Email,
                @event.Role),
            ct);

        if (!result.IsSuccess)
            throw new InvalidOperationException(
                result.ErrorDefinition?.FallbackMessage ?? "CreateStaffMember command failed.");
    }
}

using Atlas.Contracts.Tenants.IntegrationEvents;
using Atlas.SharedKernel.Application.Idempotency;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.Staff.Application.Abstractions;
using Atlas.Staff.Application.StaffMemberApp.Persistence;
using Atlas.Staff.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Atlas.Staff.Application.IntegrationEventHandlers;

/// <summary>
/// Creates a StaffMember when a user accepts an invitation.
///
/// Idempotency layers:
///   1. Technical (retry)  — IIdempotentHandler marker: IntegrationIdempotencyDecorator checks
///      the idempotency store before invoking this handler. If the (IdempotencyKey, HandlerName)
///      pair was already recorded the handler is skipped transparently — no code here needed.
///   2. Domain (business)  — ExistsAsync: guards against duplicate invitation events that carry
///      a different IdempotencyKey but the same business outcome (same TenantId + UserId).
///      This layer is intentional and must stay even if the technical layer fires.
/// </summary>
public sealed class CreateStaffMemberIntegrationEventHandler
    : IIntegrationEventHandler<UserCreatedFromInvitationIntegrationEvent>,
      IIdempotentHandler
{
    private readonly IStaffMemberRepository _repository;
    private readonly IStaffUnitOfWork _unitOfWork;
    private readonly ILogger<CreateStaffMemberIntegrationEventHandler> _logger;

    public CreateStaffMemberIntegrationEventHandler(
        IStaffMemberRepository repository,
        IStaffUnitOfWork       unitOfWork,
        ILogger<CreateStaffMemberIntegrationEventHandler> logger)
    {
        _repository  = repository;
        _unitOfWork  = unitOfWork;
        _logger      = logger;
    }

    public async Task HandleAsync(UserCreatedFromInvitationIntegrationEvent @event, CancellationToken ct)
    {
        _logger.LogInformation(
            "CreateStaffMember — TenantId={TenantId} UserId={UserId} Email={Email} Role={Role}",
            @event.TenantId, @event.UserId, @event.Email, @event.Role);

        // Domain idempotency: enforce the business invariant that a user can only
        // have one StaffMember per tenant — guards against duplicate invitation messages
        // (different IdempotencyKey, same business outcome) which the technical idempotency
        // layer would not catch because the key is different.
        if (await _repository.ExistsAsync(@event.TenantId, @event.UserId, ct))
        {
            _logger.LogInformation(
                "CreateStaffMember skipped — StaffMember already exists (UserId={UserId})",
                @event.UserId);
            return;
        }

        // Placeholder name derived from email until the user fills their profile.
        // e.g. "hugo.silva@company.com" → firstName="hugo.silva", lastName=""
        var emailLocalPart = @event.Email.Split('@')[0];
        var firstName      = emailLocalPart.Length <= 100 ? emailLocalPart : emailLocalPart[..100];

        var staff = new StaffMember(
            tenantId:  @event.TenantId,
            UserId:    @event.UserId,
            firstName: firstName,
            lastName:  string.Empty,
            role:      @event.Role);

        await _repository.AddAsync(staff, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "StaffMember created — Id={StaffId} UserId={UserId}",
            staff.Id, @event.UserId);
    }
}

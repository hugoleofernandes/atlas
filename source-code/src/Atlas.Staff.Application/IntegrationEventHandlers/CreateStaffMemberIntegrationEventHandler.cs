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
/// Idempotency is guaranteed by IIdempotencyService (INSERT ON CONFLICT DO NOTHING)
/// — safe to retry if the outbox message is re-processed after a transient failure.
/// </summary>
public sealed class CreateStaffMemberIntegrationEventHandler
    : IIntegrationEventHandler<UserCreatedFromInvitationIntegrationEvent>
{
    private readonly IIdempotencyService _idempotency;
    private readonly IStaffMemberRepository _repository;
    private readonly IStaffUnitOfWork _unitOfWork;
    private readonly ILogger<CreateStaffMemberIntegrationEventHandler> _logger;

    public CreateStaffMemberIntegrationEventHandler(
        IIdempotencyService idempotency,
        IStaffMemberRepository repository,
        IStaffUnitOfWork unitOfWork,
        ILogger<CreateStaffMemberIntegrationEventHandler> logger)
    {
        _idempotency = idempotency;
        _repository  = repository;
        _unitOfWork  = unitOfWork;
        _logger      = logger;
    }

    public async Task HandleAsync(UserCreatedFromInvitationIntegrationEvent @event, CancellationToken ct)
    {
        _logger.LogInformation(
            "CreateStaffMember — TenantId={TenantId} UserId={UserId} Email={Email} Role={Role}",
            @event.TenantId, @event.UserId, @event.Email, @event.Role);

        // Technical idempotency: skip if this exact message was already processed
        // (e.g. worker retry after a transient failure).
        if (await _idempotency.HasAlreadyProcessedAsync(ct))
        {
            _logger.LogInformation(
                "CreateStaffMember skipped — already processed (UserId={UserId})",
                @event.UserId);
            return;
        }

        // Domain idempotency: enforce the business invariant that a user can only
        // have one StaffMember per tenant — guards against duplicate invitation messages
        // (different OutboxMessage.Id, same business outcome) which the idempotency
        // service would not catch because the message key is different.
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

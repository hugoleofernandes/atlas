using Atlas.Contracts.Tenants.IntegrationEvents;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.Staff.Application.Abstractions;
using Atlas.Staff.Application.StaffMemberApp.Persistence;
using Atlas.Staff.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Atlas.Staff.Application.IntegrationEventHandlers;

/// <summary>
/// Creates a StaffMember when a user accepts an invitation.
/// Idempotent: if a StaffMember for this UserId already exists it exits early —
/// safe to retry if the outbox message is re-processed after a transient failure.
/// </summary>
public sealed class CreateStaffMemberIntegrationEventHandler
    : IIntegrationEventHandler<UserCreatedFromInvitationIntegrationEvent>
{
    private readonly IStaffMemberRepository _repository;
    private readonly IStaffUnitOfWork _unitOfWork;
    private readonly ILogger<CreateStaffMemberIntegrationEventHandler> _logger;

    public CreateStaffMemberIntegrationEventHandler(
        IStaffMemberRepository repository,
        IStaffUnitOfWork unitOfWork,
        ILogger<CreateStaffMemberIntegrationEventHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task HandleAsync(UserCreatedFromInvitationIntegrationEvent @event, CancellationToken ct)
    {
        // 👇 breakpoint aqui para ver o evento a chegar no módulo Staff
        _logger.LogInformation(
            "Creating StaffMember — TenantId={TenantId} UserId={UserId} Email={Email} Role={Role}",
            @event.TenantId, @event.UserId, @event.Email, @event.Role);

        // Idempotency: skip if already created (e.g. message retried after transient failure)
        var exists = await _repository.ExistsAsync(@event.TenantId, @event.UserId, ct);
        if (exists)
        {
            _logger.LogInformation(
                "StaffMember already exists for UserId={UserId} — skipping (idempotent)",
                @event.UserId);
            return;
        }

        // Derive a placeholder name from the email local-part until the user sets their profile.
        // e.g. "hugo.silva@company.com" → firstName="hugo.silva", lastName=""
        var emailLocalPart = @event.Email.Split('@')[0];
        var firstName = emailLocalPart.Length <= 100 ? emailLocalPart : emailLocalPart[..100];

        var staff = new StaffMember(
            tenantId: @event.TenantId,
            UserId: @event.UserId,
            firstName: firstName,
            lastName: string.Empty,
            role: @event.Role);

        await _repository.AddAsync(staff, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "StaffMember created — Id={StaffId} UserId={UserId}",
            staff.Id, @event.UserId);
    }
}

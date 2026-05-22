using Atlas.SharedKernel.Application;
using Atlas.Staff.Application.Abstractions;
using Atlas.Staff.Application.StaffMemberApp.Persistence;
using Atlas.Staff.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Atlas.Staff.Application.StaffMembers.Commands.CreateFromInvitation;

/// <summary>
/// Creates a StaffMember when a user accepts an invitation.
///
/// Pure application logic — has no knowledge of how this command was triggered.
/// The trigger (OutboxWorker, test harness) is the adapter's concern.
///
/// Idempotency (domain layer):
///   ExistsAsync guards against duplicate invitation events that carry a different
///   IdempotencyKey but produce the same business outcome (same TenantId + UserId).
///   Technical idempotency (retry deduplication) is handled by the adapter implementing
///   IIdempotentHandler, which the IntegrationIdempotencyDecorator intercepts before
///   this handler is ever called.
/// </summary>
public sealed class CreateStaffMemberFromInvitationCommandHandler
    : ICreateStaffMemberFromInvitationCommandHandler
{
    private readonly IStaffMemberRepository                          _repository;
    private readonly IStaffUnitOfWork                               _unitOfWork;
    private readonly ILogger<CreateStaffMemberFromInvitationCommandHandler> _logger;

    public CreateStaffMemberFromInvitationCommandHandler(
        IStaffMemberRepository                          repository,
        IStaffUnitOfWork                               unitOfWork,
        ILogger<CreateStaffMemberFromInvitationCommandHandler> logger)
    {
        _repository  = repository;
        _unitOfWork  = unitOfWork;
        _logger      = logger;
    }

    public async Task<Unit> ExecuteAsync(
        CreateStaffMemberFromInvitationCommand command,
        CancellationToken                      ct)
    {
        _logger.LogInformation(
            "CreateStaffMember — TenantId={TenantId} UserId={UserId} Role={Role}",
            command.TenantId, command.UserId, command.Role);

        // Domain idempotency: same business outcome may arrive via different messages.
        if (await _repository.ExistsAsync(command.TenantId, command.UserId, ct))
        {
            _logger.LogInformation(
                "CreateStaffMember skipped — StaffMember already exists (UserId={UserId})",
                command.UserId);

            return Unit.Value;
        }

        // Derive placeholder name from email until the user fills their profile.
        // e.g. "hugo.silva@company.com" → firstName="hugo.silva", lastName=""
        var emailLocalPart = command.Email.Split('@')[0];
        var firstName      = emailLocalPart.Length <= 100
            ? emailLocalPart
            : emailLocalPart[..100];

        var staff = new StaffMember(
            tenantId:  command.TenantId,
            UserId:    command.UserId,
            firstName: firstName,
            lastName:  string.Empty,
            role:      command.Role);

        await _repository.AddAsync(staff, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "StaffMember created — Id={StaffId} UserId={UserId}",
            staff.Id, command.UserId);

        return Unit.Value;
    }
}

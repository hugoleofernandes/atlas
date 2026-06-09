using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Application.Idempotency;
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
/// Implements <see cref="IIdempotentHandler"/> so the <c>IdempotencyDecorator</c>
/// in the command pipeline deduplicates retries using the (IdempotencyKey, HandlerName)
/// pair set by OutboxMessageDispatcher.
///
/// Domain idempotency:
///   ExistsAsync guards against duplicate events that carry a different IdempotencyKey
///   but produce the same business outcome (same TenantId + UserId).
///   Both guards complement each other — technical and domain-level safety.
/// </summary>
public sealed class CreateStaffMemberFromInvitationCommandHandler
    : ICreateStaffMemberFromInvitationCommandHandler,
        IIdempotentHandler
{
    private readonly IStaffMemberRepository _repository;
    private readonly IStaffUnitOfWork _unitOfWork;
    private readonly ILogger<CreateStaffMemberFromInvitationCommandHandler> _logger;

    /// <inheritdoc/>
    /// Exposed so PersistDbDecorator can call SaveChangesAsync after execution.
    public IUnitOfWork UnitOfWork => _unitOfWork;

    public CreateStaffMemberFromInvitationCommandHandler(
        IStaffMemberRepository repository,
        IStaffUnitOfWork unitOfWork,
        ILogger<CreateStaffMemberFromInvitationCommandHandler> logger
    )
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> ExecuteAsync(CreateStaffMemberFromInvitationCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
            "CreateStaffMember — TenantId={TenantId} UserId={UserId} Role={Role}",
            command.TenantId,
            command.UserId,
            command.Role
        );

        // Domain idempotency: same business outcome may arrive via messages with different
        // IdempotencyKeys (e.g. two separate invitations for the same user).
        // Technical idempotency (retry deduplication) is handled by IdempotencyDecorator.
        if (await _repository.ExistsAsync(command.TenantId, command.UserId, ct))
        {
            _logger.LogInformation(
                "CreateStaffMember skipped — StaffMember already exists (UserId={UserId})",
                command.UserId
            );

            return Unit.Value;
        }

        // Derive placeholder name from email until the user fills their profile.
        // e.g. "hugo.silva@company.com" → firstName="hugo.silva", lastName=""
        var emailLocalPart = command.Email.Split('@')[0];
        var firstName = emailLocalPart.Length <= 100 ? emailLocalPart : emailLocalPart[..100];

        var staff = new StaffMember(
            tenantId: command.TenantId,
            userId: command.UserId,
            firstName: firstName,
            lastName: string.Empty,
            role: command.Role
        );

        await _repository.AddAsync(staff, ct);
        // SaveChangesAsync is called by PersistDbDecorator — do NOT call it here.

        _logger.LogInformation("StaffMember created — Id={StaffId} UserId={UserId}", staff.Id, command.UserId);

        return Unit.Value;
    }
}

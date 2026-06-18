using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.Staff.Application.Abstractions;
using Atlas.Staff.Application.StaffMemberApp.Persistence;
using Atlas.Staff.Domain.Entities.Exceptions;
using Atlas.Staff.Domain.Shared.Exceptions;

namespace Atlas.Staff.Application.StaffMembers.Commands.Terminate;

/// <summary>Terminates the employment of a staff member and records the termination date.</summary>
public sealed class TerminateStaffMemberCommandHandler(
    IRequestContext requestContext,
    IStaffMemberRepository repository,
    IStaffUnitOfWork unitOfWork
) : ITerminateStaffMemberCommandHandler
{
    public IUnitOfWork UnitOfWork => unitOfWork;

    public async Task<Unit> ExecuteAsync(TerminateStaffMemberCommand command, CancellationToken ct)
    {
        var tenantId = requestContext.TenantId ?? throw new TenantContextNotResolvedException();

        var staffMember =
            await repository.GetByIdAsync(command.StaffMemberId, ct)
            ?? throw new StaffMemberNotFoundException(command.StaffMemberId);

        if (staffMember.TenantId != tenantId)
            throw new StaffMemberNotFoundException(command.StaffMemberId);

        staffMember.Terminate(command.TerminationDate);

        return Unit.Value;
    }
}

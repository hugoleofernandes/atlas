using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.Staff.Application.Abstractions;
using Atlas.Staff.Application.StaffMemberApp.Persistence;
using Atlas.Staff.Domain.Entities.Exceptions;
using Atlas.Staff.Domain.Shared.Exceptions;

namespace Atlas.Staff.Application.StaffMembers.Commands.Update;

/// <summary>Updates employment contract data for an existing staff member.</summary>
public sealed class UpdateStaffMemberCommandHandler(
    IRequestContext requestContext,
    IStaffMemberRepository repository,
    IStaffUnitOfWork unitOfWork
) : IUpdateStaffMemberCommandHandler
{
    public IUnitOfWork UnitOfWork => unitOfWork;

    public async Task<UpdateStaffMemberOutput> ExecuteAsync(
        UpdateStaffMemberCommand command,
        CancellationToken ct)
    {
        var tenantId = requestContext.TenantId ?? throw new TenantContextNotResolvedException();

        var staffMember =
            await repository.GetByIdAsync(command.StaffMemberId, ct)
            ?? throw new StaffMemberNotFoundException(command.StaffMemberId);

        if (staffMember.TenantId != tenantId)
            throw new StaffMemberNotFoundException(command.StaffMemberId);

        staffMember.Update(command.ContractType, command.HireDate);

        return new UpdateStaffMemberOutput(staffMember.Id);
    }
}

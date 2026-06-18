using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.Staff.Application.Abstractions;
using Atlas.Staff.Application.StaffMemberApp.Persistence;
using Atlas.Staff.Domain.Entities;
using Atlas.Staff.Domain.Entities.Exceptions;
using Atlas.Staff.Domain.Shared.Exceptions;

namespace Atlas.Staff.Application.StaffMembers.Commands.Register;

/// <summary>
/// Registers a new staff member linked to a Party (Person).
/// Enforces uniqueness of PartyId per tenant — one active employment record per person.
/// </summary>
public sealed class RegisterStaffMemberCommandHandler(
    IRequestContext requestContext,
    IStaffMemberRepository repository,
    IStaffUnitOfWork unitOfWork
) : IRegisterStaffMemberCommandHandler
{
    public IUnitOfWork UnitOfWork => unitOfWork;

    public async Task<RegisterStaffMemberOutput> ExecuteAsync(
        RegisterStaffMemberCommand command,
        CancellationToken ct)
    {
        var tenantId = requestContext.TenantId ?? throw new TenantContextNotResolvedException();

        if (await repository.ExistsForPartyAsync(tenantId, command.PartyId, ct))
            throw new StaffMemberAlreadyExistsException(command.PartyId);

        var staffMember = StaffMember.Register(
            tenantId: tenantId,
            partyId: command.PartyId,
            employeeNumber: command.EmployeeNumber,
            contractType: command.ContractType,
            hireDate: command.HireDate
        );

        await repository.AddAsync(staffMember, ct);

        return new RegisterStaffMemberOutput(staffMember.Id);
    }
}

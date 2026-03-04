using Atlas.BuildingBlocks.CQRS.Abstractions;
using Atlas.SharedKernel.Application;
using Atlas.Staff.Application.Errors;
using Atlas.Staff.Application.StaffMemberApp.Persistence;
using Atlas.Staff.Domain.Entities;

namespace Atlas.Staff.Application.StaffMembers.Commands.Create;

public sealed class Handler
    : ICommandHandler<Command, Result<ResultDto>>
{
    private readonly IStaffMemberRepository _repository;

    public Handler(IStaffMemberRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ResultDto>> Handle(
        Command command,
        CancellationToken ct)
    {
        var exists = await _repository.ExistsAsync(
            command.TenantId,
            command.IdentityUserId,
            ct);

        if (exists)
        {
            return Result<ResultDto>.Failure(
                StaffErrors.AlreadyExists,
                "Staff already exists for this user."
            );
        }

        var staff = new StaffMember(
            command.TenantId,
            command.IdentityUserId,
            command.FirstName,
            command.LastName,
            command.Role
        );

        await _repository.AddAsync(staff, ct);

        // SaveChanges será feito pelo TransactionBehavior

        return Result<ResultDto>.Ok(
            new ResultDto(staff.Id)
        );
    }
}
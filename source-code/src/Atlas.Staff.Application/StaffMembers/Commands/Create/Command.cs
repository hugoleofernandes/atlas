using Atlas.BuildingBlocks.CQRS.Abstractions;
using Atlas.SharedKernel.Application;

namespace Atlas.Staff.Application.StaffMembers.Commands.Create;

public sealed record Command(
    Guid TenantId,
    Guid IdentityUserId,
    string FirstName,
    string LastName,
    string Role
) : ICommand<Result<ResultDto>>;
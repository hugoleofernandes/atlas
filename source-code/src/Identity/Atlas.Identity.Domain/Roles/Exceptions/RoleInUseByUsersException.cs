using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Roles.Exceptions;

public sealed class RoleInUseByUsersException : DomainException
{
    public const string ErrorCode = "role.in_use_by_users";

    public RoleInUseByUsersException(string roleName)
        : base(
            ErrorCode,
            ErrorCategory.Conflict,
            $"Role '{roleName}' cannot be removed because it is assigned to active users. Reassign them first."
        ) { }
}

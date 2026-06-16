using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Roles.Exceptions;

public sealed class RoleInUseByInvitationsException : DomainException
{
    public const string ErrorCode = "role.in_use_by_invitations";

    public RoleInUseByInvitationsException(string roleName)
        : base(
            ErrorCode,
            ErrorCategory.Conflict,
            $"Role '{roleName}' cannot be removed because it has pending invitations. Cancel or reassign them first."
        ) { }
}

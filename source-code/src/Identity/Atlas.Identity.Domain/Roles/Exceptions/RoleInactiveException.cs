using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Roles.Exceptions;

public sealed class RoleInactiveException : DomainException
{
    public const string ErrorCode = "role.inactive";

    public RoleInactiveException(string roleName)
        : base(ErrorCode, ErrorCategory.Unauthorized, $"Role '{roleName}' is inactive and cannot grant access.") { }
}

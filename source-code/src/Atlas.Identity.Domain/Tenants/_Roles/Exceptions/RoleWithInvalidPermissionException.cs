using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Tenants._Roles.Exceptions;

public sealed class RoleWithInvalidPermissionException : DomainException
{
    public const string ErrorCode = "role.permission_invalid";

    public RoleWithInvalidPermissionException(IEnumerable<string> unknownCodes)
        : base(ErrorCode, ErrorCategory.Validation,
            $"Unknown permission codes: {string.Join(", ", unknownCodes)}.")
    {
    }
}

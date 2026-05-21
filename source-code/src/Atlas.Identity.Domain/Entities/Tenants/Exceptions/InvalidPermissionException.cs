using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants.Exceptions;

public sealed class InvalidPermissionException : DomainException
{
    public const string ErrorCode = "role.permission_invalid";

    public InvalidPermissionException(IEnumerable<string> unknownCodes)
        : base(ErrorCode, ErrorCategory.Validation,
            $"Unknown permission codes: {string.Join(", ", unknownCodes)}.")
    {
    }
}

using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Roles.Exceptions;

public sealed class InvalidRoleNameException : DomainException
{
    public const string ErrorCode = "role.invalid_name";

    public InvalidRoleNameException()
        : base(ErrorCode, ErrorCategory.Validation, "Role name must be between 3 and 10 characters.") { }
}

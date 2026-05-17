using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.ValueObjects.Exceptions;

public sealed class InvalidRoleException : DomainException
{
    public const string ErrorCode = "role.invalid";

    public InvalidRoleException(string role)
        : base(ErrorCode, ErrorCategory.Validation, $"Invalid role: '{role}'.") { }
}

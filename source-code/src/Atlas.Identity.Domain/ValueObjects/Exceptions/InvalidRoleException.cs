using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.ValueObjects.Exceptions;

/// <summary>
/// Thrown when a role violates the role invariant.
/// </summary>
public sealed class InvalidRoleException : DomainException
{
    public InvalidRoleException(string role)
        : base("role.invalid", ErrorCategory.Validation, $"Invalid role: '{role}'.") { }
}

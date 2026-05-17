using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.ValueObjects.Exceptions;

/// <summary>
/// Thrown when an email violates the email invariant.
/// </summary>
public sealed class InvalidEmailException : DomainException
{
    public InvalidEmailException(string email)
        : base("email.invalid", ErrorCategory.Validation, $"Invalid email format: '{email}'.") { }
}

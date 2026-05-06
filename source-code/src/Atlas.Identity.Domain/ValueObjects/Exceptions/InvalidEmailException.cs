using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.ValueObjects.Exceptions;

/// <summary>
/// Thrown when an email violates the email invariant.
/// </summary>
public sealed class InvalidEmailException : DomainException
{
    public InvalidEmailException(string email)
        : base($"Invalid email format: '{email}'.") { }
}

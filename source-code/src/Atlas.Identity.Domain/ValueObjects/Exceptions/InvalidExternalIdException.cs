using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.ValueObjects.Exceptions;

/// <summary>
/// Thrown when an external identity provider user identifier violates the invariant.
/// </summary>
public sealed class InvalidExternalIdException : DomainException
{
    public InvalidExternalIdException(string value)
        : base($"Invalid external identity provider user identifier: '{value}'.") { }
}

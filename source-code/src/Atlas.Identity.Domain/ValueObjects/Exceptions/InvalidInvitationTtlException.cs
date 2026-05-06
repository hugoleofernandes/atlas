using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.ValueObjects.Exceptions;

/// <summary>
/// Thrown when an invitation TTL violates the TTL invariant.
/// </summary>
public sealed class InvalidInvitationTtlException : DomainException
{
    public InvalidInvitationTtlException(TimeSpan ttl)
        : base($"Invalid invitation TTL: '{ttl}'.") { }
}

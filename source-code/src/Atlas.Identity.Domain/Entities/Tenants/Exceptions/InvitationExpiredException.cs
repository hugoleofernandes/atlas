using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants.Exceptions;

/// <summary>
/// Thrown when attempting to use an invitation that is no longer active.
///
/// Invariant violated:
/// - A user must be created only from a valid and active invitation.
///
/// When thrown:
/// - During access resolution if the invitation has expired or is inactive.
///
/// Aggregate:
/// - Tenant
/// </summary>
public sealed class InvitationExpiredException : DomainException
{
    public InvitationExpiredException(string email)
        : base($"The invitation for '{email}' has expired.")
    {
    }
}

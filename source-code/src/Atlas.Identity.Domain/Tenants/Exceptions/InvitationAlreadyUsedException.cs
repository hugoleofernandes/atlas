using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Tenants.Exceptions;

/// <summary>
/// Thrown when attempting to use an invitation that has already been used.
///
/// Invariant violated:
/// - An invitation can only be used once.
///
/// Aggregate:
/// - Tenant
/// </summary>
public sealed class InvitationAlreadyUsedException : DomainException
{
    public InvitationAlreadyUsedException(string email)
        : base($"The invitation for '{email}' has already been used.")
    {
    }
}

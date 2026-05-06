using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Tenants.Exceptions;

/// <summary>
/// Thrown when attempting to create a tenant without a valid name.
///
/// Invariant violated:
/// - A tenant must always have a valid and normalized name.
///
/// Aggregate:
/// - Tenant
/// </summary>
public sealed class TenantNameRequiredException : DomainException
{
    public TenantNameRequiredException()
        : base("Tenant name is required.")
    {
    }
}

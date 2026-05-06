using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Tenants.Exceptions;

public sealed class TenantInactiveException : DomainException
{
    public TenantInactiveException()
        : base("Tenant is inactive.") { }
}
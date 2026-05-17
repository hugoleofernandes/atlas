using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants.Exceptions;

public sealed class TenantInactiveException : DomainException
{
    public TenantInactiveException()
        : base("tenant.inactive", ErrorCategory.Business, "Tenant is inactive.") { }
}

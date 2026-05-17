using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants.Exceptions;

public sealed class TenantNameRequiredException : DomainException
{
    public TenantNameRequiredException()
        : base("tenant.name_required", ErrorCategory.Validation, "Tenant name is required.") { }
}

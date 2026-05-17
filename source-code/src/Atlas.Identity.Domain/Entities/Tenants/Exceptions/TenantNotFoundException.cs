using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Exceptions;

public sealed class TenantNotFoundException : DomainException
{
    public TenantNotFoundException(string tenantName)
        : base("tenant.not_found", ErrorCategory.NotFound, $"Tenant '{tenantName}' was not found.") { }
}

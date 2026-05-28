using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Tenants.Exceptions;

public sealed class TenantNotFoundException : DomainException
{
    public const string ErrorCode = "tenant.not_found";

    public TenantNotFoundException(string tenantName)
        : base(ErrorCode, ErrorCategory.NotFound, $"Tenant '{tenantName}' was not found.") { }
}

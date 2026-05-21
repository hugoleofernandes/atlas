using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants.Exceptions;

public sealed class TenantInactiveException : DomainException
{
    public const string ErrorCode = "tenant.inactive";

    public TenantInactiveException()
        : base(ErrorCode, ErrorCategory.Business, "Tenant is inactive.") { }
}

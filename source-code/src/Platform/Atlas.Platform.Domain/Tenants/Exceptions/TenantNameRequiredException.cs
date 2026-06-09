using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Platform.Domain.Tenants.Exceptions;

public sealed class TenantNameRequiredException : DomainException
{
    public const string ErrorCode = "tenant.name_required";

    public TenantNameRequiredException()
        : base(ErrorCode, ErrorCategory.Validation, "Tenant name is required.") { }
}

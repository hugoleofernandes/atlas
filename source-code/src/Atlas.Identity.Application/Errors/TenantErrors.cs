using Atlas.SharedKernel.Application.Errors;

namespace Atlas.Identity.Application.Errors;

public static class TenantErrors
{
    public static readonly ErrorDefinition NotFound =
        new(
            Code: "TENANT_001",
            DefaultMessage: "Tenant not found",
            Category: ErrorCategory.Conflict
        );
}
using Atlas.SharedKernel.Application.Errors;

namespace Atlas.Identity.Application.Tenants.Errors;

public static class TenantErrors
{
    public static readonly ErrorDefinition NotFound =
        new(
            Code: "TENANT_001",
            DefaultMessage: "Tenant not found",
            Category: ErrorCategory.Conflict
        );

    public static readonly ErrorDefinition ResolveAccess =
    new(
        Code: "TENANT_002",
        DefaultMessage: "Failed to resolve access for tenant",
        Category: ErrorCategory.Conflict
    );
}
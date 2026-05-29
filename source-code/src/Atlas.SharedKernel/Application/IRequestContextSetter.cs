namespace Atlas.SharedKernel.Application;

public interface IRequestContextSetter
{
    void Set(Guid tenantId, string name, Guid userId, string? userEmail);
    void SetCorrelationId(string correlationId);

    /// <summary>
    /// Suspends the global multi-tenant query filter for the duration of the returned scope.
    /// Use only when querying multi-tenant entities without an established tenant context —
    /// e.g. the bootstrap flow that runs before TenantId is known.
    /// The suspension is automatically lifted when the returned IDisposable is disposed.
    /// </summary>
    IDisposable SuspendTenantFilter();
}

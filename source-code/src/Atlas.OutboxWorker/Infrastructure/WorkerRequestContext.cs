using Atlas.SharedKernel.Application;

namespace Atlas.OutboxWorker.Infrastructure;

// Satisfaz IRequestContext exigido pelo MultiTenantDbContext no contexto sem HTTP request.
// O worker não tem tenant de request — query filters de multi-tenancy não se aplicam ao OutboxMessage.
internal sealed class WorkerRequestContext : IRequestContext
{
    public bool IsAuthenticated => false;
    public Guid? TenantId => null;
    public string? TenantName => null;
    public Guid? UserId => null;
    public string? UserEmail => null;
}

using Atlas.SharedKernel.Application;

namespace Atlas.Outbox.Infrastructure.Configuration;

/// <summary>
/// Mutable request context for the OutboxWorker.
/// Populated from each OutboxMessage before dispatch so that
/// SavePipeline (audit, stamper) and handlers run with the
/// correct tenant/user/correlation context.
/// </summary>
public sealed class WorkerRequestContext : IRequestContext, IRequestContextSetter
{
    private Guid? _tenantId;
    private string? _tenantName;
    private Guid? _userId;
    private string? _correlationId;

    public bool IsAuthenticated  => _tenantId.HasValue;
    public Guid? TenantId        => _tenantId;
    public string? TenantName    => _tenantName;
    public Guid? UserId          => _userId;
    public string? UserEmail     => null;   // not stored in OutboxMessage
    public string? CorrelationId => _correlationId;

    public void Set(Guid tenantId, string name, Guid userId, string? userEmail)
    {
        _tenantId   = tenantId;
        _tenantName = name;
        _userId     = userId;
    }

    public void SetCorrelationId(string correlationId)
        => _correlationId = correlationId;
}

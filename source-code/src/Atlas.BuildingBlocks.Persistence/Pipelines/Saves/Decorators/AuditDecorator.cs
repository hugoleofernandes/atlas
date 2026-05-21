using Atlas.BuildingBlocks.Persistence.DbContexts;
using Atlas.BuildingBlocks.Persistence.Entities.Audits.Interfaces;
using Atlas.BuildingBlocks.Persistence.Pipelines.Saves.Interfaces;

namespace Atlas.BuildingBlocks.Persistence.Pipelines.Saves.Decorators;

/// <summary>
/// Records the audit trail before delegating downstream.
/// </summary>
internal sealed class AuditDecorator : ISavePipelineStep
{
    private readonly ISavePipelineStep _inner;
    private readonly IAuditTrailService _auditTrailService;

    public AuditDecorator(ISavePipelineStep inner, IAuditTrailService auditTrailService)
    {
        _inner             = inner;
        _auditTrailService = auditTrailService;
    }

    public async Task ExecuteAsync(DbContextBase db, CancellationToken ct)
    {
        await _auditTrailService.RecordAsync(db, ct);
        await _inner.ExecuteAsync(db, ct);
    }
}

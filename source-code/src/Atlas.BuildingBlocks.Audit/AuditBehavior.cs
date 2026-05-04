using MediatR;
using Atlas.BuildingBlocks.CQRS.Abstractions;

namespace Atlas.BuildingBlocks.Audit;

public sealed class AuditBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IAuditStore _auditStore;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentTenant _currentTenant;

    public AuditBehavior(
        IAuditStore auditStore,
        ICurrentUser currentUser,
        ICurrentTenant currentTenant)
    {
        _auditStore = auditStore;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var response = await next();

        if (request is ICommand<TResponse>)
        {
            var audit = new AuditEntry(
                action: request.GetType().Name,
                entityName: request.GetType().Name,
                entityId: null,
                userId: _currentUser.UserId,
                tenantId: _currentTenant.TenantId,
                changes: System.Text.Json.JsonSerializer.Serialize(request)
            );

            await _auditStore.AddAsync(audit, ct);
        }

        return response;
    }
}
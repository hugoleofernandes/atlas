using Atlas.SharedKernel.Domain;

namespace Atlas.SharedKernel.Application;

public interface IAuditService
{
    Task AddAuditLogsAsync<TAudit>(IUnitOfWork uow, CancellationToken ct)
         where TAudit : class, IAuditLog, new();
}

using Atlas.SharedKernel.Application.Commands;

namespace Atlas.Identity.Application.Tenants.Commands.ResolveTenantAccess;

public interface ICommandHandler
{
    Task<Result<Output>> ExecuteAsync(Command cmd, CancellationToken ct);
}

namespace Atlas.Identity.Application.Tenants.Commands.ResolveTenantAccess;

public interface ICommandHandler
{
    Task<Output> ExecuteAsync(Command cmd, CancellationToken ct);
}

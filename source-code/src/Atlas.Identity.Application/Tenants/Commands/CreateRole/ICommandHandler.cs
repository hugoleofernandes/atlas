namespace Atlas.Identity.Application.Tenants.Commands.CreateRole;

public interface ICommandHandler
{
    Task<Output> ExecuteAsync(Command cmd, CancellationToken ct);
}

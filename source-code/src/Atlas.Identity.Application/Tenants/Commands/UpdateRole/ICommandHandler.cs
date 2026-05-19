namespace Atlas.Identity.Application.Tenants.Commands.UpdateRole;

public interface ICommandHandler
{
    Task<Output> ExecuteAsync(Command cmd, CancellationToken ct);
}

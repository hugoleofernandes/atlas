namespace Atlas.Identity.Application.Tenants.Commands.RemoveRole;

public interface IRemoveRoleCommandHandler
{
    Task<RemoveRoleOutput> ExecuteAsync(RemoveRoleCommand cmd, CancellationToken ct);
}

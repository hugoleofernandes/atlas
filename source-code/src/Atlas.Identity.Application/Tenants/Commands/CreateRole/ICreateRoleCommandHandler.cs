namespace Atlas.Identity.Application.Tenants.Commands.CreateRole;

public interface ICreateRoleCommandHandler
{
    Task<CreateRoleOutput> ExecuteAsync(CreateRoleCommand cmd, CancellationToken ct);
}

namespace Atlas.Identity.Application.Queries.Roles.GetRoleById;

public interface IGetRoleByIdReader
{
    Task<GetRoleByIdDto?> GetByIdAsync(Guid tenantId, Guid roleId, CancellationToken ct);
}

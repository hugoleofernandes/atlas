using Atlas.Identity.Application.Queries.Roles.ListRoles;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Queries.Roles.GetRoleById;

public sealed class GetRoleByIdQueryHandler : IGetRoleByIdQueryHandler
{
    private readonly IGetRoleByIdReader _reader;
    private readonly IRequestContext _context;

    public GetRoleByIdQueryHandler(IGetRoleByIdReader reader, IRequestContext context)
    {
        _reader = reader;
        _context = context;
    }

    public Task<RoleDto?> ExecuteAsync(GetRoleByIdQuery query, CancellationToken ct)
    {
        var tenantId = _context.TenantId!.Value;
        return _reader.GetByIdAsync(tenantId, query.RoleId, ct);
    }
}

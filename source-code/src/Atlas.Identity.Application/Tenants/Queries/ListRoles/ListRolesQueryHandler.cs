using Atlas.Identity.Application.Tenants.Queries.Dtos;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Tenants.Queries.ListRoles;

public sealed class ListRolesQueryHandler : IListRolesQueryHandler
{
    private readonly IListRolesReader _reader;
    private readonly IRequestContext _context;

    public ListRolesQueryHandler(IListRolesReader reader, IRequestContext context)
    {
        _reader = reader;
        _context = context;
    }

    public Task<PagedResult<RoleDto>> ExecuteAsync(ListRolesQuery query, CancellationToken ct)
    {
        var tenantId = _context.TenantId!.Value;
        return _reader.ListAsync(tenantId, query.Page, query.PageSize, query.IncludeInactive, ct);
    }
}

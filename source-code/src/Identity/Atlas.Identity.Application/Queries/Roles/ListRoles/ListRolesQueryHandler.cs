using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Queries.Roles.ListRoles;

public sealed class ListRolesQueryHandler : IListRolesQueryHandler
{
    private readonly IListRolesReader _reader;
    private readonly IRequestContext  _context;

    public ListRolesQueryHandler(IListRolesReader reader, IRequestContext context)
    {
        _reader  = reader;
        _context = context;
    }

    public Task<IReadOnlyList<ListRolesDto>> ExecuteAsync(ListRolesQuery query, CancellationToken ct)
    {
        var tenantId = _context.TenantId!.Value;
        return _reader.ListAsync(tenantId, query.IsActive, ct);
    }
}

using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Tenants.Roles.Handlers.Queries.LookupRoles;

public sealed class LookupRolesQueryHandler : ILookupRolesQueryHandler
{
    private readonly ILookupRolesReader _reader;
    private readonly IRequestContext _context;

    public LookupRolesQueryHandler(ILookupRolesReader reader, IRequestContext context)
    {
        _reader  = reader;
        _context = context;
    }

    public Task<IReadOnlyList<RoleLookupDto>> ExecuteAsync(LookupRolesQuery query, CancellationToken ct)
    {
        var tenantId = _context.TenantId!.Value;
        return _reader.LookupAsync(tenantId, ct);
    }
}

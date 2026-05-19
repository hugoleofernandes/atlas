using Atlas.BuildingBlocks.Infrastructure.Workflows;
using Atlas.Identity.Application.Tenants.Queries.Dtos;
using Atlas.SharedKernel.Application;
using Microsoft.Extensions.Logging;

namespace Atlas.Identity.Application.Tenants.Queries.ListRoles;

public sealed class ListRolesQueryHandler
    : QueryHandlerBase<Query, PagedResult<RoleDto>>, IListRolesQueryHandler
{
    private readonly IListRolesReader _reader;
    private readonly IRequestContext _context;

    public ListRolesQueryHandler(
        IListRolesReader reader,
        IRequestContext context,
        ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        _reader = reader;
        _context = context;
    }

    protected override Task<PagedResult<RoleDto>> HandleAsync(Query query, CancellationToken ct)
    {
        var tenantId = _context.TenantId!.Value;
        return _reader.ListAsync(tenantId, query.Page, query.PageSize, query.IncludeInactive, ct);
    }
}

using Atlas.BuildingBlocks.Infrastructure.Workflows;
using Atlas.Identity.Application.Tenants.Queries.Dtos;
using Atlas.SharedKernel.Application;
using Microsoft.Extensions.Logging;

namespace Atlas.Identity.Application.Tenants.Queries.GetRoleById;

public sealed class GetRoleByIdQueryHandler
    : QueryHandlerBase<Query, RoleDto?>, IGetRoleByIdQueryHandler
{
    private readonly IGetRoleByIdReader _reader;
    private readonly IRequestContext _context;

    public GetRoleByIdQueryHandler(
        IGetRoleByIdReader reader,
        IRequestContext context,
        ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        _reader = reader;
        _context = context;
    }

    protected override Task<RoleDto?> HandleAsync(Query query, CancellationToken ct)
    {
        var tenantId = _context.TenantId!.Value;
        return _reader.GetByIdAsync(tenantId, query.RoleId, ct);
    }
}

using Atlas.SharedKernel.Application;

namespace Atlas.Party.Application.Queries.Organizations.ListOrganizations;

public sealed class ListOrganizationsQueryHandler : IListOrganizationsQueryHandler
{
    private readonly IListOrganizationsReader _reader;
    private readonly IRequestContext _context;

    public ListOrganizationsQueryHandler(IListOrganizationsReader reader, IRequestContext context)
    {
        _reader = reader;
        _context = context;
    }

    public Task<IReadOnlyList<OrganizationDto>> ExecuteAsync(ListOrganizationsQuery query, CancellationToken ct)
    {
        var tenantId = _context.TenantId!.Value;
        return _reader.ListAsync(tenantId, query.IsActive, ct);
    }
}

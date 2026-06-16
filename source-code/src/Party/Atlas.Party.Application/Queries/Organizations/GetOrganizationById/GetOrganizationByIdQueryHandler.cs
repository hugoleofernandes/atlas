using Atlas.SharedKernel.Application;

namespace Atlas.Party.Application.Queries.Organizations.GetOrganizationById;

public sealed class GetOrganizationByIdQueryHandler : IGetOrganizationByIdQueryHandler
{
    private readonly IGetOrganizationByIdReader _reader;
    private readonly IRequestContext _context;

    public GetOrganizationByIdQueryHandler(IGetOrganizationByIdReader reader, IRequestContext context)
    {
        _reader = reader;
        _context = context;
    }

    public Task<OrganizationDto?> ExecuteAsync(GetOrganizationByIdQuery query, CancellationToken ct)
    {
        var tenantId = _context.TenantId!.Value;
        return _reader.GetByIdAsync(tenantId, query.PartyId, ct);
    }
}

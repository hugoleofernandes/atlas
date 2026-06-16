using Atlas.SharedKernel.Application;

namespace Atlas.Party.Application.Queries.Individuals.GetIndividualById;

public sealed class GetIndividualByIdQueryHandler : IGetIndividualByIdQueryHandler
{
    private readonly IGetIndividualByIdReader _reader;
    private readonly IRequestContext _context;

    public GetIndividualByIdQueryHandler(IGetIndividualByIdReader reader, IRequestContext context)
    {
        _reader = reader;
        _context = context;
    }

    public Task<IndividualDto?> ExecuteAsync(GetIndividualByIdQuery query, CancellationToken ct)
    {
        var tenantId = _context.TenantId!.Value;
        return _reader.GetByIdAsync(tenantId, query.PartyId, ct);
    }
}

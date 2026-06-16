using Atlas.SharedKernel.Application;

namespace Atlas.Party.Application.Queries.Individuals.ListIndividuals;

public sealed class ListIndividualsQueryHandler : IListIndividualsQueryHandler
{
    private readonly IListIndividualsReader _reader;
    private readonly IRequestContext _context;

    public ListIndividualsQueryHandler(IListIndividualsReader reader, IRequestContext context)
    {
        _reader = reader;
        _context = context;
    }

    public Task<IReadOnlyList<IndividualDto>> ExecuteAsync(ListIndividualsQuery query, CancellationToken ct)
    {
        var tenantId = _context.TenantId!.Value;
        return _reader.ListAsync(tenantId, query.IsActive, ct);
    }
}

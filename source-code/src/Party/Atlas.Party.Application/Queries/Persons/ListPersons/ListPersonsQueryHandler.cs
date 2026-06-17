using Atlas.SharedKernel.Application;

namespace Atlas.Party.Application.Queries.Persons.ListPersons;

public sealed class ListPersonsQueryHandler : IListPersonsQueryHandler
{
    private readonly IListPersonsReader _reader;
    private readonly IRequestContext _context;

    public ListPersonsQueryHandler(IListPersonsReader reader, IRequestContext context)
    {
        _reader = reader;
        _context = context;
    }

    public Task<IReadOnlyList<PersonDto>> ExecuteAsync(ListPersonsQuery query, CancellationToken ct)
    {
        var tenantId = _context.TenantId!.Value;
        return _reader.ListAsync(tenantId, query.IsActive, ct);
    }
}


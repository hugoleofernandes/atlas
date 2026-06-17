using Atlas.SharedKernel.Application;

namespace Atlas.Party.Application.Queries.Persons.GetPersonById;

public sealed class GetPersonByIdQueryHandler : IGetPersonByIdQueryHandler
{
    private readonly IGetPersonByIdReader _reader;
    private readonly IRequestContext _context;

    public GetPersonByIdQueryHandler(IGetPersonByIdReader reader, IRequestContext context)
    {
        _reader = reader;
        _context = context;
    }

    public Task<PersonDto?> ExecuteAsync(GetPersonByIdQuery query, CancellationToken ct)
    {
        var tenantId = _context.TenantId!.Value;
        return _reader.GetByIdAsync(tenantId, query.PartyId, ct);
    }
}


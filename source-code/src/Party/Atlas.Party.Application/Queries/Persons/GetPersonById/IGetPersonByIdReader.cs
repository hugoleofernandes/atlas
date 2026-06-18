namespace Atlas.Party.Application.Queries.Persons.GetPersonById;

public interface IGetPersonByIdReader
{
    Task<GetPersonByIdDto?> GetByIdAsync(Guid tenantId, Guid partyId, CancellationToken ct);
}


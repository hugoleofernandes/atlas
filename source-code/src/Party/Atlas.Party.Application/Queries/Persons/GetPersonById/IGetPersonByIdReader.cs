namespace Atlas.Party.Application.Queries.Persons.GetPersonById;

public interface IGetPersonByIdReader
{
    Task<PersonDto?> GetByIdAsync(Guid tenantId, Guid partyId, CancellationToken ct);
}


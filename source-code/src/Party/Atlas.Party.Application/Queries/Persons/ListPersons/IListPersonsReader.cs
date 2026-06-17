namespace Atlas.Party.Application.Queries.Persons.ListPersons;

public interface IListPersonsReader
{
    Task<IReadOnlyList<PersonDto>> ListAsync(Guid tenantId, bool? isActive, CancellationToken ct);
}


namespace Atlas.Party.Application.Queries.Persons.ListPersons;

public interface IListPersonsReader
{
    Task<IReadOnlyList<ListPersonsDto>> ListAsync(Guid tenantId, bool? isActive, CancellationToken ct);
}


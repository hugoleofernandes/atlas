using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Application.Queries.Persons.ListPersons;

public interface IListPersonsReader
{
    Task<IReadOnlyList<ListPersonsDto>> ListAsync(Guid tenantId, bool? isActive, ClassificationType? classification, CancellationToken ct);
}

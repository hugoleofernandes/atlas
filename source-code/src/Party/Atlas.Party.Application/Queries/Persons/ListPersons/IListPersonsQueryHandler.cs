using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Party.Application.Queries.Persons.ListPersons;

public interface IListPersonsQueryHandler : IQueryHandler<ListPersonsQuery, IReadOnlyList<ListPersonsDto>>
{
}


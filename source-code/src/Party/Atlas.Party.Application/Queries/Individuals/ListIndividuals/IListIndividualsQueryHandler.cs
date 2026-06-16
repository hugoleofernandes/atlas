using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Party.Application.Queries.Individuals.ListIndividuals;

public interface IListIndividualsQueryHandler : IQueryHandler<ListIndividualsQuery, IReadOnlyList<IndividualDto>>
{
}

using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Party.Application.Queries.Individuals.GetIndividualById;

public interface IGetIndividualByIdQueryHandler : IQueryHandler<GetIndividualByIdQuery, IndividualDto?>
{
}

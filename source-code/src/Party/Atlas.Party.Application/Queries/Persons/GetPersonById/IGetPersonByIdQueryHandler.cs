using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Party.Application.Queries.Persons.GetPersonById;

public interface IGetPersonByIdQueryHandler : IQueryHandler<GetPersonByIdQuery, GetPersonByIdDto?>
{
}


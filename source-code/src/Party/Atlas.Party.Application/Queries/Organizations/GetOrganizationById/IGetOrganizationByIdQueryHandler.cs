using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Party.Application.Queries.Organizations.GetOrganizationById;

public interface IGetOrganizationByIdQueryHandler : IQueryHandler<GetOrganizationByIdQuery, OrganizationDto?>
{
}

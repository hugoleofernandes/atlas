using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Party.Application.Queries.Organizations.ListOrganizations;

public interface IListOrganizationsQueryHandler : IQueryHandler<ListOrganizationsQuery, IReadOnlyList<OrganizationDto>>
{
}

namespace Atlas.Party.Application.Queries.Organizations.ListOrganizations;

public interface IListOrganizationsReader
{
    Task<IReadOnlyList<OrganizationDto>> ListAsync(Guid tenantId, bool? isActive, CancellationToken ct);
}

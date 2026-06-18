namespace Atlas.Party.Application.Queries.Organizations.ListOrganizations;

public interface IListOrganizationsReader
{
    Task<IReadOnlyList<ListOrganizationsDto>> ListAsync(Guid tenantId, bool? isActive, CancellationToken ct);
}

namespace Atlas.Party.Application.Queries.Organizations.GetOrganizationById;

public interface IGetOrganizationByIdReader
{
    Task<OrganizationDto?> GetByIdAsync(Guid tenantId, Guid partyId, CancellationToken ct);
}

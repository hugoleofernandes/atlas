namespace Atlas.Party.Application.Queries.Organizations.GetOrganizationById;

public interface IGetOrganizationByIdReader
{
    Task<GetOrganizationByIdDto?> GetByIdAsync(Guid tenantId, Guid partyId, CancellationToken ct);
}

namespace Atlas.Party.Application.Queries.Individuals.GetIndividualById;

public interface IGetIndividualByIdReader
{
    Task<IndividualDto?> GetByIdAsync(Guid tenantId, Guid partyId, CancellationToken ct);
}

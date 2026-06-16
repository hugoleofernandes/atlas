namespace Atlas.Party.Application.Queries.Individuals.ListIndividuals;

public interface IListIndividualsReader
{
    Task<IReadOnlyList<IndividualDto>> ListAsync(Guid tenantId, bool? isActive, CancellationToken ct);
}

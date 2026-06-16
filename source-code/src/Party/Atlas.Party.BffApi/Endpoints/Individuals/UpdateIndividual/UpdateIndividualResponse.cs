using Atlas.Party.Application.Commands.UpdateIndividual;

namespace Atlas.Party.BffApi.Endpoints.Individuals.UpdateIndividual;

public sealed record UpdateIndividualResponse(Guid PartyId, string FullName)
{
    public static UpdateIndividualResponse From(UpdateIndividualOutput output)
        => new(output.PartyId, output.FullName);
}

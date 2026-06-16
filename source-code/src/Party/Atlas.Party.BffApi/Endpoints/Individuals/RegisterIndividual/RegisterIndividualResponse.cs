using Atlas.Party.Application.Commands.RegisterIndividual;

namespace Atlas.Party.BffApi.Endpoints.Individuals.RegisterIndividual;

public sealed record RegisterIndividualResponse(Guid PartyId, string TaxNumber, string FullName)
{
    public static RegisterIndividualResponse From(RegisterIndividualOutput output)
        => new(output.PartyId, output.TaxNumber, output.FullName);
}

using Atlas.Party.Application.Commands.RegisterPerson;

namespace Atlas.Party.BffApi.Endpoints.Persons.RegisterPerson;

public sealed record RegisterPersonResponse(Guid PartyId, string TaxNumber, string FullName)
{
    public static RegisterPersonResponse From(RegisterPersonOutput output)
        => new(output.PartyId, output.TaxNumber, output.FullName);
}


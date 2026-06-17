using Atlas.Party.Application.Commands.UpdatePerson;

namespace Atlas.Party.BffApi.Endpoints.Persons.UpdatePerson;

public sealed record UpdatePersonResponse(Guid PartyId, string FullName)
{
    public static UpdatePersonResponse From(UpdatePersonOutput output)
        => new(output.PartyId, output.FullName);
}


namespace Atlas.Party.Application.Commands.DeactivatePerson;

public sealed record DeactivatePersonOutput(Guid PartyId, bool IsActive);


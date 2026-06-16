namespace Atlas.Party.Application.Commands.DeactivateIndividual;

public sealed record DeactivateIndividualOutput(Guid PartyId, bool IsActive);

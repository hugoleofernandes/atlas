namespace Atlas.Party.Application.Commands.RegisterIndividual;

public sealed record RegisterIndividualOutput(Guid PartyId, string TaxNumber, string FullName);

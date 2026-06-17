namespace Atlas.Party.Application.Commands.RegisterPerson;

public sealed record RegisterPersonOutput(Guid PartyId, string TaxNumber, string FullName);


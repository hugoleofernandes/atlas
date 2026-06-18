namespace Atlas.Party.Application.Queries.Persons.GetPersonById;

public sealed record GetPersonByIdContactDto(Guid ContactId, string Type, string Value, bool IsPrimary);

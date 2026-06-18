namespace Atlas.Party.Application.Queries.Persons.GetPersonById;

public sealed record GetPersonByIdClassificationDto(string Type, DateOnly Since, DateOnly? Until);

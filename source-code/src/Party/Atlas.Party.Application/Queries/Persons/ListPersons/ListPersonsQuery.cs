using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Application.Queries.Persons.ListPersons;

public sealed record ListPersonsQuery(bool? IsActive, ClassificationType? Classification);

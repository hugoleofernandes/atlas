using Atlas.Party.Domain.Shared;

namespace Atlas.Party.BffApi.Endpoints.Persons.ListPersons;

public sealed class ListPersonsRequest
{
    public bool? IsActive { get; init; }
    public ClassificationType? Classification { get; init; }
}


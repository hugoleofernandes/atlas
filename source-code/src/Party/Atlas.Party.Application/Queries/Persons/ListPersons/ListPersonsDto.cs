using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Application.Queries.Persons.ListPersons;

public sealed record ListPersonsDto(
    Guid PartyId,
    string TaxNumber,
    string FirstName,
    string LastName,
    string? MiddleName,
    string FullName,
    DateOnly? BirthDate,
    Gender? Gender,
    bool IsActive,
    DateTime CreatedAt,
    Guid? CreatedBy,
    string? CreatedByEmail,
    DateTime? UpdatedAt,
    Guid? UpdatedBy,
    string? UpdatedByEmail
);

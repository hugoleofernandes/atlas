using Atlas.Party.Application.Queries.Shared;
using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Application.Queries.Persons;

public sealed record PersonDto(
    Guid PartyId,
    string TaxNumber,
    string FirstName,
    string LastName,
    string? MiddleName,
    string FullName,
    DateOnly? BirthDate,
    Gender? Gender,
    bool IsActive,
    IReadOnlyList<AddressDto> Addresses,
    DateTime CreatedAt,
    Guid? CreatedBy,
    string? CreatedByEmail,
    DateTime? UpdatedAt,
    Guid? UpdatedBy,
    string? UpdatedByEmail
);


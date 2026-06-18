using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Application.Queries.Persons.GetPersonById;

public sealed record GetPersonByIdDto(
    Guid PartyId,
    string TaxNumber,
    string FirstName,
    string LastName,
    string? MiddleName,
    string FullName,
    DateOnly? BirthDate,
    Gender? Gender,
    bool IsActive,
    string? Notes,
    IReadOnlyList<GetPersonByIdAddressDto> Addresses,
    IReadOnlyList<GetPersonByIdContactDto> Contacts,
    IReadOnlyList<GetPersonByIdClassificationDto> Classifications,
    DateTime CreatedAt,
    Guid? CreatedBy,
    string? CreatedByEmail,
    DateTime? UpdatedAt,
    Guid? UpdatedBy,
    string? UpdatedByEmail
);

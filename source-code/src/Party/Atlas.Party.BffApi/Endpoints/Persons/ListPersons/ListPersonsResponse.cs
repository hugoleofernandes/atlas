using Atlas.Party.Application.Queries.Lookups;
using Atlas.Party.Application.Queries.Persons.ListPersons;

namespace Atlas.Party.BffApi.Endpoints.Persons.ListPersons;

public sealed record ListPersonsResponse(
    Guid PartyId,
    string TaxNumber,
    string FirstName,
    string LastName,
    string? MiddleName,
    string FullName,
    DateOnly? BirthDate,
    string? Gender,
    bool IsActive,
    IReadOnlyList<ListPersonsClassificationResponse> Classifications,
    DateTime CreatedAt,
    Guid? CreatedBy,
    string? CreatedByEmail,
    DateTime? UpdatedAt,
    Guid? UpdatedBy,
    string? UpdatedByEmail
)
{
    public static IReadOnlyList<ListPersonsResponse> FromList(
        IReadOnlyList<ListPersonsDto> result,
        IPartyLookupLabelLocalizer localizer
    )
    {
        return result
            .Select(x => From(x, localizer))
            .ToList();
    }

    public static ListPersonsResponse From(ListPersonsDto dto, IPartyLookupLabelLocalizer localizer)
    {
        var genderCode = dto.Gender?.ToString();

        return new ListPersonsResponse(
            PartyId: dto.PartyId,
            TaxNumber: dto.TaxNumber,
            FirstName: dto.FirstName,
            LastName: dto.LastName,
            MiddleName: dto.MiddleName,
            FullName: dto.FullName,
            BirthDate: dto.BirthDate,
            Gender: genderCode is null ? null : localizer.GetGenderName(genderCode),
            IsActive: dto.IsActive,
            Classifications: ListPersonsClassificationResponse.FromList(dto.Classifications, localizer),
            CreatedAt: dto.CreatedAt,
            CreatedBy: dto.CreatedBy,
            CreatedByEmail: dto.CreatedByEmail,
            UpdatedAt: dto.UpdatedAt,
            UpdatedBy: dto.UpdatedBy,
            UpdatedByEmail: dto.UpdatedByEmail
        );
    }
}

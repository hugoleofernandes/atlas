using Atlas.Party.Application.Queries.Persons.GetPersonById;
using Atlas.Party.Domain.Shared;
using Atlas.Party.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Party.Infrastructure.Readers.Persons.GetPersonById;

public sealed class GetPersonByIdReader(PartyDbContext db) : IGetPersonByIdReader
{
    private const string Sql = """
        SELECT
            p.id               AS PartyId,
            p.tax_number       AS TaxNumber,
            p.first_name       AS FirstName,
            p.last_name        AS LastName,
            p.middle_name      AS MiddleName,
            p.birth_date       AS BirthDate,
            p.gender           AS Gender,
            p.is_active        AS IsActive,
            p.created_at       AS CreatedAt,
            p.created_by       AS CreatedBy,
            p.created_by_email AS CreatedByEmail,
            p.updated_at       AS UpdatedAt,
            p.updated_by       AS UpdatedBy,
            p.updated_by_email AS UpdatedByEmail
        FROM atlas_party.parties p
        WHERE p.tenant_id = @TenantId
          AND p.id = @PartyId
          AND p.party_type = 'Person'
        """;

    public async Task<GetPersonByIdDto?> GetByIdAsync(Guid tenantId, Guid partyId, CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();

        var row = await conn.QuerySingleOrDefaultAsync<PersonRow>(
            Sql,
            new { TenantId = tenantId, PartyId = partyId }
        );

        if (row is null)
            return null;

        var addresses = (await conn.QueryAsync<GetPersonByIdAddressDto>(AddressesSql, new { PartyId = partyId })).ToList();
        var contacts = (await conn.QueryAsync<GetPersonByIdContactDto>(ContactsSql, new { PartyId = partyId })).ToList();

        var gender = row.Gender is null ? (Gender?)null : Enum.Parse<Gender>(row.Gender);

        return new GetPersonByIdDto(
            PartyId: row.PartyId,
            TaxNumber: row.TaxNumber,
            FirstName: row.FirstName,
            LastName: row.LastName,
            MiddleName: row.MiddleName,
            FullName: row.MiddleName is null ? $"{row.FirstName} {row.LastName}" : $"{row.FirstName} {row.MiddleName} {row.LastName}",
            BirthDate: row.BirthDate,
            Gender: gender,
            IsActive: row.IsActive,
            Addresses: addresses,
            Contacts: contacts,
            CreatedAt: row.CreatedAt,
            CreatedBy: row.CreatedBy,
            CreatedByEmail: row.CreatedByEmail,
            UpdatedAt: row.UpdatedAt,
            UpdatedBy: row.UpdatedBy,
            UpdatedByEmail: row.UpdatedByEmail
        );
    }

    private sealed record PersonRow(
        Guid PartyId,
        string TaxNumber,
        string FirstName,
        string LastName,
        string? MiddleName,
        DateOnly? BirthDate,
        string? Gender,
        bool IsActive,
        DateTime CreatedAt,
        Guid? CreatedBy,
        string? CreatedByEmail,
        DateTime? UpdatedAt,
        Guid? UpdatedBy,
        string? UpdatedByEmail
    );

    private const string AddressesSql = """
        SELECT
            id          AS AddressId,
            type        AS Type,
            street      AS Street,
            number      AS Number,
            complement  AS Complement,
            district    AS District,
            city        AS City,
            state       AS State,
            zip_code    AS ZipCode,
            country     AS Country,
            is_primary  AS IsPrimary
        FROM atlas_party.party_addresses
        WHERE party_id = @PartyId
        ORDER BY is_primary DESC, type ASC, created_at ASC
        """;

    private const string ContactsSql = """
        SELECT
            id          AS ContactId,
            type        AS Type,
            value       AS Value,
            is_primary  AS IsPrimary
        FROM atlas_party.party_contacts
        WHERE party_id = @PartyId
        ORDER BY is_primary DESC, type ASC, created_at ASC
        """;
}


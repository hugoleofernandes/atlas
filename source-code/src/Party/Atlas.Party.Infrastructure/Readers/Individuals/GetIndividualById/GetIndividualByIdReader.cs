using Atlas.Party.Application.Queries.Individuals;
using Atlas.Party.Application.Queries.Individuals.GetIndividualById;
using Atlas.Party.Application.Queries.Shared;
using Atlas.Party.Domain.Shared;
using Atlas.Party.Infrastructure.Persistence.DbContexts;
using Atlas.Party.Infrastructure.Readers.Shared;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Party.Infrastructure.Readers.Individuals.GetIndividualById;

public sealed class GetIndividualByIdReader(PartyDbContext db) : IGetIndividualByIdReader
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
          AND p.party_type = 'Individual'
        """;

    public async Task<IndividualDto?> GetByIdAsync(Guid tenantId, Guid partyId, CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();

        var row = await conn.QuerySingleOrDefaultAsync<IndividualRow>(
            Sql,
            new { TenantId = tenantId, PartyId = partyId }
        );

        if (row is null)
            return null;

        var addresses = await AddressReaderSql.ListByPartyIdAsync(conn, partyId);

        var gender = row.Gender is null ? (Gender?)null : Enum.Parse<Gender>(row.Gender);

        return new IndividualDto(
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
            CreatedAt: row.CreatedAt,
            CreatedBy: row.CreatedBy,
            CreatedByEmail: row.CreatedByEmail,
            UpdatedAt: row.UpdatedAt,
            UpdatedBy: row.UpdatedBy,
            UpdatedByEmail: row.UpdatedByEmail
        );
    }

    private sealed record IndividualRow(
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
}

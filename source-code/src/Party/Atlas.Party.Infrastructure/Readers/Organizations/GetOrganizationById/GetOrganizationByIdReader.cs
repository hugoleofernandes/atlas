using Atlas.Party.Application.Queries.Organizations;
using Atlas.Party.Application.Queries.Organizations.GetOrganizationById;
using Atlas.Party.Domain.Shared;
using Atlas.Party.Infrastructure.Persistence.DbContexts;
using Atlas.Party.Infrastructure.Readers.Shared;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Party.Infrastructure.Readers.Organizations.GetOrganizationById;

public sealed class GetOrganizationByIdReader(PartyDbContext db) : IGetOrganizationByIdReader
{
    private const string Sql = """
        SELECT
            p.id               AS PartyId,
            p.tax_number       AS TaxNumber,
            p.legal_name       AS LegalName,
            p.trade_name       AS TradeName,
            p.legal_type       AS LegalType,
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
          AND p.party_type = 'Organization'
        """;

    public async Task<OrganizationDto?> GetByIdAsync(Guid tenantId, Guid partyId, CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();

        var row = await conn.QuerySingleOrDefaultAsync<OrganizationRow>(
            Sql,
            new { TenantId = tenantId, PartyId = partyId }
        );

        if (row is null)
            return null;

        var addresses = await AddressReaderSql.ListByPartyIdAsync(conn, partyId);

        return new OrganizationDto(
            PartyId: row.PartyId,
            TaxNumber: row.TaxNumber,
            LegalName: row.LegalName,
            TradeName: row.TradeName,
            LegalType: Enum.Parse<LegalType>(row.LegalType),
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

    private sealed record OrganizationRow(
        Guid PartyId,
        string TaxNumber,
        string LegalName,
        string? TradeName,
        string LegalType,
        bool IsActive,
        DateTime CreatedAt,
        Guid? CreatedBy,
        string? CreatedByEmail,
        DateTime? UpdatedAt,
        Guid? UpdatedBy,
        string? UpdatedByEmail
    );
}

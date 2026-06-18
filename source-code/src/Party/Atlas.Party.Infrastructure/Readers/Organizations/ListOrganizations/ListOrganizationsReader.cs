using Atlas.Party.Application.Queries.Organizations.ListOrganizations;
using Atlas.Party.Domain.Shared;
using Atlas.Party.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Atlas.Party.Infrastructure.Readers.Organizations.ListOrganizations;

public sealed class ListOrganizationsReader(PartyDbContext db) : IListOrganizationsReader
{
    private const string SqlBase = """
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
          AND p.party_type = 'Organization'
        """;

    private const string OrderBySql = "ORDER BY p.legal_name ASC";

    public async Task<IReadOnlyList<ListOrganizationsDto>> ListAsync(Guid tenantId, bool? isActive, CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();

        var sql = new StringBuilder();
        sql.AppendLine(SqlBase);
        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);

        if (isActive is not null)
        {
            sql.AppendLine("  AND p.is_active = @IsActive");
            parameters.Add("IsActive", isActive.Value);
        }

        sql.AppendLine(OrderBySql);

        var rows = await conn.QueryAsync<OrganizationRow>(sql.ToString(), parameters);

        return rows
            .Select(r => new ListOrganizationsDto(
                PartyId: r.PartyId,
                TaxNumber: r.TaxNumber,
                LegalName: r.LegalName,
                TradeName: r.TradeName,
                LegalType: Enum.Parse<LegalType>(r.LegalType),
                IsActive: r.IsActive,
                Addresses: Array.Empty<ListOrganizationsAddressDto>(),
                CreatedAt: r.CreatedAt,
                CreatedBy: r.CreatedBy,
                CreatedByEmail: r.CreatedByEmail,
                UpdatedAt: r.UpdatedAt,
                UpdatedBy: r.UpdatedBy,
                UpdatedByEmail: r.UpdatedByEmail
            ))
            .ToList();
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

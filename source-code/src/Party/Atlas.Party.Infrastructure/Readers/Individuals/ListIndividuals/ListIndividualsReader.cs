using Atlas.Party.Application.Queries.Individuals;
using Atlas.Party.Application.Queries.Individuals.ListIndividuals;
using Atlas.Party.Domain.Shared;
using Atlas.Party.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Atlas.Party.Infrastructure.Readers.Individuals.ListIndividuals;

public sealed class ListIndividualsReader(PartyDbContext db) : IListIndividualsReader
{
    private const string SqlBase = """
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
          AND p.party_type = 'Individual'
        """;

    private const string OrderBySql = "ORDER BY p.first_name ASC, p.last_name ASC";

    public async Task<IReadOnlyList<IndividualDto>> ListAsync(Guid tenantId, bool? isActive, CancellationToken ct)
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

        var rows = await conn.QueryAsync<IndividualRow>(sql.ToString(), parameters);

        return rows
            .Select(r => new IndividualDto(
                PartyId: r.PartyId,
                TaxNumber: r.TaxNumber,
                FirstName: r.FirstName,
                LastName: r.LastName,
                MiddleName: r.MiddleName,
                FullName: r.MiddleName is null ? $"{r.FirstName} {r.LastName}" : $"{r.FirstName} {r.MiddleName} {r.LastName}",
                BirthDate: r.BirthDate,
                Gender: r.Gender is null ? (Gender?)null : Enum.Parse<Gender>(r.Gender),
                IsActive: r.IsActive,
                Addresses: [],
                CreatedAt: r.CreatedAt,
                CreatedBy: r.CreatedBy,
                CreatedByEmail: r.CreatedByEmail,
                UpdatedAt: r.UpdatedAt,
                UpdatedBy: r.UpdatedBy,
                UpdatedByEmail: r.UpdatedByEmail
            ))
            .ToList();
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

using Atlas.Party.Application.Queries.Persons.ListPersons;
using Atlas.Party.Domain.Shared;
using Atlas.Party.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Atlas.Party.Infrastructure.Readers.Persons.ListPersons;

public sealed class ListPersonsReader(PartyDbContext db) : IListPersonsReader
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
          AND p.party_type = 'Person'
        """;

    private const string OrderBySql = "ORDER BY p.first_name ASC, p.last_name ASC";

    private const string ClassificationsSql = """
        SELECT
            party_id AS PartyId,
            type     AS Code
        FROM atlas_party.party_classifications
        WHERE party_id = ANY(@PartyIds)
        ORDER BY type ASC
        """;

    public async Task<IReadOnlyList<ListPersonsDto>> ListAsync(Guid tenantId, bool? isActive, ClassificationType? classification, CancellationToken ct)
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

        if (classification is not null)
        {
            sql.AppendLine("  AND EXISTS (SELECT 1 FROM atlas_party.party_classifications pc WHERE pc.party_id = p.id AND pc.type = @Classification)");
            parameters.Add("Classification", classification.Value.ToString());
        }

        sql.AppendLine(OrderBySql);

        var rows = await conn.QueryAsync<PersonRow>(sql.ToString(), parameters);
        var rowList = rows.ToList();

        var partyIds = rowList
            .Select(row => row.PartyId)
            .Distinct()
            .ToArray();

        var classifications = partyIds.Length == 0
            ? []
            : (await conn.QueryAsync<ClassificationRow>(ClassificationsSql, new { PartyIds = partyIds })).ToList();

        var classificationsByPartyId = classifications
            .ToLookup(row => row.PartyId, row => new ListPersonsClassificationDto(row.Code));

        return rowList
            .Select(r => new ListPersonsDto(
                PartyId: r.PartyId,
                TaxNumber: r.TaxNumber,
                FirstName: r.FirstName,
                LastName: r.LastName,
                MiddleName: r.MiddleName,
                FullName: r.MiddleName is null ? $"{r.FirstName} {r.LastName}" : $"{r.FirstName} {r.MiddleName} {r.LastName}",
                BirthDate: r.BirthDate,
                Gender: r.Gender is null ? (Gender?)null : Enum.Parse<Gender>(r.Gender),
                IsActive: r.IsActive,
                Classifications: classificationsByPartyId[r.PartyId].ToList(),
                CreatedAt: r.CreatedAt,
                CreatedBy: r.CreatedBy,
                CreatedByEmail: r.CreatedByEmail,
                UpdatedAt: r.UpdatedAt,
                UpdatedBy: r.UpdatedBy,
                UpdatedByEmail: r.UpdatedByEmail
            ))
            .ToList();
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

    private sealed record ClassificationRow(Guid PartyId, string Code);
}

using Atlas.Party.Application.Queries.Shared;
using Atlas.Party.Domain.Shared;
using Dapper;
using System.Data;

namespace Atlas.Party.Infrastructure.Readers.Shared;

/// <summary>
/// Loads a Party's addresses for the GetById readers. Reused by Individual and Organization
/// since both project the same atlas_party.party_addresses shape.
/// </summary>
internal static class AddressReaderSql
{
    private const string Sql = """
        SELECT
            a.id          AS AddressId,
            a.type        AS Type,
            a.street      AS Street,
            a.number      AS Number,
            a.complement  AS Complement,
            a.district    AS District,
            a.city        AS City,
            a.state       AS State,
            a.zip_code    AS ZipCode,
            a.country     AS Country,
            a.is_primary  AS IsPrimary
        FROM atlas_party.party_addresses a
        WHERE a.party_id = @PartyId
        ORDER BY a.type ASC
        """;

    public static async Task<IReadOnlyList<AddressDto>> ListByPartyIdAsync(IDbConnection conn, Guid partyId)
    {
        var rows = await conn.QueryAsync<AddressRow>(Sql, new { PartyId = partyId });

        return rows
            .Select(r => new AddressDto(
                AddressId: r.AddressId,
                Type: Enum.Parse<AddressType>(r.Type),
                Street: r.Street,
                Number: r.Number,
                Complement: r.Complement,
                District: r.District,
                City: r.City,
                State: r.State,
                ZipCode: r.ZipCode,
                Country: r.Country,
                IsPrimary: r.IsPrimary
            ))
            .ToList();
    }

    private sealed record AddressRow(
        Guid AddressId,
        string Type,
        string Street,
        string Number,
        string? Complement,
        string District,
        string City,
        string State,
        string ZipCode,
        string Country,
        bool IsPrimary
    );
}

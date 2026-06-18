using Atlas.Platform.Application.Queries.Geography.GetCitiesByState;
using Atlas.Platform.Application.Queries.Geography.GetStatesByCountry;
using Atlas.Platform.Domain.Geography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atlas.Platform.Infrastructure.Seeders;

public sealed partial class PlatformModuleSeeder
{
    // Fixed IDs — deterministic, stable across re-seeds.
    // Pattern: 00000000-0000-0000-9999-{seq:012d} (sequential by state code alphabetical order)
    internal static readonly Guid StateIdAC = new("00000000-0000-0000-9999-000000000001");
    internal static readonly Guid StateIdAL = new("00000000-0000-0000-9999-000000000002");
    internal static readonly Guid StateIdAM = new("00000000-0000-0000-9999-000000000003");
    internal static readonly Guid StateIdAP = new("00000000-0000-0000-9999-000000000004");
    internal static readonly Guid StateIdBA = new("00000000-0000-0000-9999-000000000005");
    internal static readonly Guid StateIdCE = new("00000000-0000-0000-9999-000000000006");
    internal static readonly Guid StateIdDF = new("00000000-0000-0000-9999-000000000007");
    internal static readonly Guid StateIdES = new("00000000-0000-0000-9999-000000000008");
    internal static readonly Guid StateIdGO = new("00000000-0000-0000-9999-000000000009");
    internal static readonly Guid StateIdMA = new("00000000-0000-0000-9999-000000000010");
    internal static readonly Guid StateIdMG = new("00000000-0000-0000-9999-000000000011");
    internal static readonly Guid StateIdMS = new("00000000-0000-0000-9999-000000000012");
    internal static readonly Guid StateIdMT = new("00000000-0000-0000-9999-000000000013");
    internal static readonly Guid StateIdPA = new("00000000-0000-0000-9999-000000000014");
    internal static readonly Guid StateIdPB = new("00000000-0000-0000-9999-000000000015");
    internal static readonly Guid StateIdPE = new("00000000-0000-0000-9999-000000000016");
    internal static readonly Guid StateIdPI = new("00000000-0000-0000-9999-000000000017");
    internal static readonly Guid StateIdPR = new("00000000-0000-0000-9999-000000000018");
    internal static readonly Guid StateIdRJ = new("00000000-0000-0000-9999-000000000019");
    internal static readonly Guid StateIdRN = new("00000000-0000-0000-9999-000000000020");
    internal static readonly Guid StateIdRO = new("00000000-0000-0000-9999-000000000021");
    internal static readonly Guid StateIdRR = new("00000000-0000-0000-9999-000000000022");
    internal static readonly Guid StateIdRS = new("00000000-0000-0000-9999-000000000023");
    internal static readonly Guid StateIdSC = new("00000000-0000-0000-9999-000000000024");
    internal static readonly Guid StateIdSE = new("00000000-0000-0000-9999-000000000025");
    internal static readonly Guid StateIdSP = new("00000000-0000-0000-9999-000000000026");
    internal static readonly Guid StateIdTO = new("00000000-0000-0000-9999-000000000027");

    private async Task SeedGeographyAsync(
        IGetStatesByCountryCache statesCache,
        IGetCitiesByStateCache citiesCache,
        CancellationToken ct)
    {
        await SeedBrazilStatesAsync(statesCache, ct);
        await SeedBrazilCitiesAsync(citiesCache, ct);
    }

    private async Task SeedBrazilStatesAsync(IGetStatesByCountryCache statesCache, CancellationToken ct)
    {
        if (await db.States.AnyAsync(s => s.CountryCode == "BR", ct))
        {
            logger.LogInformation("PlatformGeographySeeder (BR states) skipped - data already exists");
            return;
        }

        logger.LogInformation("PlatformGeographySeeder (BR states) started");

        State[] states =
        [
            State.Create(StateIdAC, "BR", "AC", "Acre"),
            State.Create(StateIdAL, "BR", "AL", "Alagoas"),
            State.Create(StateIdAM, "BR", "AM", "Amazonas"),
            State.Create(StateIdAP, "BR", "AP", "Amapá"),
            State.Create(StateIdBA, "BR", "BA", "Bahia"),
            State.Create(StateIdCE, "BR", "CE", "Ceará"),
            State.Create(StateIdDF, "BR", "DF", "Distrito Federal"),
            State.Create(StateIdES, "BR", "ES", "Espírito Santo"),
            State.Create(StateIdGO, "BR", "GO", "Goiás"),
            State.Create(StateIdMA, "BR", "MA", "Maranhão"),
            State.Create(StateIdMG, "BR", "MG", "Minas Gerais"),
            State.Create(StateIdMS, "BR", "MS", "Mato Grosso do Sul"),
            State.Create(StateIdMT, "BR", "MT", "Mato Grosso"),
            State.Create(StateIdPA, "BR", "PA", "Pará"),
            State.Create(StateIdPB, "BR", "PB", "Paraíba"),
            State.Create(StateIdPE, "BR", "PE", "Pernambuco"),
            State.Create(StateIdPI, "BR", "PI", "Piauí"),
            State.Create(StateIdPR, "BR", "PR", "Paraná"),
            State.Create(StateIdRJ, "BR", "RJ", "Rio de Janeiro"),
            State.Create(StateIdRN, "BR", "RN", "Rio Grande do Norte"),
            State.Create(StateIdRO, "BR", "RO", "Rondônia"),
            State.Create(StateIdRR, "BR", "RR", "Roraima"),
            State.Create(StateIdRS, "BR", "RS", "Rio Grande do Sul"),
            State.Create(StateIdSC, "BR", "SC", "Santa Catarina"),
            State.Create(StateIdSE, "BR", "SE", "Sergipe"),
            State.Create(StateIdSP, "BR", "SP", "São Paulo"),
            State.Create(StateIdTO, "BR", "TO", "Tocantins"),
        ];

        await db.States.AddRangeAsync(states, ct);
        await uow.SaveChangesAsync(ct);

        statesCache.Invalidate();

        logger.LogInformation("PlatformGeographySeeder (BR states) completed — 27 states seeded");
    }
}

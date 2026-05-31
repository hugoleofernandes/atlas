using Atlas.Platform.Domain.Tenants.Exceptions;

namespace Atlas.Platform.Application.Queries.Tenants.GetTenantByName;

public sealed class GetTenantByNameQueryHandler : IGetTenantByNameQueryHandler
{
    private readonly IGetTenantByNameReader _reader;

    public GetTenantByNameQueryHandler(IGetTenantByNameReader reader)
    {
        _reader = reader;
    }

    public async Task<TenantInfoDto> ExecuteAsync(GetTenantByNameQuery query, CancellationToken ct)
    {
        var dto = await _reader.ReadAsync(query.TenantName.ToLowerInvariant(), ct);

        if (dto is null)
            throw new TenantNotFoundException(query.TenantName);

        return dto;
    }
}

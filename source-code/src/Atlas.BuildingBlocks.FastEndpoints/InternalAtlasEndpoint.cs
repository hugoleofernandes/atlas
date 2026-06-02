using System.Security.Cryptography;
using System.Text;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Idempotency;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Atlas.BuildingBlocks.FastEndpoints;

public abstract class InternalAtlasEndpoint<TReq, TRes> : AtlasEndpoint<TReq, TRes>
    where TReq : notnull
{
    protected async Task<bool> AuthorizeAndHydrateOutboxContextAsync(CancellationToken ct)
    {
        if (!IsInternalApiKeyValid())
        {
            await Send.UnauthorizedAsync(ct);
            return false;
        }

        if (!TryGetGuidHeader(InternalApiHeaders.TenantId, out var tenantId)
            || !TryGetGuidHeader(InternalApiHeaders.UserId, out var userId)
            || !TryGetGuidHeader(InternalApiHeaders.IdempotencyKey, out var idempotencyKey)
            || !TryGetStringHeader(InternalApiHeaders.OutboxSubscription, out var subscription))
        {
            await Send.ResultAsync(Results.BadRequest("Missing or invalid internal outbox headers."));
            return false;
        }

        TryGetStringHeader(InternalApiHeaders.UserEmail, out var userEmail);
        TryGetStringHeader(InternalApiHeaders.CorrelationId, out var correlationId);

        var requestContextSetter = Resolve<IRequestContextSetter>();
        requestContextSetter.Set(tenantId, string.Empty, userId, userEmail);

        if (!string.IsNullOrWhiteSpace(correlationId))
            requestContextSetter.SetCorrelationId(correlationId);

        Resolve<IIdempotencyContextSetter>().Set(idempotencyKey, subscription);

        return true;
    }

    private bool IsInternalApiKeyValid()
    {
        var expected = Resolve<IConfiguration>()["OutboxWorker:InternalApiKey"];
        if (string.IsNullOrWhiteSpace(expected))
            return false;

        if (!TryGetStringHeader(InternalApiHeaders.ApiKey, out var actual))
            return false;

        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var actualBytes = Encoding.UTF8.GetBytes(actual);

        return expectedBytes.Length == actualBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }

    private bool TryGetGuidHeader(string name, out Guid value)
    {
        value = default;
        return TryGetStringHeader(name, out var raw) && Guid.TryParse(raw, out value);
    }

    private bool TryGetStringHeader(string name, out string value)
    {
        value = string.Empty;

        if (!HttpContext.Request.Headers.TryGetValue(name, out StringValues values))
            return false;

        value = values.ToString();
        return !string.IsNullOrWhiteSpace(value);
    }
}

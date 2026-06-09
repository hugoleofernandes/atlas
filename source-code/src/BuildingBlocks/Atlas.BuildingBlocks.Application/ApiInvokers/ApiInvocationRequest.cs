using System.Net.Http;

namespace Atlas.BuildingBlocks.Application.ApiInvokers;

public sealed record ApiInvocationRequest(
    string Name,
    HttpMethod Method,
    Uri Url,
    object Payload,
    Guid IdempotencyKey,
    Guid OutboxMessageId,
    string CorrelationId,
    string? TraceParent,
    Guid TenantId,
    Guid UserId,
    string? UserEmail);
